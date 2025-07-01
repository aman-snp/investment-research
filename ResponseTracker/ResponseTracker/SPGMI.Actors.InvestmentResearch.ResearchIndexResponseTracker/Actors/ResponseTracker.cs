using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SPGMI.Actors.InvestmentResearch.ResearchIndexResponseTracker.Helpers;
using SPGMI.Pipeline;
using SPGMI.Pipeline.ObjectSets;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexResponseTracker.Actors
{
    public class ResponseTracker
    {
        ConsumerConfig config;
        ILogger _logger;
        readonly string TopicName, BrokerAddress, ConsumerGroupName;
        IPipeline _pipeline;
        IObjectSetPool<ContentReadyObjectSet> _objectSetPool;
        public ResponseTracker(string brokerAddress, string topicName, string consumerGroupName, Pipeline.IPipeline pipeline, IObjectSetPool<ContentReadyObjectSet> objectSetPool, ILogger logger)
        {
            TopicName = topicName;
            BrokerAddress = brokerAddress;
            ConsumerGroupName = consumerGroupName;
            _logger = logger;
            _pipeline = pipeline;
            _objectSetPool = objectSetPool;
        }
        public async Task StartConsumer(CancellationToken stoppingToken)
        {
            Task serviceTask = Task.Factory.StartNew(() =>
            {
                var consumer = new KafkaConsumer<Rootobject>()
                {
                    //Connection information
                    BrokerAddress = BrokerAddress ,
                    TopicName = TopicName,
                    ConsumerGroupName = ConsumerGroupName,

                    // MS wait time for Consumer if last check had no messages
                    CheckInterval = 1000,

                    // Offset management
                    AutoCommitOffset = true, // if false you must send offset commits to Kafka manually
                    AutoCommitInterval = 5000, // MS time between auto offset commits on server, only matters if 
                    AutoStoreOffset = true,
                    log = _logger,
                    // demo settings
                    showEvents = true // if true Kafka events will write out to the Console
                };
                consumer.RegisterCallback(ConsumedCdc, m => m != null);
            }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);
            serviceTask.ContinueWith(t => HandleServiceInstanceException(t, t.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }

        private void HandleServiceInstanceException(Task t, AggregateException exception)
        {

        }

        private void ConsumedCdc(Rootobject message)
        {
            ContentReadyObjectSet os = null;
            string primaryKeyValue = string.Empty;
            try
            {
                if (message.VectorDoc.ToString().Equals("false", StringComparison.CurrentCultureIgnoreCase))
                {
                    primaryKeyValue = message.DocId.Substring(message.DocId.LastIndexOf('_') + 1);

                    _logger.LogInformation($"Received message with Tracker {message.IssueId} , KeyName KeyFileCollection, KeyValue {primaryKeyValue}, Status {message.Status} ");
                    if (!message.Status.ToString().Equals("Success", StringComparison.CurrentCultureIgnoreCase))
                        throw new Exception($"Indexing failed for OSIdentifier {message.IssueId} and KeyFileCollection {primaryKeyValue}");

                    _logger.LogInformation($"Processing Message with OSIdentifier {message.IssueId} and KeyFileCollection {primaryKeyValue}");
                    os = _objectSetPool.Get();
                    os.KeyItem.Value = 96208;
                    os.KeyObject.Value = 5514;
                    os.OID.Value = primaryKeyValue;
                    //os.StateOverride = "IndexingComplete";
                    os.State = Pipeline.Content.Interfaces.ObjectSetStates.INITIALIZED;
                    os.Namespace = "MI";

                    os.PublishContent();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace.ToString());
            }
            finally
            {
                if (os != null)
                    os.Release();
            }
        }
    }


    public class Rootobject
    {
        [JsonProperty(PropertyName = "IssueId")]
        public string IssueId { get; set; }
        [JsonProperty(PropertyName = "DocId")]
        public string DocId { get; set; }
        [JsonProperty(PropertyName = "Status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "VectorDoc")]
        public bool VectorDoc { get; set; }
        [JsonProperty(PropertyName = "RegionId")]
        public string RegionId { get; set; }
    }

}
//Pipeline Instance in App Initilise and Create an Objectset pool and then publiish the Content ready objectset 
