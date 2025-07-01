using System;
using System.Collections.Generic;
using System.Text;
using SPGMI.Pipeline.Interfaces;
using Microsoft.Extensions.Logging;
using SPGMI.Pipeline.ObjectSets;
using System.Threading;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexer
{
    public class PipelineEventCallback : IPipelineEventCallback
    {
        readonly ILogger logger;
        public static object objectA = new object();
        public PipelineEventCallback(Microsoft.Extensions.Logging.ILogger a_logger)
        {
            logger = a_logger;
        }
        public void OnError(IPipelineConsumer a_consumer, Exception a_error)
        {
            int i = 1;
            var exmessage = "Pipeline Event Call back" + a_error.Message + Environment.NewLine + " Stack Trace " + a_error.StackTrace + Environment.NewLine;
            while (a_error.InnerException != null)
            {
                exmessage += i.ToString() + Environment.NewLine + a_error.InnerException.Message + Environment.NewLine + "StackTrace" + a_error.InnerException.StackTrace + Environment.NewLine;
                a_error = a_error.InnerException;
                i++;
            }
            logger.LogError(exmessage);
        }

        public void OnEvent(IPipelineConsumer a_consumer, IPipelineEvent a_event)
        {
            try
            {
                switch (a_event.Type)
                {
                    case EventType.ConnectionLost:
                    case EventType.SubjectTimeout:
                    case EventType.SubscriptionPaused:
                    case EventType.MaximumQueueSizeReached:
                        logger.LogError(a_event.Type + a_event.ToString());
                        break;

                    case EventType.ConnectionRestored:
                    case EventType.SubjectResolved:
                    case EventType.SubscriptionResumed:
                    case EventType.ContentAcknowledged:
                    case EventType.HoldQueueRestored:
                        logger.LogInformation(a_event.Type + a_event.ToString());
                        break;

                    case EventType.HearbeatRestored:
                    case EventType.HeartbeatLost:
                    case EventType.HoldQueueResolved:
                    case EventType.HoldQueueResolving:
                        //case EventType.HoldQueueDependencyEvent:
                        logger.LogDebug(a_event.Type + a_event.ToString());
                        break;

                    case EventType.HoldQueueAgeWarning:
                        logger.LogDebug(a_event.Type + a_event.ToString());
                        var exmessage1 = string.Empty;
                        try
                        {
                            var exmessage = string.Empty;
                            //lock (objectA)
                            // {
                            try
                            {
                                SPGMI.Pipeline.Interfaces.IHoldingQueueCallBack hqCallback = a_event.GetHoldingQueueCallBack();
                                if (hqCallback != null)
                                {
                                    //var itemCopy = hqCallback.AcquireHeldItemCopy();
                                    //if (itemCopy != null)
                                    // {
                                    //   var osIdentifier = (itemCopy.Value as InvResrchPresentationObjectSet).OSIdentifier;
                                    //   HoldKeyFileCollectionList.ObjectsetIdentifier.Add(osIdentifier);
                                    //}
                                    hqCallback.ReleaseHeldItemFromHQ();
                                    logger.LogDebug("Message released from HQ");
                                }
                                else
                                    logger.LogError("Pipeline event On Event HQ Callback is null");

                            }
                            catch (Exception exception)
                            {
                                int i = 1;
                                exmessage = exception.Message + Environment.NewLine + " Stack Trace " + (exception.StackTrace ?? string.Empty) + Environment.NewLine;
                                while (exception.InnerException != null)
                                {
                                    exmessage += i.ToString() + Environment.NewLine + exception.InnerException.Message + Environment.NewLine + "StackTrace" + (exception.InnerException.StackTrace ?? string.Empty) + Environment.NewLine;
                                    exception = exception.InnerException;
                                    i++;
                                }

                            }
                            // }
                            if (!string.IsNullOrWhiteSpace(exmessage))
                                logger.LogError("Pipeline event callback Exception " + a_event.Type.ToString() + " " + exmessage);
                        }
                        catch (Exception exception)
                        {
                            int i = 1;
                            exmessage1 = exception.Message + Environment.NewLine + " Stack Trace " + (exception.StackTrace ?? string.Empty) + Environment.NewLine;
                            while (exception.InnerException != null)
                            {
                                exmessage1 += i.ToString() + Environment.NewLine + exception.InnerException.Message + Environment.NewLine + "StackTrace" + (exception.InnerException.StackTrace ?? string.Empty) + Environment.NewLine;
                                exception = exception.InnerException;
                                i++;
                            }
                            if (!string.IsNullOrWhiteSpace(exmessage1))
                                logger.LogError("Pipeline event callback Outer Exception " + a_event.Type.ToString() + " " + exmessage1);
                        }
                        break;
                }
            }
            catch
            {

            }
        }
        public void ReleaseObject(IPipelineEvent a_event)
        {

        }
    }
}
