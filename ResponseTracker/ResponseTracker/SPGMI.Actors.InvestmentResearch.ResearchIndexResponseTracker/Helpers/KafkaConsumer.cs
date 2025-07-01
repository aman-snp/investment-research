using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexResponseTracker.Helpers
{
    public class KafkaConsumer<T>
    {
        public bool showEvents = false;

        private readonly ConcurrentBag<Tuple<Action<T>, Predicate<T>>> callbacks = new ConcurrentBag<Tuple<Action<T>, Predicate<T>>>();
        private readonly System.Timers.Timer checkTimer = new System.Timers.Timer();
        private volatile bool initialized = false;
        private readonly object initSync = new object();

        public IConsumer<string, string> kafkaConsumer = null;

        public KafkaConsumer()
        {
            checkTimer.Enabled = false;
            this.ConsumerGroupName = Guid.NewGuid().ToString();

        }
        public ILogger log { get; set; }

        public string BrokerAddress { get; set; }

        public string TopicName { get; set; }

        public string ConsumerGroupName { get; set; }

        public long CheckInterval { get; set; }

        public bool? AutoCommitOffset { get; set; }

        public bool? AutoStoreOffset { get; set; }

        public int? AutoCommitInterval { get; set; }

        public int? StatisticsInterval { get; set; }

        public void RegisterCallback(Action<T> callback, Predicate<T> predicate)
        {
            // add it
            Initialize();
            callbacks.Add(new Tuple<Action<T>, Predicate<T>>(callback, predicate));
        }

        private void Initialize()
        {
            if (!initialized)
            {
                lock (initSync)
                {
                    if (!initialized)
                    {
                        var config = new ConsumerConfig { BootstrapServers = this.BrokerAddress, GroupId = ConsumerGroupName, EnableAutoOffsetStore = (AutoStoreOffset ?? true), AutoCommitIntervalMs = (AutoCommitInterval ?? 5000), EnableAutoCommit = (AutoCommitOffset ?? false), StatisticsIntervalMs = (StatisticsInterval ?? 60000), AutoOffsetReset = AutoOffsetReset.Earliest };
                        //Create the Kafka consumer, and subscribe
                        this.kafkaConsumer = new ConsumerBuilder<string, string>(config).Build();
                        //AttachToKafkaEvents();
                        SubscribeKafka();

                        // configure timer
                        checkTimer.Interval = CheckInterval;

                        // elapsed event
                        checkTimer.Elapsed += CheckTimer_Elapsed;

                        // start time
                        checkTimer.Start();

                        //Initialized
                        initialized = true;
                    }
                }
            }

        }


        private void CheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // stop timer
            checkTimer.Stop();

            try
            {
                // check for message
                var message = Consume();

                while (message != null)
                {
                    // evaluate callbacks
                    foreach (var callbackTuple in callbacks)
                    {
                        // evaluate predicate
                        if (callbackTuple.Item2(message))
                        {
                            // invoke callaback
                            callbackTuple.Item1(message);
                        }
                    }

                    // check for message
                    message = Consume();
                }
            }
            catch (KafkaException ke)
            {
                // log Kafka failure
            }
            catch (Exception ex)
            {
                // log non-Kafka failure
            }
            finally
            {
                checkTimer.Start();
            }
        }

        private T Consume()
        {
            // Consume a message
            ConsumeResult<string, string> kafkaMessage;
            T deserializedKafkaMessage = default(T);

            // get a message if one exists
            kafkaMessage = this.kafkaConsumer.Consume(TimeSpan.FromMilliseconds(100));
            
             if (kafkaMessage == null || kafkaMessage.Message == null || string.IsNullOrWhiteSpace(kafkaMessage.Message.Value))
            {
                // throw
                //throw new Exception( "Blank message received");
            }
            else
            {
               // log.LogInformation($"Messgae received {kafkaMessage.Message.Value}");
                // de-serialize message
                deserializedKafkaMessage = JsonConvert.DeserializeObject<T>(kafkaMessage.Message.Value);

                //AcknowledgeMessage(kafkaMessage.Topic, kafkaMessage.Partition, kafkaMessage.Offset);
            }


            return deserializedKafkaMessage;
        }

        //private void AttachToKafkaEvents()
        //{
        //    //Hooked into Kafka events to make sure that partitions are assigned/unassigned properly during a rebalance

        //    //When we reach the end of a partition
        //    this.kafkaConsumer.OnPartitionEOF += (_, end) =>
        //    {
        //        if (showEvents)
        //            log.LogInformation($"Reached end of topic {end.Topic} partition {end.Partition}, next message will be at offset {end.Offset}");
        //    };

        //    //Errors
        //    this.kafkaConsumer.OnError += (_, error) =>
        //    {
        //        if (showEvents)
        //            log.LogInformation($"Error: {error}", new KafkaException(error));
        //    };

        //    //When a partition is assigned
        //    this.kafkaConsumer.OnPartitionsAssigned += (_, partitions) =>
        //    {
        //        if (showEvents)
        //            log.LogInformation($"Assigned partitions: [{string.Join(", ", partitions)}], member id: {this.kafkaConsumer.MemberId}");

        //        this.kafkaConsumer.Assign(partitions);

        //        //use a version of the following code to assign a specific partition and offset
        //        //this.KafkaConsumer.Assign(new List<TopicPartitionOffset>{ new TopicPartitionOffset(partitions[0], Offset.Beginning) });
        //    };

        //    //When a partition is revoked
        //    this.kafkaConsumer.OnPartitionsRevoked += (_, partitions) =>
        //    {
        //        if (showEvents)
        //            log.LogInformation($"Revoked partitions: [{string.Join(", ", partitions)}]");

        //        try
        //        {
        //            this.kafkaConsumer.Unassign();
        //        }
        //        catch { };

        //    };
        //}
        private void SubscribeKafka()
        {
            this.kafkaConsumer.Subscribe(this.TopicName);
        }

        // The acknowledge structure is left here to experiment with using manual offset committing.
        public void AcknowledgeMessage(string topic, int partition, long offset)
        {
            try
            {
                var topicPartitionOffset = new TopicPartitionOffset(topic, partition, new Offset(offset));
                this.kafkaConsumer.Commit(new List<TopicPartitionOffset> { topicPartitionOffset });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}
