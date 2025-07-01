using System;
using System.Collections.Generic;
using System.Text;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexResponseTracker
{
    public class AppSettings
    {
        public string ClientSettingsProviderServiceUri { get; set; }
        public string EnvironmentSector { get; set; }
        public string MappingEnvironmentSector { get; set; }
        public string AppName { get; set; }
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
        
        public string User { get; set; }
        public string AccessKey { get; set; }
        public string Domain { get; set; }
        public string BrokerAddress { get; set; }
        public string TopicName { get; set; }
        public string ConsumerGroupName { get; set; }
        public string ElectorSubject { get; set; }
        public string ElectorTransport { get; set; }
    }
}
