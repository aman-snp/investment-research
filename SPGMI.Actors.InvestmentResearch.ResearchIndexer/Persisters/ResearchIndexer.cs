using System;
using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using SPGMI.Actor.Interfaces.Bindings;
using SPGMI.Actors.InvestmentResearch.ResearchIndexer.Helpers;
using SPGMI.DataPipeline.Actors.Bindings.ContentAPI;
using SPGMI.DataPipeline.Actors.Persister;
using SPGMI.Pipeline;
using SPGMI.Pipeline.Content.Interfaces;
using SPGMI.Pipeline.Interfaces;
using SPGMI.Pipeline.ObjectSets;
using SPGMI.Logging;
using System.Diagnostics;
using System.Text;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexer
{
    internal class ResearchIndexer : PersisterActorBase<InvResrchPresentationObjectSet>
    {
        readonly int _maxRetryCount, _hqTimeOut;
        readonly string KafkaTopic;
        readonly string SearchApiKey;
        readonly AppSettings appSettings;
        readonly ISPGMILogger metricLogger;

        readonly Stopwatch TotalTimer;
        readonly Stopwatch holdTimer;
        readonly Stopwatch indexingtimer;
        readonly Stopwatch dependencyTimer;
        double holdTime = 0, indexingTime = 0, totalTime = 0, dependencyTime = 0;
        string objectsetKey = string.Empty;
        ODataClientHelper helper;
        #region Ctor

        public ResearchIndexer(string name, ISPGMILogger logger, IInputBinding<IPipelinePayload> inputBinding, IEnumerable<IOutputBinding> outputBinding, AppSettings actorConfig) : base(name, inputBinding, outputBinding)
        {
            KafkaTopic = actorConfig.KafkaTopic;
            SearchApiKey = actorConfig.SearchApiKey;
            _maxRetryCount = actorConfig.MaxRetryCount;
            _hqTimeOut = actorConfig.HoldingQueueAgeThreshold;
            appSettings = actorConfig;
            metricLogger = logger;
            helper = new ODataClientHelper(actorConfig.ODataEndpoint, actorConfig.ODataClientId, actorConfig.ODataAppKey, actorConfig.SecurityServiceUrl, this.Logger);

            TotalTimer = new Stopwatch();
            holdTimer = new Stopwatch();
            indexingtimer = new Stopwatch();
            dependencyTimer = new Stopwatch();
        }

        #endregion Ctor

        #region Override Methods
        protected override int HashFunction(IPipelinePayload input)
        {
            int.TryParse(((InvResrchPresentationObjectSet)input.Value).DocsFileCollPresentation_instance.ResearchDocumentID.SourceValue, out int researchDocumentId);
            return researchDocumentId;
        }

        public override void OnUnhandledException(Exception exception)
        {
            int i = 1;
            var exmessage = exception.Message + Environment.NewLine + " Stack Trace " + exception.StackTrace + Environment.NewLine;
            while (exception.InnerException != null)
            {
                exmessage += i.ToString() + Environment.NewLine + exception.InnerException.Message + Environment.NewLine + "StackTrace" + exception.InnerException.StackTrace + Environment.NewLine;
                exception = exception.InnerException;
                i++;
            }
            this.Logger.LogError($"Unhandled Exception {exmessage}");
        }

        public override IReadOnlyCollection<ObjectSet> Persist(IReadOnlyCollection<InvResrchPresentationObjectSet> input)
        {
            this.Logger.LogInformation($"Received batch input of size: {input.Count}", true);
            var batchOutput = new List<ObjectSet>();
            Persist(input);
            foreach (var item in input)
            {
                var output = this.Persist(item);
                if (output != null)
                    batchOutput.Add(output);
            }
            return batchOutput;
        }

        public override ObjectSet Persist(InvResrchPresentationObjectSet input)
        {
            holdTime = 0;
            indexingTime = 0;
            totalTime = 0;
            dependencyTime = 0;

            TotalTimer.Reset();
            dependencyTimer.Reset();
            holdTimer.Reset();
            indexingtimer.Reset();

            TotalTimer.Start();
            dependencyTimer.Start();
            objectsetKey = string.Empty;
            try
            {
                objectsetKey = input.OSIdentifier;
                if (input.Action == ActionType.DELETE && !input.DocsFileCollPresentation_instance.KeyFileCollection.HasValue)
                {
                    this.Logger.LogInformation($"Received Objectset with OSIdentifier {objectsetKey} {this.Input.MessageQueueCount.ToString()} Message Skipped as No KeyFileCollection Found from Objectset");
                    input.StateOverride = "INVALID_MESSAGE";
                    return input;
                }
                var keyFileCollection = input.DocsFileCollPresentation_instance.KeyFileCollection.Value;
                this.Logger.LogInformation($"Received Message with OSIdentifier {objectsetKey} KeyFileCollection: {keyFileCollection} with State {input.State} & StateOverride {input.StateOverride}");
                var pipelinepayload = base.GetPipelinePayload(input.OSIdentifier);
                if (input.State == ObjectSetStates.PERSISTENCE_SUCCESS || input.StateOverride.ToString().ToLower() == "REPLICATION_SUCCESS".ToLower())
                {
                    PublishJsontoIndex(input, keyFileCollection);
                    var osDependencyForKeyFileCollection = GetHQDependencyWithkeyFileCollection(input);
                    if (osDependencyForKeyFileCollection != null)
                        Hold(pipelinepayload, input, keyFileCollection, new List<ObjectSet>() { osDependencyForKeyFileCollection }, 1);
                    else
                    {
                        // Release from processing after max Retry for indexing
                        this.Logger.LogInformation($"OSIdentifier {objectsetKey} KeyFileCollection: {keyFileCollection} - Message Failed as No dependency to Hold and Indexing is disabled.");
                        return PublishtoEWA(input);
                    }
                }
                else if (input.State == ObjectSetStates.IN_HOLDINGQUEUE)
                {
                    // Message released from Holding Queue normally
                    if (pipelinepayload.HoldReleased)
                    {
                        if (appSettings.IndexerPostFixId == 104)
                        {
                            if (appSettings.ReplicationFirst)
                                return PublishOStoAlerts(input);
                            else
                                return PublishOSforReplicationCheck(input);
                        }
                        //// Message received for First time After released from HQ - Pass it to Alerts
                        //if (input.DocsFileCollPresentation_instance.TriggerEventID.Value == 1 && ValidateTimeSpan(input.DocsFileCollPresentation_instance.PresentationDate.Value))
                        //    return PublishOStoAlerts(input);

                        //// If released only after Indexing Success  and State of Replication is Unknown return with No Change and release
                        this.Logger.LogInformation($"OSIdentifier {input.OSIdentifier} KeyFileCollection: {keyFileCollection} - Message released from HQ after Indexing Success and Alert NA");
                        input.StateOverride = "ALERT_NA";
                        return input;

                    }

                    //// if released after 5 minutes from HQ , Then re-request for Indexing  and Hold for 5 times
                    long HoldCount = input.DocsFileCollPresentation_instance.TriggerEventID.Value;
                    if (HoldCount < _maxRetryCount && appSettings.IsIndexingEnabled)
                    {
                        PublishJsontoIndex(input, keyFileCollection);
                        //Only 1 dependency
                        var hqDependencyWithKeyCollection = GetHQDependencyWithkeyFileCollection(input);
                        Hold(pipelinepayload, input, keyFileCollection, new List<ObjectSet>() { hqDependencyWithKeyCollection }, HoldCount + 1);
                    }
                    else
                    {
                        // Release from processing after max Retry for indexing
                        this.Logger.LogInformation($"OSIdentifier {input.OSIdentifier} KeyFileCollection: {keyFileCollection} - Message Failed in Indexing after MAX Retry{HoldCount}");
                        return PublishtoEWA(input);
                    }
                }
            }
            catch (Exception ex)
            {
                int i = 1;
                var exmessage = ex.Message + Environment.NewLine + " Stack Trace " + ex.StackTrace + Environment.NewLine;
                while (ex.InnerException != null)
                {
                    exmessage += i.ToString() + Environment.NewLine + ex.InnerException.Message + Environment.NewLine + "StackTrace" + ex.InnerException.StackTrace + Environment.NewLine;
                    ex = ex.InnerException;
                    i++;
                }
                this.Logger.LogError($"Objectset Indexing failed for OSIdentifier: {input.OSIdentifier} and KeyFileCollection  {input.DocsFileCollPresentation_instance.KeyFileCollection.Value} with {exmessage}");
            }
            TotalTimer.Stop();
            totalTime = TotalTimer.Elapsed.TotalMilliseconds;

            var dictionary = new Dictionary<string, string>
            {
                { "IndexingDependencyGenerationTime.double", dependencyTime.ToString() },
                { "IndexingHoldTime.double", holdTime.ToString() },
                { "IndexingRequestTime.double", indexingTime.ToString() },
                { "IndexingTotalTime.double", totalTime.ToString() },
                { "IndexingOskey", objectsetKey }
            };
            metricLogger.TrackTrace($"Logging Metric Information", dictionary);
            return null;
        }

        #endregion

        #region Publish For State End

        ObjectSet PublishtoEWA(InvResrchPresentationObjectSet input)
        {
            //// This Full Functionality Needs to be implemented say Index response actor
            //// Listen on kafka message response from indexing Team and then if the result if success /failure, then 
            ////If Success then Push a message to release an object from Holding Queue
            //// if failure then release 
            ////
            //if (int.Parse(input.StateInfo) < 5)// maxretrycount)
            //{
            //    this.Logger.LogInformation($"OSIdentifier {input.OSIdentifier} KeyFileCollection: {keyFileCollection} - Inside Publish Json to index for Index Failed.");
            //    PublishJsontoIndex(input);
            //}
            //else
            //{
            //}

            this.Logger.LogInformation($"Message published to indexing Failed for KeyFileCollection {input.DocsFileCollPresentation_instance.KeyFileCollection.Value} , OSIdentifier {input.OSIdentifier} for timeout from Indexing Failure or Replication Failure");
            input.StateOverride = "INDEXING_FAILED";
            return input;
        }

        ObjectSet PublishOStoAlerts(InvResrchPresentationObjectSet input)
        {
            this.Logger.LogInformation($"OSIdentifier {input.OSIdentifier} KeyFileCollection: {input.DocsFileCollPresentation_instance.KeyFileCollection.Value} - Inside Publish OS to Alerts");
            input.StateOverride = appSettings.AlertReadySubject;
            return input;
        }
        ObjectSet PublishOSforReplicationCheck(InvResrchPresentationObjectSet input)
        {
            this.Logger.LogInformation($"OSIdentifier {input.OSIdentifier} KeyFileCollection: {input.DocsFileCollPresentation_instance.KeyFileCollection.Value} - Inside Publish OS to Replication Check");
            input.DocsFileCollPresentation_instance.TriggerEventID.Value = 1;
            input.StateOverride = "Indexing_Success";
            return input;
        }
        List<ObjectSet> GetHoldingQueueDependencies(InvResrchPresentationObjectSet input)
        {
            var holdingQueueDependencies = new List<ObjectSet>();

            // KeyFileCollection Dependency Item with Indexing
            var hqKeyFileCollectionDependency = GetHQDependencyWithkeyFileCollection(input);
            if (hqKeyFileCollectionDependency != null)
                holdingQueueDependencies.Add(hqKeyFileCollectionDependency);

            return holdingQueueDependencies;
        }

        ContentReadyObjectSet GetHQDependencyWithkeyFileCollection(InvResrchPresentationObjectSet input)
        {
            if (!appSettings.IsIndexingEnabled)
                return null;
            var contentReadyMIObject = this.GetNewInstance<ContentReadyObjectSet>();
            contentReadyMIObject.KeyObject.Value = appSettings.FileCollectionKeyObject;
            contentReadyMIObject.KeyItem.Value = appSettings.KeyFileCollectionKeyKeyItem;
            contentReadyMIObject.Namespace = "MI";
            //contentReadyMIObject.StateOverride = "IndexingComplete";
            contentReadyMIObject.State = ObjectSetStates.INITIALIZED;
            contentReadyMIObject.OID.Value = input.DocsFileCollPresentation_instance.KeyFileCollection.Value.ToString();

            return contentReadyMIObject;
        }

        ContentReadyObjectSet GetHQDependencyWithKeyFileVersion(InvResrchPresentationObjectSet input)
        {
            //If realtime, which means the data is in last 3 days then we care for replication in order to send for alerts
            //If backfill/Bulk/online(3 days older) we dont send alerts to client, so no need to check for replication 
            if (appSettings.IndexerPostFixId == 104)
            {
                //// Use this HQ condition only if Fileversion has change - Primary one only should be selected or all
                string mainFileKeyFileVersion = string.Empty;
                var mainFileKeyFileVersionEntity = input.DocsFileCollPresentation_instance.DocsFileVersion_instance.OfType<DocsFileVersion>().FirstOrDefault(version => version.FileURI.SourceValue.ToLower().Contains("researchroot") && !version.FileURI.SourceValue.ToLower().Contains(".syn") && version.KeyFileVersion.HasValue);
                if (mainFileKeyFileVersionEntity != null)
                    mainFileKeyFileVersion = mainFileKeyFileVersionEntity.KeyFileVersion.Value.ToUpper();
                if (!string.IsNullOrWhiteSpace(mainFileKeyFileVersion))
                {
                    var contentReadyMIObject1 = this.GetNewInstance<ContentReadyObjectSet>();
                    contentReadyMIObject1.KeyObject.Value = appSettings.FileVersionKeyObject;
                    contentReadyMIObject1.KeyItem.Value = appSettings.FileVersionKeyitem;
                    contentReadyMIObject1.Namespace = "MI";
                    contentReadyMIObject1.State = ObjectSetStates.INITIALIZED;
                    contentReadyMIObject1.OID.Value = mainFileKeyFileVersion;
                    return contentReadyMIObject1;
                }
            }
            return null;
        }

        void Hold(IPipelinePayload pipelinepayload, InvResrchPresentationObjectSet input, int keyFileCollection, List<ObjectSet> holdingQueueDependencies, long? holdCount)
        {
            dependencyTimer.Stop();
            dependencyTime = dependencyTimer.Elapsed.TotalMilliseconds;
            holdTimer.Start();
            int j = 0;
            while (j < appSettings.HoldRetryCount)
            {
                j++;
                // Add Retry Logic
                try
                {
                    input.State = ObjectSetStates.IN_HOLDINGQUEUE;
                    input.DocsFileCollPresentation_instance.TriggerEventID.Value = holdCount ?? 1;
                    input.StateInfo = string.Empty;
                    (this.Input as InputBinding<InvResrchPresentationObjectSet>).Hold(pipelinepayload, holdingQueueDependencies, _hqTimeOut, null);
                    this.Logger.LogInformation($"OSIdentifier {input.OSIdentifier} KeyFileCollection: {keyFileCollection} - Message Hold Successfully and Publish Json to index Started with {holdCount ?? 0}");
                    break;
                }
                catch (Exception ex)
                {
                    int i = 1;
                    var exmessage = ex.Message + Environment.NewLine + " Stack Trace " + ex.StackTrace + Environment.NewLine;
                    while (ex.InnerException != null)
                    {
                        exmessage += i.ToString() + Environment.NewLine + ex.InnerException.Message + Environment.NewLine + "StackTrace" + ex.InnerException.StackTrace + Environment.NewLine;
                        ex = ex.InnerException;
                        i++;
                    }
                    this.Logger.LogError($"Objectset Indexing failed for OSIdentifier: {input.OSIdentifier} and KeyFileCollection  {input.DocsFileCollPresentation_instance.KeyFileCollection.Value} with {exmessage}");
                }
            }
            holdTimer.Stop();
            holdTime = holdTimer.Elapsed.TotalMilliseconds;
        }

        bool ValidateTimeSpan(DateTime presentationDateTime)
        {
            TimeSpan timespan = DateTime.Now - presentationDateTime;
            return (timespan.TotalHours <= 72);
        }

        void deliveryhandler(DeliveryReport<Null, string> obj)
        {
            this.Logger.LogInformation(!obj.Error.IsError ? $"Delivered message to {obj.TopicPartitionOffset}" : $"Delivery Error: {obj.Error.Reason}");
        }
        #endregion Publish For State End

        #region Get Json from Objectset
        ResearchContent PrepareFullIndexingJson(InvResrchPresentationObjectSet input)
        {
            #region Indexing message Json generation
            var rcontent = new ResearchContent();
            var header = new HeaderData()
            {
                UpdateType = 0,
                ApiKey = SearchApiKey
            };

            var rdata = new Data
            {
                Action = input.Action == ActionType.DELETE ? "delete" : "add",
                //Presentation 
                KeyFileCollection = input.DocsFileCollPresentation_instance.KeyFileCollection.Value,
                Headline = input.DocsFileCollPresentation_instance.PresentationTitle.Value,
                ResearchReportDate = input.DocsFileCollPresentation_instance.PresentationDate.Value
            };

            if(header.UpdateType == 1)
                rdata.NewsWireHeadlineSortStr = input.DocsFileCollPresentation_instance.PresentationTitle.Value;

            //Todo : if No mainfile need to assign the KFCD of metadataroot .
            if (input.DocsFileCollPresentation_instance.KeyFileCollectionDetail.HasValue)// Need to remove this if condition after persister changes 
                rdata.KeyFileCollectionDetail = input.DocsFileCollPresentation_instance.KeyFileCollectionDetail.Value;

            //rdata.Url = "#investmentResearch/preview?id=" + rdata.KeyFileCollectionDetail.ToString();

            #region Sensitive information	
            var response = helper.GetSingleKeyValue("KeyCACSuppression", $"CACSuppressions?$filter=(KeyTable+eq+8514)and(OID+eq+'{rdata.KeyFileCollection}')and(UpdOperation+lt+2)&$select=KeyCACSuppression");
            if (!string.IsNullOrWhiteSpace(response))
            {
                rdata.SensitiveInformation = true;
            }
            else
                rdata.SensitiveInformation = false;
            this.Logger.LogInformation($"Checking for Sensitive Information {rdata.KeyFileCollection} and Sensitive Information {rdata.SensitiveInformation}");
            #endregion Sensitive information

            //rcontent.KeyInvestResearchReportType = input.DocsFileCollPresentation_instance.KeyInvestResearchReportType.Value;
            if (input.DocsFileCollPresentation_instance.KeyInvestResearchReportType.HasValue)
            {
                var reportTypeDerefrence = input.DocsFileCollPresentation_instance.KeyInvestResearchReportType.DereferencedValues.FirstOrDefault(x => x.Key == "InvestResearchReportType");
                if (reportTypeDerefrence.Value.Count > 0)
                    rdata.ResearchReportType = reportTypeDerefrence.Value[0]?.ToString();
                rdata.KeyInvestResearchReportType = new List<int>
                {
                    input.DocsFileCollPresentation_instance.KeyInvestResearchReportType.Value
                };
            }

            rdata.Synopsis = input.DocsFileCollPresentation_instance.FCPresentationSynopsis.HasValue ? input.DocsFileCollPresentation_instance.FCPresentationSynopsis.Value
                               : (input.DocsFileCollPresentation_instance.DocsFileCollectionAbstract_instance.FileAbstract.HasValue ? input.DocsFileCollPresentation_instance.DocsFileCollectionAbstract_instance.FileAbstract : string.Empty);

            rdata.lastUpdateDate = input.DocsFileCollPresentation_instance.DocumentLastUpdated.HasValue ? input.DocsFileCollPresentation_instance.DocumentLastUpdated.Value
                                : input.DocsFileCollPresentation_instance.PresentationDate.Value;
            rdata.UpdDate = input.DocsFileCollPresentation_instance.DocumentLastUpdated.HasValue ? input.DocsFileCollPresentation_instance.DocumentLastUpdated.Value : DateTime.Now;

            if (input.DocsFileCollPresentation_instance.Pages.HasValue)
                rdata.Pages = input.DocsFileCollPresentation_instance.Pages.Value;
            if (input.DocsFileCollPresentation_instance.KeyForeignLanguage.HasValue)
            {
                rdata.LanguageId = input.DocsFileCollPresentation_instance.KeyForeignLanguage.Value;
                var languageDerefrence = input.DocsFileCollPresentation_instance.KeyForeignLanguage.DereferencedValues.FirstOrDefault(x => x.Key == "ForeignLanguageShort");
                if (languageDerefrence.Value.Count > 0)
                    rdata.Language = languageDerefrence.Value[0]?.ToString();
            }


            foreach (DocsFileVersion version in input.DocsFileCollPresentation_instance.DocsFileVersion_instance)
            {
                // Todo : need to remove the hardcodes 
                //  mainfile
                if (version.FileURI.SourceValue.ToLower().Contains("researchroot") && !version.FileURI.SourceValue.ToLower().Contains(".syn") && version.KeyFileVersion.HasValue)
                {

                    rdata.KeyFileVersion = version.KeyFileVersion.Value;
                    if (version.KeyFileFormat.HasValue)
                        rdata.KeyFileFormat = version.KeyFileFormat.Value;
                    rdata.BytesExtended = version.BytesExtended.Value;
                }

                //Solr text file 
                if (version.KeyFileFormat.Value == 0 && !version.FileURI.SourceValue.ToLower().Contains(".syn") && version.KeyFileFormat.HasValue && version.KeyFileVersion.HasValue)
                {
                    rdata.TextKeyFileVersion = version.KeyFileVersion.Value;
                    rdata.NonCodexTextFileVersion = version.KeyFileVersion.Value;
                    //mdata.isTextVersion = true;
                }
            }

            GetResearchlink(input, ref rdata);
            GetCategory(input, ref rdata);
            GetRelations(input, ref rdata);
            GetProducts(input, ref rdata);
            GetMIIndustry(input, ref rdata);
            GetCIQIndustry(input, ref rdata);
            GetGeography(input, ref rdata);
            rdata.id = "researchreport_" + rdata.KeyFileCollection;
            rdata.Url = "#investmentResearch/preview?id=" + rdata.KeyFileCollection.ToString();

            header.FilePrimaryKey = !string.IsNullOrEmpty(rdata.TextKeyFileVersion) ? rdata.TextKeyFileVersion : rdata.KeyFileVersion;
            if (!string.IsNullOrWhiteSpace(header.FilePrimaryKey))
            {
                header.Udr = new List<UDR>
                {
                    new UDR()
                };
            }
            header.IssueId = input.OSIdentifier;
            header.RevisionId = input.OSIdentifier;
            header.UpdateType = 0;
            header.DocId = rdata.id;
            rcontent.headers = header;
            rcontent.data = rdata;
            #endregion
            return rcontent;
        }
        ResearchIndustryContent PrepareIndustryJson(InvResrchPresentationObjectSet input)
        {
            //ResearchIndustryContent researchIndustryContent = new ResearchIndustryContent();
            int KeyFileCollection;
            KeyFileCollection = input.DocsFileCollPresentation_instance.KeyFileCollection.Value;
            #region Data
            var rdata = new ResearchIndustryData();
            rdata.id = "researchreport_" + KeyFileCollection.ToString();
            (rdata.Industry, rdata.IndustryName) = ExtractMIIndustryFromInput(input);
            (rdata.GicsIndustry, rdata.GicsIndustryName) = ExtractCIQIndustryFromInput(input);
            #endregion

            #region Headerdata
            var researchIndustryContent = new ResearchIndustryContent();
            var headerData = new IndustryHeaderData()
            {
                UpdateType = 1,
                ApiKey = SearchApiKey
            };
            //headerData.FilePrimaryKey = !string.IsNullOrEmpty(rdata.TextKeyFileVersion) ? rdata.TextKeyFileVersion : rdata.KeyFileVersion;
            //if (!string.IsNullOrWhiteSpace(headerData.FilePrimaryKey))
            //{
            //    headerData.Udr = new List<UDR>
            //    {
            //        new UDR()
            //    };
            //}
            headerData.IssueId = input.OSIdentifier;
            headerData.RevisionId = input.OSIdentifier;
            headerData.DocId = rdata.id;
            #endregion
            
            researchIndustryContent.headers = headerData;
            researchIndustryContent.data = rdata;
            return researchIndustryContent;
        }
        bool IsLinkBack(InvResrchPresentationObjectSet input)
        {
            var milinkback = new List<int>();
            foreach (var storobj in input.DocsFileCollPresentation_instance.DocsFileCollResearchLinkMIObject_instance.OfType<DocsFileCollResearchLinkMIObject>().Where(x => x.KeyInvestResearchStorageType.HasValue && x.KeyFileCollectionResearchLink != null).ToList())
            {
                milinkback.Add(storobj.KeyInvestResearchStorageType.Value);
            }
            //CASE WHEN ll.MaskInvestResearchStorageType IN ( 1 ,3 ,29 ) THEN 1 ELSE 0 END AS IsLinkBack ,CASE WHEN ll.MaskInvestResearchStorageType IN (3) THEN 1 ELSE 0 END AS IsRemotelyHosted
            long maskedvalue = MaskCalculation(milinkback);
            return (maskedvalue == 1 || maskedvalue == 3 || maskedvalue == 29);
        }
        void PublishJsontoIndex(InvResrchPresentationObjectSet input, int keyFileCollection)
        {
            string message = string.Empty;
            string headerJson = string.Empty;
            Confluent.Kafka.Headers header = new Headers();

            indexingtimer.Start();
            if (!appSettings.IsIndexingEnabled)
                return;
            int changeType = input.DocsFileCollPresentation_instance.KeyPipelineChangeType.Value;

            //Replace below if statement with switch when implementing partial index for other change types
            if (appSettings.EnablePartialIndex && Helpers.Constants.RefreshEntities.Contains(changeType))
            {
                var rIndustryContent = PrepareIndustryJson(input);
                message = DataConverters.ToJSONString(rIndustryContent.data);
                //header.Add("FilePrimaryKey", Encoding.Default.GetBytes(string.Empty));
                //header.Add("Udr", Encoding.Default.GetBytes(DataConverters.ToJSONString(string.Empty)));
                header.Add("IssueId", Encoding.Default.GetBytes(rIndustryContent.headers.IssueId));
                header.Add("RevisionId", Encoding.Default.GetBytes(rIndustryContent.headers.RevisionId));
                header.Add("UpdateType", Encoding.Default.GetBytes(rIndustryContent.headers.UpdateType.ToString()));
                header.Add("ApiKey", Encoding.Default.GetBytes(rIndustryContent.headers.ApiKey));
                header.Add("DocId", Encoding.Default.GetBytes(rIndustryContent.headers.DocId));
                header.Add("PreferredCollection", Encoding.Default.GetBytes(getPreferredCollection(keyFileCollection)));
            }
            else
            {
                var rcontent = PrepareFullIndexingJson(input);
                message = DataConverters.ToJSONString(rcontent.data);
                header.Add("FilePrimaryKey", Encoding.Default.GetBytes(rcontent.headers.FilePrimaryKey));
                if (rcontent.headers.Udr != null) {
                    // header.Add("Udr", Encoding.Default.GetBytes(rcontent.headers.udr.ToString()));
                    header.Add("Udr", Encoding.Default.GetBytes(DataConverters.ToJSONString(rcontent.headers.Udr)));
                }
                header.Add("IssueId", Encoding.Default.GetBytes(rcontent.headers.IssueId));
                header.Add("RevisionId", Encoding.Default.GetBytes(rcontent.headers.RevisionId));
                header.Add("UpdateType", Encoding.Default.GetBytes(rcontent.headers.UpdateType.ToString()));
                header.Add("ApiKey", Encoding.Default.GetBytes(rcontent.headers.ApiKey));
                header.Add("DocId", Encoding.Default.GetBytes(rcontent.headers.DocId));
                header.Add("PreferredCollection", Encoding.Default.GetBytes(getPreferredCollection(keyFileCollection)));
            }

            this.Logger.LogInformation($"Publish message to Kafka for KeyFileCollection {input.DocsFileCollPresentation_instance.KeyFileCollection.Value} The Json : {message}");
            KafkaPublisher.producer.Produce(KafkaTopic, new Message<Null, string>() { Value = message, Headers = header }, deliveryhandler);
            indexingtimer.Stop();
            indexingTime = indexingtimer.Elapsed.TotalMilliseconds;
        }


        void GetResearchlink(InvResrchPresentationObjectSet input, ref Data rcontent)
        {
            try
            {
                var milinkback = new List<int>();
                this.Logger.LogInformation($"KeyFileCollection: {rcontent.KeyFileCollection} - Inside GetResearchlink");
                foreach (var storobj in input.DocsFileCollPresentation_instance.DocsFileCollResearchLinkMIObject_instance.OfType<DocsFileCollResearchLinkMIObject>().Where(x => x.KeyInvestResearchStorageType.HasValue && x.KeyFileCollectionResearchLink != null).ToList())
                {
                    if (storobj.KeyInvestResearchStorageType.Value == 0)
                        rcontent.WebsiteURLExtended = storobj.WebsiteURLExtended.Value;
                    else if (storobj.KeyInvestResearchStorageType.Value == 2)
                        rcontent.BrokerHosted_AMR = storobj.WebsiteURLExtended.Value;
                    else if (storobj.KeyInvestResearchStorageType.Value == 3)
                        rcontent.PrintCommand_RealTime = storobj.WebsiteURLExtended.Value;
                    else if (storobj.KeyInvestResearchStorageType.Value == 4)
                        rcontent.PrintCommand_AMR = storobj.WebsiteURLExtended.Value;

                    milinkback.Add(storobj.KeyInvestResearchStorageType.Value);
                }
                //CASE WHEN ll.MaskInvestResearchStorageType IN ( 1 ,3 ,29 ) THEN 1 ELSE 0 END AS IsLinkBack ,CASE WHEN ll.MaskInvestResearchStorageType IN (3) THEN 1 ELSE 0 END AS IsRemotelyHosted
                long maskedvalue = MaskCalculation(milinkback);
                rcontent.IsLinkBack = (maskedvalue == 1 || maskedvalue == 3 || maskedvalue == 29);
                rcontent.IsRemotelyHosted = (maskedvalue == 3);
            }
            catch (Exception exception)
            {
                int i = 1;
                var exmessage = exception.Message + Environment.NewLine + " Stack Trace " + exception.StackTrace + Environment.NewLine;
                while (exception.InnerException != null)
                {
                    exmessage += i.ToString() + Environment.NewLine + exception.InnerException.Message + Environment.NewLine + "StackTrace" + exception.InnerException.StackTrace + Environment.NewLine;
                    exception = exception.InnerException;
                    i++;
                }
                this.Logger.LogError($"Unhandled Exception {exmessage}");
            }
        }

        void GetCategory(InvResrchPresentationObjectSet input, ref Data rcontent)
        {
            try
            {
                var catdict = new Dictionary<string, string>();
                this.Logger.LogInformation($"KeyFileCollection: {rcontent.KeyFileCollection} - Inside GetCategory  For Category");
                foreach (InvestResearchCat Cat in input.DocsFileCollPresentation_instance.InvestResearchCat_instance.OfType<InvestResearchCat>().Where(x => x.KeyInvestResearchCategory.HasValue))
                {
                    if (Cat.KeyInvestResearchCategory.DereferencedValues.Any() && Cat.KeyInvestResearchCategory.DereferencedValues.ElementAt(0).Value.Count() > 0 && !catdict.ContainsKey(Cat.KeyInvestResearchCategory.DereferencedValues.ElementAt(0).Value[0].ToString()))
                        catdict.Add(Cat.KeyInvestResearchCategory.DereferencedValues.ElementAt(0).Value[0]?.ToString(), Cat.KeyInvestResearchCategory.DereferencedValues.ElementAt(1).Value[0]?.ToString());
                }

                foreach (InvestResearchAppr appr in input.DocsFileCollPresentation_instance.InvestResearchAppr_instance.OfType<InvestResearchAppr>().Where(x => x.KeyInvestResearchApproach.HasValue))
                {
                    if (appr.KeyInvestResearchApproach.DereferencedValues.Any() && appr.KeyInvestResearchApproach.DereferencedValues.ElementAt(0).Value.Count() > 0 && !catdict.ContainsKey(appr.KeyInvestResearchApproach.DereferencedValues.ElementAt(0).Value[0].ToString()))
                        catdict.Add(appr.KeyInvestResearchApproach.DereferencedValues.ElementAt(0).Value[0]?.ToString(), appr.KeyInvestResearchApproach.DereferencedValues.ElementAt(1).Value[0]?.ToString());
                }

                foreach (InvestResearchFocus focus in input.DocsFileCollPresentation_instance.InvestResearchFocus_instance.OfType<InvestResearchFocus>().Where(x => x.KeyInvestResearchFocus.HasValue))
                {
                    if (focus.KeyInvestResearchFocus.DereferencedValues.Any() && focus.KeyInvestResearchFocus.DereferencedValues.ElementAt(0).Value.Count() > 0 && !catdict.ContainsKey(focus.KeyInvestResearchFocus.DereferencedValues.ElementAt(0).Value[0]?.ToString()))
                        catdict.Add(focus.KeyInvestResearchFocus.DereferencedValues.ElementAt(0).Value[0]?.ToString(), focus.KeyInvestResearchFocus.DereferencedValues.ElementAt(1).Value[0]?.ToString());
                }

                foreach (InvestResearchEvent researchevent in input.DocsFileCollPresentation_instance.InvestResearchEvent_instance.OfType<InvestResearchEvent>().Where(x => x.KeyInvestResearchEvent.HasValue))
                {
                    if (researchevent.KeyInvestResearchEvent.DereferencedValues.Any() && researchevent.KeyInvestResearchEvent.DereferencedValues.ElementAt(0).Value.Count() > 0 && !catdict.ContainsKey(researchevent.KeyInvestResearchEvent.DereferencedValues.ElementAt(0).Value[0]?.ToString()))
                        catdict.Add(researchevent.KeyInvestResearchEvent.DereferencedValues.ElementAt(0).Value[0]?.ToString(), researchevent.KeyInvestResearchEvent.DereferencedValues.ElementAt(1).Value[0]?.ToString());
                }

                foreach (InvestResearchSubj subj in input.DocsFileCollPresentation_instance.InvestResearchSubj_instance.OfType<InvestResearchSubj>().Where(x => x.KeyInvestResearchSubject.HasValue))
                {
                    if (subj.KeyInvestResearchSubject.DereferencedValues.Any() && subj.KeyInvestResearchSubject.DereferencedValues.ElementAt(0).Value.Count() > 0 && !catdict.ContainsKey(subj.KeyInvestResearchSubject.DereferencedValues.ElementAt(0).Value[0]?.ToString()))
                        catdict.Add(subj.KeyInvestResearchSubject.DereferencedValues.ElementAt(0).Value[0]?.ToString(), subj.KeyInvestResearchSubject.DereferencedValues.ElementAt(1).Value[0]?.ToString());
                }

                foreach (InvestResearchType researchtype in input.DocsFileCollPresentation_instance.InvestResearchType_instance.OfType<InvestResearchType>().Where(x => x.KeyInvestResearchType.HasValue))
                {
                    if (researchtype.KeyInvestResearchType.DereferencedValues.Any() && researchtype.KeyInvestResearchType.DereferencedValues.ElementAt(0).Value.Count() > 0 && !catdict.ContainsKey(researchtype.KeyInvestResearchType.DereferencedValues.ElementAt(0).Value[0]?.ToString()))
                        catdict.Add(researchtype.KeyInvestResearchType.DereferencedValues.ElementAt(0).Value[0]?.ToString(), researchtype.KeyInvestResearchType.DereferencedValues.ElementAt(1).Value[0]?.ToString());
                }

                rcontent.Category = catdict.Keys.ToList();
                rcontent.CategoryName = catdict.Values.ToList();
                this.Logger.LogInformation($"KeyFileCollection: {rcontent.KeyFileCollection} - Inside GetCategory  Final List of category {rcontent.Category} with Values {rcontent.CategoryName}");
            }
            catch (Exception exception)
            {
                int i = 1;
                var exmessage = exception.Message + Environment.NewLine + " Stack Trace " + exception.StackTrace + Environment.NewLine;
                while (exception.InnerException != null)
                {
                    exmessage += i.ToString() + Environment.NewLine + exception.InnerException.Message + Environment.NewLine + "StackTrace" + exception.InnerException.StackTrace + Environment.NewLine;
                    exception = exception.InnerException;
                    i++;
                }
                this.Logger.LogError($"Unhandled Exception {exmessage}");
            }
        }

        void GetRelations(InvResrchPresentationObjectSet input, ref Data rcontent)
        {
            try
            {
                //var result =GetCompanyMapping(input);
                List<int> lstanalystId = new List<int>();
                var lstanalyst = new List<string>();
                var lstcompanyId = new List<int>();
                var lstcompany = new List<string>();
                this.Logger.LogInformation($"KeyFileCollection: {rcontent.KeyFileCollection} - Inside GetRelations ");
                foreach (DocsFileCollRelation rel in input.DocsFileCollPresentation_instance.DocsFileCollRelation_instance)
                {
                    if (rel.KeyDocRelationshipType.Value == 4 && rel.KeyInstn.HasValue && rel.KeyInstn.DereferencedValues.ContainsKey("InstnName") && rel.KeyInstn.DereferencedValues.FirstOrDefault(x => x.Key == "InstnName").Value.Count() > 0)//Company Mapped by Contributor
                    {
                        if (!rel.InsignificantReference.Value)
                        {
                            rcontent.PrimaryCompanyId = rel.KeyInstn.Value;
                            rcontent.PrimaryCompany = rel.KeyInstn.DereferencedValues.ElementAt(0).Value[0]?.ToString();
                            lstcompanyId.Add(rcontent.PrimaryCompanyId);
                            lstcompany.Add(rcontent.PrimaryCompany.Trim());
                        }
                        else
                        {
                            lstcompanyId.Add(rel.KeyInstn.Value);
                            lstcompany.Add(rel.KeyInstn.DereferencedValues.ElementAt(0).Value[0]?.ToString().Trim());
                        }
                    }

                    if (rel.KeyDocRelationshipType.Value == 3)//Contributor  
                    {
                        //ContributorName 
                        var contributorNameDereference = rel.DocsFileCollRelationContributorMIObject_instance.KeyInstn.DereferencedValues.FirstOrDefault(x => x.Key == "ResearchContributorName");
                        if (contributorNameDereference.Value.Count > 0)
                            rcontent.ResearchContributor = contributorNameDereference.Value[0].ToString();
                        // rel.KeyInstn.DereferencedValues.ElementAt(0).Value[0].ToString();
                        rcontent.ResearchContributorId = rel.DocsFileCollRelationContributorMIObject_instance.KeyInstn.Value;
                        var contributorDereference = rel.DocsFileCollRelationContributorMIObject_instance.KeyInstn.DereferencedValues.FirstOrDefault(x => x.Key == "ResearchContributorStatus");
                        if (contributorDereference.Value.Count > 0)
                            rcontent.ResearchContributorStatus = contributorDereference.Value[0].ToString();
                        //instsn.ResearchContributorStatus != null ? instsn.ResearchContributorStatus.ResearchContributorStatus : string.Empty;
                        var contributorTypeDereference = rel.DocsFileCollRelationContributorMIObject_instance.KeyInstn.DereferencedValues.FirstOrDefault(x => x.Key == "InvestResearchContributorType");
                        if (contributorTypeDereference.Value.Count > 0)
                            rcontent.ResearchContributorType = contributorTypeDereference.Value[0].ToString();
                        //instsn.InstnInternalUseOnlys != null && instsn.InstnInternalUseOnlys.FirstOrDefault().InvestResearchContribType != null ? instsn.InstnInternalUseOnlys.FirstOrDefault().InvestResearchContribType.InvestResearchContributorType : string.Empty;
                    }

                    //Analyst and Primary Analyst 
                    if (rel.KeyDocRelationshipType.Value == 0 && rel.KeyPerson.HasValue && rel.KeyPerson.DereferencedValues.ContainsKey("BestPersonName") && rel.KeyPerson.DereferencedValues.FirstOrDefault(x => x.Key == "BestPersonName").Value.Count() > 0)
                    {
                        if (!rel.InsignificantReference.Value)
                        {
                            rcontent.PrimaryAnalystId = rel.KeyPerson.Value.ToString();
                            if (rel.KeyPerson.DereferencedValues.FirstOrDefault(x => x.Key == "BestPersonName").Value.Count > 0)
                            {
                                rcontent.PrimaryAnalyst = rel.KeyPerson.DereferencedValues.FirstOrDefault(x => x.Key == "BestPersonName").Value[0]?.ToString();
                                lstanalyst.Add(rcontent.PrimaryAnalyst);
                            }
                            lstanalystId.Add(rel.KeyPerson.Value);

                        }
                        else
                        {
                            lstanalystId.Add(rel.KeyPerson.Value);
                            if (rel.KeyPerson.DereferencedValues.FirstOrDefault(x => x.Key == "BestPersonName").Value.Count > 0)
                                lstanalyst.Add(rel.KeyPerson.DereferencedValues.FirstOrDefault(x => x.Key == "BestPersonName").Value[0]?.ToString());
                        }
                    }
                    // Team
                    if (rel.KeyDocRelationshipType.Value == 0 && rel.KeyInstnResearchTeam.HasValue && rel.KeyInstnResearchTeam.DereferencedValues != null && rel.KeyInstnResearchTeam.DereferencedValues.ContainsKey("InstnResearchTeamName") && rel.KeyInstnResearchTeam.DereferencedValues.FirstOrDefault(x => x.Key == "InstnResearchTeamName").Value.Count() > 0)
                    {
                        rcontent.IsTeam = true;
                        if (!rel.InsignificantReference.Value)
                        {
                            rcontent.PrimaryAnalystId = rel.KeyInstnResearchTeam.Value.ToString();
                            lstanalystId.Add(rel.KeyInstnResearchTeam);
                            if (rel.KeyInstnResearchTeam.DereferencedValues.FirstOrDefault(x => x.Key == "InstnResearchTeamName").Value.Count > 0)
                            {
                                rcontent.PrimaryAnalyst = rel.KeyInstnResearchTeam.DereferencedValues.FirstOrDefault(x => x.Key == "InstnResearchTeamName").Value[0]?.ToString();
                                lstanalyst.Add(rcontent.PrimaryAnalyst);
                            }
                        }
                        else
                        {
                            lstanalystId.Add(rel.KeyInstnResearchTeam.Value);

                            if (rel.KeyInstnResearchTeam.DereferencedValues.FirstOrDefault(x => x.Key == "InstnResearchTeamName").Value.Count > 0)
                                lstanalyst.Add(rel.KeyInstnResearchTeam.DereferencedValues.FirstOrDefault(x => x.Key == "InstnResearchTeamName").Value[0]?.ToString());
                        }
                    }
                }
                if (lstanalystId.Count() > 0)
                    rcontent.ResearchAnalystId = lstanalystId;
                if (lstanalyst.Count() > 0)
                    rcontent.ResearchAnalyst = String.Join(",", lstanalyst);
                if (lstcompanyId.Count() > 0)
                    rcontent.CompanyIds = lstcompanyId;
                if (lstcompany.Count() > 0)
                    rcontent.Companies = String.Join(", ", lstcompany);
            }
            catch (Exception exception)
            {
                int i = 1;
                var exmessage = exception.Message + Environment.NewLine + " Stack Trace " + exception.StackTrace + Environment.NewLine;
                while (exception.InnerException != null)
                {
                    exmessage += i.ToString() + Environment.NewLine + exception.InnerException.Message + Environment.NewLine + "StackTrace" + exception.InnerException.StackTrace + Environment.NewLine;
                    exception = exception.InnerException;
                    i++;
                }
                this.Logger.LogError($"Unhandled Exception {exmessage}");
            }
        }

        void GetProducts(InvResrchPresentationObjectSet input, ref Data rcontent)
        {
            try
            {
                var prodId = new List<int>();
                var fileprice = new List<decimal>();
                this.Logger.LogInformation($"KeyFileCollection: {rcontent.KeyFileCollection} - Inside GetProducts ");
                foreach (InvestResearchProductFileCollection product in input.DocsFileCollPresentation_instance.InvestResearchProductFileCollection_instance)
                {
                    prodId.Add(product.KeyCIQIRProduct.Value);
                    fileprice.Add(product.FileCollectionPrice.Value);
                }
                rcontent.KeyCIQIRProduct = prodId;
                rcontent.FileCollectionPrice = fileprice;
            }
            catch (Exception exception)
            {
                int i = 1;
                var exmessage = exception.Message + Environment.NewLine + " Stack Trace " + exception.StackTrace + Environment.NewLine;
                while (exception.InnerException != null)
                {
                    exmessage += i.ToString() + Environment.NewLine + exception.InnerException.Message + Environment.NewLine + "StackTrace" + exception.InnerException.StackTrace + Environment.NewLine;
                    exception = exception.InnerException;
                    i++;
                }
                this.Logger.LogError($"Unhandled Exception {exmessage}");
            }
        }
        (string Industry, string IndustryName) ExtractMIIndustryFromInput(InvResrchPresentationObjectSet input)
        {
            string Industry = string.Empty, IndustryName = string.Empty;
            try
            {
                // this.Logger.LogInformation($"KeyFileCollection: {rcontent.KeyFileCollection} - Inside GetMIIndustry");
                foreach (var indus in input.DocsFileCollPresentation_instance.DocsFileCollIndustry_instance.OfType<DocsFileCollIndustry>().Where(x => x.KeyMIIndustryTree.HasValue && x.KeyMIIndustryTree.SourceValue != null))
                {
                    if (indus.KeyMIIndustryTree.DereferencedValues.Any() && indus.KeyMIIndustryTree.DereferencedValues.ElementAt(0).Value.Count() > 0)
                    {
                        Industry = (Industry == string.Empty ? Industry : Industry + ", ");
                        IndustryName = (IndustryName == string.Empty ? IndustryName : IndustryName + ", ");
                        Industry += indus.KeyMIIndustryTree.Value.ToString();
                        //rel.KeyInstn.HasValue && rel.KeyInstn.DereferencedValues.ElementAt(0).Value.Count() > 0
                        IndustryName += indus.KeyMIIndustryTree.DereferencedValues.ElementAt(0).Value[0]?.ToString();
                    }
                }
            }
            catch (Exception exception)
            {
                int i = 1;
                var exmessage = exception.Message + Environment.NewLine + " Stack Trace " + exception.StackTrace + Environment.NewLine;
                while (exception.InnerException != null)
                {
                    exmessage += i.ToString() + Environment.NewLine + exception.InnerException.Message + Environment.NewLine + "StackTrace" + exception.InnerException.StackTrace + Environment.NewLine;
                    exception = exception.InnerException;
                    i++;
                }
                this.Logger.LogError($"Unhandled Exception {exmessage}");
            }
            return (Industry, IndustryName);
        }
        (string GicsIndustry, string GicsIndustryName) ExtractCIQIndustryFromInput(InvResrchPresentationObjectSet input)
        {
            string GicsIndustry = string.Empty, GicsIndustryName = string.Empty;
            try
            {
                // this.Logger.LogInformation($"KeyFileCollection: {rcontent.KeyFileCollection} - Inside GetMIIndustry");
                foreach (var indus in input.DocsFileCollPresentation_instance.DocFileCollCIQIndustryMIObject_instance.OfType<DocFileCollCIQIndustryMIObject>().Where(x => x.KeyCIQIndustryTree.HasValue && x.KeyCIQIndustryTree.SourceValue != null))
                {
                    if (indus.KeyCIQIndustryTree.DereferencedValues.Any() && indus.KeyCIQIndustryTree.DereferencedValues.ElementAt(0).Value.Count() > 0)
                    {
                        GicsIndustry = (GicsIndustry == string.Empty ? GicsIndustry : GicsIndustry + ", ");
                        GicsIndustryName = (GicsIndustryName == string.Empty ? GicsIndustryName : GicsIndustryName + ", ");
                        GicsIndustry += indus.KeyCIQIndustryTree.Value.ToString();
                        //rel.KeyInstn.HasValue && rel.KeyInstn.DereferencedValues.ElementAt(0).Value.Count() > 0
                        GicsIndustryName += indus.KeyCIQIndustryTree.DereferencedValues.ElementAt(0).Value[0]?.ToString();
                    }
                }
            }
            catch (Exception exception)
            {
                int i = 1;
                var exmessage = exception.Message + Environment.NewLine + " Stack Trace " + exception.StackTrace + Environment.NewLine;
                while (exception.InnerException != null)
                {
                    exmessage += i.ToString() + Environment.NewLine + exception.InnerException.Message + Environment.NewLine + "StackTrace" + exception.InnerException.StackTrace + Environment.NewLine;
                    exception = exception.InnerException;
                    i++;
                }
                this.Logger.LogError($"Unhandled Exception {exmessage}");
            }
            return (GicsIndustry, GicsIndustryName);
        }
        void GetMIIndustry(InvResrchPresentationObjectSet input, ref Data rcontent)
        {
            try
            {
                this.Logger.LogInformation($"KeyFileCollection: {rcontent.KeyFileCollection} - Inside GetMIIndustry");
                //foreach (var indus in input.DocsFileCollPresentation_instance.DocsFileCollIndustry_instance.OfType<DocsFileCollIndustry>().Where(x => x.KeyMIIndustryTree.HasValue && x.KeyMIIndustryTree.SourceValue != null))
                //{
                //    if (indus.KeyMIIndustryTree.DereferencedValues.Any() && indus.KeyMIIndustryTree.DereferencedValues.ElementAt(0).Value.Count() > 0)
                //    {
                //        rcontent.Industry += indus.KeyMIIndustryTree.Value.ToString() + ",";
                //        //rel.KeyInstn.HasValue && rel.KeyInstn.DereferencedValues.ElementAt(0).Value.Count() > 0
                //        rcontent.IndustryName = rcontent.IndustryName + indus.KeyMIIndustryTree.DereferencedValues.ElementAt(0).Value[0]?.ToString() + ",";
                //    }
                //}
                (rcontent.Industry, rcontent.IndustryName) = ExtractMIIndustryFromInput(input);

            }
            catch (Exception exception)
            {
                int i = 1;
                var exmessage = exception.Message + Environment.NewLine + " Stack Trace " + exception.StackTrace + Environment.NewLine;
                while (exception.InnerException != null)
                {
                    exmessage += i.ToString() + Environment.NewLine + exception.InnerException.Message + Environment.NewLine + "StackTrace" + exception.InnerException.StackTrace + Environment.NewLine;
                    exception = exception.InnerException;
                    i++;
                }
                this.Logger.LogError($"Unhandled Exception {exmessage}");
            }
        }

        void GetCIQIndustry(InvResrchPresentationObjectSet input, ref Data rcontent)
        {
            try
            {
                this.Logger.LogInformation($"KeyFileCollection: {rcontent.KeyFileCollection} - Inside GetCIQIndustry");
                //foreach (var indus in input.DocsFileCollPresentation_instance.DocFileCollCIQIndustryMIObject_instance.OfType<DocFileCollCIQIndustryMIObject>().Where(x => x.KeyCIQIndustryTree.HasValue && x.KeyCIQIndustryTree.SourceValue != null))
                //{
                //    if (indus.KeyCIQIndustryTree.DereferencedValues.Any() && indus.KeyCIQIndustryTree.DereferencedValues.ElementAt(0).Value.Count() > 0)
                //    {
                //        rcontent.GicsIndustry += indus.KeyCIQIndustryTree.Value.ToString() + ",";
                //        //rel.KeyInstn.HasValue && rel.KeyInstn.DereferencedValues.ElementAt(0).Value.Count() > 0
                //        rcontent.GicsIndustryName = rcontent.GicsIndustryName + indus.KeyCIQIndustryTree.DereferencedValues.ElementAt(0).Value[0]?.ToString() + ",";
                //    }
                //}
                (rcontent.GicsIndustry, rcontent.GicsIndustryName) = ExtractCIQIndustryFromInput(input);
            }
            catch (Exception exception)
            {
                int i = 1;
                var exmessage = exception.Message + Environment.NewLine + " Stack Trace " + exception.StackTrace + Environment.NewLine;
                while (exception.InnerException != null)
                {
                    exmessage += i.ToString() + Environment.NewLine + exception.InnerException.Message + Environment.NewLine + "StackTrace" + exception.InnerException.StackTrace + Environment.NewLine;
                    exception = exception.InnerException;
                    i++;
                }
                this.Logger.LogError($"Unhandled Exception {exmessage}");
            }
        }

        void GetGeography(InvResrchPresentationObjectSet input, ref Data rcontent)
        {
            try
            {
                var lstgeo = new List<string>();
                var lstgeoids = new List<string>();
                this.Logger.LogInformation($"KeyFileCollection: {rcontent.KeyFileCollection} - Inside GetGeography");
                foreach (var geo in input.DocsFileCollPresentation_instance.DocsFileCollGeo_instance.OfType<DocsFileCollGeo>().Where(x => x.KeyGeographyTree.HasValue))
                {
                    if (geo.KeyGeographyTree.DereferencedValues.ContainsKey("ProductCaption") && geo.KeyGeographyTree.DereferencedValues.FirstOrDefault(x => x.Key == "ProductCaption").Value.Count > 0 && geo.KeyGeographyTree.DereferencedValues.FirstOrDefault(x => x.Key == "ProductCaption").Value.Any(y => y != null))
                    {
                        //rcontent.PrimaryGeography += geo.KeyGeographyTree.DereferencedValues.FirstOrDefault(x => x.Key == "ProductCaption").Value.FirstOrDefault(y => y != null) + ", ";
                        lstgeo.Add(geo.KeyGeographyTree.DereferencedValues.FirstOrDefault(x => x.Key == "ProductCaption").Value.FirstOrDefault(y => y != null).ToString());
                        lstgeoids.Add(geo.KeyGeographyTree.Value.ToString());
                    }
                }
                //rcontent.PrimaryGeography = lstgeo.Count() > 0 ? lstgeo : null;//lstgeoids.Count() > 0 ? lstgeoids : null;
                if (lstgeo.Any())
                    rcontent.PrimaryGeography = String.Join(", ", lstgeo);
                if (lstgeoids.Any())
                    rcontent.KeyGeographyTree = String.Join(", ", lstgeoids);
            }
            catch (Exception exception)
            {
                var exmessage = string.Empty;
                int i = 1;
                exmessage = exception.Message + Environment.NewLine + " Stack Trace " + exception.StackTrace + Environment.NewLine;
                while (exception.InnerException != null)
                {
                    exmessage += i.ToString() + Environment.NewLine + exception.InnerException.Message + Environment.NewLine + "StackTrace" + exception.InnerException.StackTrace + Environment.NewLine;
                    exception = exception.InnerException;
                    i++;
                }
                this.Logger.LogError($"GetGeography {exmessage}");
            }
        }

        long MaskCalculation(List<int> maskfields)
        {
            long value = 0;
            for (int i = 0; i < maskfields.Count; i++)
                value += long.Parse(Math.Pow(2, maskfields[i]).ToString());
            this.Logger.LogInformation("MaskCalculation Completed");
            return value;
        }

        #endregion  Get Json from Objectset

        /// <summary>
        /// Get Preferred Collection for Consumer 2.0
        /// </summary>
        /// <param name="keyFileCollection"></param>
        /// <returns></returns>
        private string getPreferredCollection(int keyFileCollection)
        {
            if (keyFileCollection < 100000001)
            {
                return "mi-search-research-kfc-0m-100m";
            }
            else if (keyFileCollection < 166000000)
            {
                return "mi-search-research-kfc-101m-165m";
            }
            else 
            {
                return "mi-search-research-kfc-166m-250m";
            }
        }
    }
}
