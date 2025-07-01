using System;
using System.Collections.Generic;
using System.Text;
using SPGMI.Pipeline.Interfaces;
using Microsoft.Extensions.Logging;
using SPGMI.Pipeline.ObjectSets;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexResponseTracker
{
    public class PipelineEventCallback : IPipelineEventCallback
    {
        private  readonly ILogger logger = null;
        public static object objectA = new object();
        public PipelineEventCallback(ILogger a_logger)
        {
            this.logger = a_logger;
        }
        public void OnError(IPipelineConsumer a_consumer, Exception a_error)
        {
            var exmessage = string.Empty;
            var exception = a_error;
            int i = 1;
            while (exception.InnerException != null)
            {
                exmessage += i.ToString() + Environment.NewLine + exception.InnerException.Message + Environment.NewLine + "StackTrace" + exception.InnerException.StackTrace + Environment.NewLine;
                exception = exception.InnerException;
                i++;
            }
            logger.LogError(exmessage);
        }

        public void OnEvent(IPipelineConsumer a_consumer, IPipelineEvent a_event)
        {
            switch (a_event.Type)
            {
                case EventType.ConnectionLost:
                case EventType.SubjectTimeout:
                case EventType.SubscriptionPaused:
                case EventType.MaximumQueueSizeReached:
                    logger.LogError(a_event.Type.ToString());
                    break;

                case EventType.ConnectionRestored:
                case EventType.SubjectResolved:
                case EventType.SubscriptionResumed:
                case EventType.ContentAcknowledged:
                case EventType.HoldQueueRestored:
                    //case EventType.HoldQueueResolved:
                    logger.LogInformation(a_event.Type.ToString());
                    break;

                case EventType.HearbeatRestored:
                case EventType.HeartbeatLost:
                case EventType.HoldQueueResolving:
                case EventType.HoldQueueAgeWarning:
                    //case EventType.HoldQueueDependencyEvent:
                    logger.LogDebug(a_event.Type.ToString());
                    break;
            }
            
        }
    }
}
