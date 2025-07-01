using System;
using System.Collections.Generic;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using SPGMI.Logging;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexer.Helpers
{
    public static class KafkaPublisher
    {
        public static IProducer<Null, string> producer;
        public static string KafkaBootstrapServers;
        public static ProducerBuilder<Null, string> producerBuilder;
        public static ILogger _logger;
        static KafkaPublisher()
        {
        }

        public static void Initialise(ILogger logger)
        {
            _logger = logger;
            var kafkaProducerConfig = new ClientConfig() { BootstrapServers = KafkaBootstrapServers};
            var config = new ProducerConfig(kafkaProducerConfig);
            producerBuilder = new ProducerBuilder<Null, string>(config);
            producer = producerBuilder.SetErrorHandler((producer, error) =>
            {
                if (error.IsError)
                    ErrorHandler(error);
            }).Build();
        }

        private static void ErrorHandler(Error error)
        {
            var exmessage = error.Reason + Environment.NewLine + " Error Code " + error.Code + Environment.NewLine + "Trying to reset Kafka Connection";
            _logger.LogError(exmessage);
            Initialise(_logger);
        }

        //public static void PublishMessage(string message)
        //{

        //    KafkaPublisher.producer.Produce("consumer-search-push-investmentresearch", new Message<Null, string>() { Value = message });


        //}
    }
}
