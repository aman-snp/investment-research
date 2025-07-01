using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using SPGMI.Actor.Interfaces.Bindings;
using SPGMI.ContainerHost;
using SPGMI.DataPipeline.Actors;
using SPGMI.DataPipeline.Actors.Bindings.ContentAPI;
using SPGMI.DataPipeline.Actors.Persister;
using SPGMI.Logging;
using SPGMI.Pipeline;
using SPGMI.Pipeline.Configuration;
using SPGMI.Pipeline.Content.Interfaces;
using SPGMI.Pipeline.Interfaces;
using SPGMI.Pipeline.ObjectSets;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexer
{
    class PersisterInstance : ContainerInstance
    {
        PersisterActorBase<InvResrchPresentationObjectSet> indexerInstance = null;

        public PersisterInstance(int a_instanceId, string a_instanceName, int a_restartDelay) : base(a_instanceId, a_instanceName, a_restartDelay)
        {

        }

        public override void Start(CancellationToken cancellationToken)
        {
            try
            {
                CommonLogger.Instance.LogInformation($"Starting Container Instance = {InstanceName} with Subscriber Name = {InstanceName}");

                //actor configuration
                var actorConfig = SPGMI.ContainerHost.ServiceHelper.ResolveInstance<AppSettings>();
                var pipelineSettings = SPGMI.ContainerHost.ServiceHelper.ResolveInstance<PipelineSettings>();
                var metriclogger = SPGMI.ContainerHost.ServiceHelper.ResolveInstance<ISPGMILogger>();
                //Configure pipeline, input bindings, output bindings
                var pipeline = ContentPipeline.Create(pipelineSettings);
                IPipelineEventCallback eventCallback = new PipelineEventCallback(CommonLogger.Instance);

                var inputBinding = GetInputBinding(actorConfig, pipeline, eventCallback);

                //Output Binding
                var outputBinding = new OutputBinding<InvResrchPresentationObjectSet>(pipeline, eventCallback);
                var outputBindings = new List<IOutputBinding<InvResrchPresentationObjectSet>>() { outputBinding };

                //Run actor
                if (InstanceID <= 104)
                {
                    indexerInstance = new ResearchIndexer(InstanceName, metriclogger, inputBinding, outputBindings, actorConfig);
                }
                else
                    indexerInstance = new ResearchReplicator(InstanceName, metriclogger, inputBinding, outputBindings, actorConfig);
                CommonLogger.Instance.LogInformation($"Started Container Instance = {InstanceName} with Subscriber Name = {InstanceName}");
                indexerInstance.Run();
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
                CommonLogger.Instance.LogError($"Unhandled Exception while Starting Container Instance = {InstanceName} with Subscriber Name = {InstanceName} {exmessage}");
            }
        }

        public override void Stop()
        {
            //Cleanup pipeline, input/output bindings, etc.
            //Dispose actor
            try
            {
                if (indexerInstance != null)
                {
                    CommonLogger.Instance.LogInformation("Stopping container instance=" + InstanceName);
                    indexerInstance.Stop();
                    indexerInstance.Dispose();
                }
                CommonLogger.Instance.LogInformation($"Stopped Container Instance = {InstanceName} with Subscriber Name = {InstanceName}");
            }
            catch (Exception ex)
            {
                string errorMsg = String.Format($"Error Stopping Container Instance={InstanceName}. Exception: {ex.Message}, StackTrace: {ex.StackTrace}");
                CommonLogger.Instance.LogError(errorMsg);
            }
        }

        private InputBinding<InvResrchPresentationObjectSet> GetInputBinding(AppSettings actorConfig, IPipeline pipeline, IPipelineEventCallback eventCallback)
        {
            DynamicLoadBalancingSettings dynamicLoadBalancingSettings = new DynamicLoadBalancingSettings
            {
                ElectorSubject = actorConfig.ElectorSubject,
                ElectorTransport = actorConfig.ElectorTransport,
                SchedulerMaxDispatchSize = Convert.ToInt32(actorConfig.SchedulerMaxDispatchSize)
            };
            //Input Binding
            var inputBinding = new InputBinding<InvResrchPresentationObjectSet>(pipeline, dynamicLoadBalancingSettings, eventCallback)
            {
                GetObjectSetHashOverride = HashFunction,
            };

            //Batch Processing
            if (actorConfig.BatchProcessor)
            {
                inputBinding.BatchSize = actorConfig.BatchSize;
                inputBinding.BatchTimeoutMs = actorConfig.BatchTimeoutMs;
            }
            inputBinding.SubscriptionObjectSet.Namespace = actorConfig.SubscriptionNamespace;
            if (InstanceID != 0 && InstanceID == 105)
            {
                if (actorConfig.ReplicationFirst)
                {
                    inputBinding.SubscriptionObjectSet.State = ObjectSetStates.PERSISTENCE_SUCCESS;
                    inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.FormOrder.Value = 104;
                }
                else
                    inputBinding.SubscriptionObjectSet.StateOverride = new ObjectSetStateOverride("Indexing_Success");
            }
            else
            {
                if (actorConfig.ReplicationFirst && InstanceID == 104)
                {
                    inputBinding.SubscriptionObjectSet.StateOverride = new ObjectSetStateOverride("REPLICATION_SUCCESS");
                    actorConfig.IndexerPostFixId = 104;
                }
                else
                {
                    inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.FormOrder.Value = InstanceID;
                    inputBinding.SubscriptionObjectSet.State = ObjectSetStates.PERSISTENCE_SUCCESS;
                }

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchCat_instance.Add();
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchCat_instance[0].KeyInvestResearchCategory.DereferenceFields = new List<string>() { "KeyIRReportCategoryTree", "ProductCaption", "InvestResearchCategory" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchCat_instance[0].KeyInvestResearchCategory.DereferenceMode = DereferenceModeType.All;

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchAppr_instance.Add();
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchAppr_instance[0].KeyInvestResearchApproach.DereferenceFields = new List<string>() { "KeyIRReportCategoryTree", "ProductCaption", "InvestmentResearchApproach" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchAppr_instance[0].KeyInvestResearchApproach.DereferenceMode = DereferenceModeType.All;

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchEvent_instance.Add();
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchEvent_instance[0].KeyInvestResearchEvent.DereferenceFields = new List<string>() { "KeyIRReportCategoryTree", "ProductCaption", "InvestmentResearchEvent" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchEvent_instance[0].KeyInvestResearchEvent.DereferenceMode = DereferenceModeType.All;

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchFocus_instance.Add();
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchFocus_instance[0].KeyInvestResearchFocus.DereferenceFields = new List<string>() { "KeyIRReportCategoryTree", "ProductCaption", "InvestResearchFocus" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchFocus_instance[0].KeyInvestResearchFocus.DereferenceMode = DereferenceModeType.All;

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchType_instance.Add();
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchType_instance[0].KeyInvestResearchType.DereferenceFields = new List<string>() { "KeyIRReportCategoryTree", "ProductCaption" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchType_instance[0].KeyInvestResearchType.DereferenceMode = DereferenceModeType.All;

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchSubj_instance.Add();
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchSubj_instance[0].KeyInvestResearchSubject.DereferenceFields = new List<string>() { "KeyIRReportCategoryTree", "ProductCaption", "KeyInvestResearchSubject", "InvestResearchSubject" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.InvestResearchSubj_instance[0].KeyInvestResearchSubject.DereferenceMode = DereferenceModeType.All;


                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollRelation_instance.Add();
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollRelation_instance[0].KeyInstnResearchTeam.DereferenceFields = new List<string>() { "InstnResearchTeamName" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollRelation_instance[0].KeyInstnResearchTeam.DereferenceMode = DereferenceModeType.All;

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollRelation_instance[0].KeyInstn.DereferenceFields = new List<string>() { "InstnName" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollRelation_instance[0].KeyInstn.DereferenceMode = DereferenceModeType.All;

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollRelation_instance[0].DocsFileCollRelationContributorMIObject_instance.KeyInstn.DereferenceFields = new List<string>() { "InstnName", "ResearchContributorName", "ResearchContributorStatus", "InvestResearchContributorType" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollRelation_instance[0].DocsFileCollRelationContributorMIObject_instance.KeyInstn.DereferenceMode = DereferenceModeType.All;

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollRelation_instance[0].KeyPerson.DereferenceFields = new List<string>() { "BestPersonName" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollRelation_instance[0].KeyPerson.DereferenceMode = DereferenceModeType.All;

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.KeyForeignLanguage.DereferenceFields = new List<string>() { "ForeignLanguage" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.KeyForeignLanguage.DereferenceFields = new List<string>() { "ForeignLanguageShort" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.KeyForeignLanguage.DereferenceMode = DereferenceModeType.All;

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.KeyInvestResearchReportType.DereferenceFields = new List<string>() { "InvestResearchReportType" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.KeyInvestResearchReportType.DereferenceMode = DereferenceModeType.All;

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollGeo_instance.Add();
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollGeo_instance[0].KeyGeographyTree.DereferenceFields = new List<string>() { "ProductCaption" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollGeo_instance[0].KeyGeographyTree.DereferenceMode = DereferenceModeType.All;

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocFileCollCIQIndustryMIObject_instance.Add();
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocFileCollCIQIndustryMIObject_instance[0].KeyCIQIndustryTree.DereferenceFields = new List<string>() { "IndustryLongName" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocFileCollCIQIndustryMIObject_instance[0].KeyCIQIndustryTree.DereferenceMode = DereferenceModeType.All;

                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollIndustry_instance.Add();
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollIndustry_instance[0].KeyMIIndustryTree.DereferenceFields = new List<string>() { "IndustryLongName" };
                inputBinding.SubscriptionObjectSet.DocsFileCollPresentation_instance.DocsFileCollIndustry_instance[0].KeyMIIndustryTree.DereferenceMode = DereferenceModeType.All;
            }
            return inputBinding;
        }

        public static int HashFunction(InvResrchPresentationObjectSet input)
        {
            int.TryParse(input.DocsFileCollPresentation_instance.ResearchDocumentID.SourceValue, out int researchDocumentId);
            return researchDocumentId;
        }

    }
}
