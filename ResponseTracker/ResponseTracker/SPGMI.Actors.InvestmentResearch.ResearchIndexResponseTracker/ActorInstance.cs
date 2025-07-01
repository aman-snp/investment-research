using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SPGMI.Actor.Interfaces.Bindings;
using SPGMI.Actors.InvestmentResearch.ResearchIndexResponseTracker.Actors;
using SPGMI.ContainerHost;
using SPGMI.DataPipeline.Actors.Bindings.ContentAPI;
using SPGMI.OData.Client.Extensions;
using SPGMI.Pipeline;
using SPGMI.Pipeline.Configuration;
using SPGMI.Pipeline.Content.Interfaces;
using SPGMI.Pipeline.Interfaces;
using SPGMI.Pipeline.ObjectSets;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexResponseTracker
{
    class ActorInstance : ContainerInstance
    {
        ResponseTracker tracker = null;
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken stoppingToken;
        public ActorInstance(int a_instanceId, string a_instanceName, int a_restartDelay) : base(a_instanceId, a_instanceName, a_restartDelay)
        {
            stoppingToken = tokenSource.Token;
        }

        public override void Start(CancellationToken cancellationToken)
        {
            CommonLogger.Instance.LogInformation($"Starting Container Instance = {InstanceName} with Subscriber Name = {InstanceName}");

            //actor configuration
            var actorConfig = SPGMI.ContainerHost.ServiceHelper.ResolveInstance<AppSettings>();
            var pipeline = SPGMI.ContainerHost.ServiceHelper.ResolveInstance<IPipeline>();
            var objectSetPool = SPGMI.ContainerHost.ServiceHelper.ResolveInstance<IObjectSetPool<ContentReadyObjectSet>>();
            tracker = new ResponseTracker(actorConfig.BrokerAddress, actorConfig.TopicName, actorConfig.ConsumerGroupName, pipeline, objectSetPool,  CommonLogger.Instance);
            
            Task.Run(() => tracker.StartConsumer(stoppingToken));
            CommonLogger.Instance.LogInformation($"Started Container Instance = {InstanceName} with Subscriber Name = {InstanceName}");
        }

        public override void Stop()
        {
            //Cleanup pipeline, input/output bindings, etc.
            //Dispose actor
            try
            {
                
                if (tracker != null)
                {
                    tokenSource.Cancel();
                    CommonLogger.Instance.LogInformation("Stopping container instance=" + InstanceName);
                    //tracker.Stop();
                    //tracker.Dispose();
                }
                CommonLogger.Instance.LogInformation($"Stopped Container Instance = {InstanceName} with Subscriber Name = {InstanceName}");
            }
            catch (Exception ex)
            {
                string errorMsg = String.Format($"Error Stopping Container Instance={InstanceName}. Exception: {ex.Message}, StackTrace: {ex.StackTrace}");
                CommonLogger.Instance.LogError(errorMsg);
            }
        }

    }
}
