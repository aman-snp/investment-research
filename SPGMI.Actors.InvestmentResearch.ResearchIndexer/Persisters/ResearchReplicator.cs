using System;
using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using SPGMI.Actor.Interfaces.Bindings;
using SPGMI.DataPipeline.Actors.Bindings.ContentAPI;
using SPGMI.DataPipeline.Actors.Persister;
using SPGMI.Pipeline;
using SPGMI.Pipeline.Content.Interfaces;
using SPGMI.Pipeline.Interfaces;
using SPGMI.Pipeline.ObjectSets;
using SPGMI.Logging;
using System.Diagnostics;
using SPGMI.Actors.InvestmentResearch.ResearchIndexer.Helpers;
using System.Threading.Tasks;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexer
{
    internal class ResearchReplicator : PersisterActorBase<InvResrchPresentationObjectSet>
    {
        readonly int _maxRetryCount, _hqTimeOut;
        readonly string KafkaTopic;
        readonly AppSettings appSettings;
        readonly ISPGMILogger metricLogger;

        readonly Stopwatch TotalTimer;
        readonly Stopwatch holdTimer;
        readonly Stopwatch indexingtimer;
        readonly Stopwatch dependencyTimer;
        double holdTime = 0, indexingTime = 0, totalTime = 0, dependencyTime = 0;
        string objectsetKey = string.Empty;

        #region Ctor

        public ResearchReplicator(string name, ISPGMILogger logger, IInputBinding<IPipelinePayload> inputBinding, IEnumerable<IOutputBinding> outputBinding, AppSettings actorConfig) : base(name, inputBinding, outputBinding)
        {
            KafkaTopic = actorConfig.KafkaTopic;
            _maxRetryCount = actorConfig.MaxRetryCount;
            _hqTimeOut = actorConfig.HoldingQueueAgeThreshold;
            appSettings = actorConfig;
            metricLogger = logger;

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

                // If dependency resolved then Publish to Alerts
                if (input.State == ObjectSetStates.IN_HOLDINGQUEUE && input.StateOverride == "IN_HOLDINGQUEUE")
                {
                    //return PublishOStoAlerts(input).GetAwaiter().GetResult();
                    return Task.Run(() => PublishOStoAlerts(input)).GetAwaiter().GetResult();
                }

                var hqKeyFileVersionDependency = GetHQDependencyWithKeyFileVersion(input);
                // received from Persister - Hold The message with Consensus and indexing Dependency
                // once input is in Holding Queue, gather information and publish Json for Indexing 
                if (ValidateTimeSpan(input.DocsFileCollPresentation_instance.PresentationDate.Value) && hqKeyFileVersionDependency != null)
                {
                    Hold(input, keyFileCollection, new List<ObjectSet>() { hqKeyFileVersionDependency }, input.DocsFileCollPresentation_instance.TriggerEventID.Value);
                    return null;
                }
                else
                {
                    return Task.Run(() => PublishOStoAlerts(input)).GetAwaiter().GetResult();
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

        async Task<ObjectSet> PublishOStoAlerts(InvResrchPresentationObjectSet input)
        {
            this.Logger.LogInformation($"OSIdentifier {input.OSIdentifier} KeyFileCollection: {input.DocsFileCollPresentation_instance.KeyFileCollection.Value} - Inside Publish OS to Alerts");
            if (appSettings.ReplicationFirst)
            {
                input.StateOverride = "REPLICATION_SUCCESS";
                return input;
            }
            else
            {
                this.Logger.LogInformation($"PublishOStoAlerts Delay start: {DateTime.Now} for KeyFileCollection  {input.DocsFileCollPresentation_instance.KeyFileCollection.Value}");
                await Task.Delay(TimeSpan.FromMinutes(2));
                this.Logger.LogInformation($"PublishOStoAlerts Delay end: {DateTime.Now} for KeyFileCollection  {input.DocsFileCollPresentation_instance.KeyFileCollection.Value}");

                input.StateOverride = appSettings.AlertReadySubject;
                return input;
            }
        }

        ContentReadyObjectSet GetHQDependencyWithKeyFileVersion(InvResrchPresentationObjectSet input)
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

            return null;
        }

        void Hold(InvResrchPresentationObjectSet input, int keyFileCollection, List<ObjectSet> holdingQueueDependencies, long? holdCount)
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
                    input.StateOverride = "IN_HOLDINGQUEUE";
                    input.DocsFileCollPresentation_instance.TriggerEventID.Value = holdCount ?? 1;
                    input.StateInfo = string.Empty;
                    var pipelinepayload = base.GetPipelinePayload(input.OSIdentifier);
                    (this.Input as InputBinding<InvResrchPresentationObjectSet>).Hold(pipelinepayload, holdingQueueDependencies, _hqTimeOut, null);
                    this.Logger.LogInformation($"OSIdentifier {input.OSIdentifier} KeyFileCollection: {keyFileCollection} - Message Hold Successfully and for replication check try  {holdCount ?? 0}");
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

    }
}
