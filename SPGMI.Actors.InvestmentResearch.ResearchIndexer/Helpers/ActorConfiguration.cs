using System;
using System.Collections.Generic;
using System.Text;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexer
{
    public class AppSettings
    {

        public string ElectorSubject { get; set; }
        public string ElectorTransport { get; set; }
        public int SchedulerMaxDispatchSize { get; set; }
        public string ODataEndpoint { get; set; }
        public string SecurityServiceUrl { get; set; }
        public string ODataAppKey { get; set; }
        public string ODataClientId { get; set; }
        public string EnvironmentSector { get; set; }
        public string MappingEnvironmentSector { get; set; }
        public string SubscriberName { get; set; }
        public string SubscriptionNamespace { get; set; }
        public bool BatchProcessor { get; set; }
        public int BatchSize { get; set; }
        public int BatchTimeoutMs { get; set; }
        public int MaxObjects { get; set; }
        public int MinObjects { get; set; }
        public int MaxPayloads { get; set; }
        public int MinPayloads { get; set; }
        public int QueueSize { get; set; }
        public int ResumeThreshold { get; set; }
        public bool PersistData { get; set; }
        public string User { get; set; }
        public string AccessKey { get; set; }
        public string Domain { get; set; }
        public string TransportProfile { get; set; }
        public string SubjectPrefix { get; set; }
        public string SearchApiKey { get; set; }
        public string KafkaBootStrapServer { get; set; }
        public string KafkaTopic { get; set; }
        public int HoldingQueueAgeThreshold { get; set; }
        public int MaxRetryCount { get; set; }
        public int FileCollectionKeyObject { get; set; }
        public int KeyFileCollectionKeyKeyItem { get; set; }
        public int FileVersionKeyObject { get; set; }
        public int FileVersionKeyitem { get; set; }

        public string ResponseackTopic { get; set; }

        public int HQworkerTimeout { get; set; }
        public bool IsIndexingEnabled { get; set; }
        public int HoldRetryCount { get; set; }

        public int IndexerPostFixId { get; set; }
        public string AppName { get; set; }
        public List<string> ODataErrors { get; set; }
        public string AlertReadySubject { get; set; }
        public bool ReplicationFirst { get; set; }
        public bool EnablePartialIndex { get; set; }
    }  
}
