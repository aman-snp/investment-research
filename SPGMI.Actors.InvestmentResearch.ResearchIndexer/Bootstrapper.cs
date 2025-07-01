using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SPGMI.ContainerHost.Interfaces;
using SPGMI.Logging;
using SPGMI.Pipeline.Configuration;
using SPGMI.Actors.InvestmentResearch.ResearchIndexer;
using SPGMI.Actors.InvestmentResearch.ResearchIndexer.Helpers;
using SPGMI.Config.Factory;
using SPGMI.Config.Reader;
using System.Linq;


namespace SPGMI.Actors.InvestmentResearch.ResearchIndexer
{
    //[Optional] Read configs; Additional settings, if any.
    //pipeline settings, actor specific settings,  etc. 
    //Read from <appsettings.json>: add "ActorConfiguration" section and bind it to a class.
    public class Bootstrapper : IBootstrapper
    {
        public void SetUp(IContainerInstance[] containerInstances)
        {
            var env = Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var actorConfig = configuration.GetSection("AppSettings").Get<AppSettings>();
            var loggerConfig = configuration.GetSection("MILogger").Get<LoggerConfig>();
            var postfix = configuration.GetValue<string>("PERSISTER_POSTFIX");
            int instanceId = 0;
            Console.WriteLine(postfix);
            switch (postfix)
            {
                case "Backfill":
                    instanceId = 101;
                    break;
                case "Bulk":
                    instanceId = 102;
                    break;
                case "Online":
                    instanceId = 103;
                    break;
                case "Realtime":
                    instanceId = 104;
                    break;
                case "Replicator":
                    instanceId = 105;
                    break;
            }

            SPGMI.ContainerHost.ServiceHelper.RegistertInstance<AppSettings>(actorConfig);
            ConfigurationProvider.GetPersistentStorageConfiguration(ref actorConfig, ref loggerConfig);
            KafkaPublisher.KafkaBootstrapServers = actorConfig.KafkaBootStrapServer;

            //[Optional] MI logger
            IServiceCollection services = new ServiceCollection();
            services.AddMILogger(options =>
            {
                options.ApplicationName = "ResearchIndexer" + postfix + "_int";
                options.BrokerList = loggerConfig.BrokerList;
                options.LogStrategy = loggerConfig.LogStrategy;
                options.Topic = loggerConfig.Topic;
                //configuration.GetSection("MILogger").Bind(options);
            });


            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var logProvider = serviceProvider.GetRequiredService<ILoggerProvider>();
            var logger = SPGMI.Actor.Core.LogHelper.CreateLogger(logProvider, "SPGMI.Actors.InvestmentResearch.ResearchIndexer");

            SPGMI.ContainerHost.ServiceHelper.RegistertInstance<ILogger>(logger);

            var metriclogger = serviceProvider.GetRequiredService<ISPGMILogger>();
            SPGMI.ContainerHost.ServiceHelper.RegistertInstance<ISPGMILogger>(metriclogger);

            var pipelineSettings = new PipelineSettings
            {
                Pooling = new PoolSettings()
                {
                    MinObjects = actorConfig.MinObjects,
                    MaxObjects = actorConfig.MaxObjects,
                    MinPayloads = actorConfig.MinPayloads,
                    MaxPayloads = actorConfig.MaxPayloads
                },

                Queue = new QueueSettings()
                {
                    MaxSize = actorConfig.QueueSize,
                    ResumeThreshold = actorConfig.ResumeThreshold
                },
                HoldingQueue = new HoldingQueueSettings() { AgeThreshold = (actorConfig.HoldingQueueAgeThreshold) },
                EnvironmentSector = actorConfig.EnvironmentSector,
                MappingEnvironmentSector = actorConfig.MappingEnvironmentSector
            };

            SPGMI.ContainerHost.ServiceHelper.RegistertInstance<PipelineSettings>(pipelineSettings);
            actorConfig.IndexerPostFixId = instanceId;
            foreach (var containerInstance in containerInstances)
            {
                //some postfix
                containerInstance.InstanceName += actorConfig.SubscriberName + postfix;
                containerInstance.InstanceID = instanceId;
            }
            if (instanceId != 105)
                KafkaPublisher.Initialise(logger);
            //Read from <app.config>
            //var logLevel = (LogLevel)Enum.Parse(typeof(LogLevel), ConfigurationManager.AppSettings["LogLevel"]);
            //var logFilePath = ConfigurationManager.AppSettings["LogFilePath"];

            //SPGMI.Actor.Core.LogHelper.LoggerFactory.AddConsole(LogLevel.Debug);

            //[Note] If you're using serviceInstance Name/Id in the subscriber name, 
            //then please note that each instance will have different name and id unlike legacy winservicehost framework.
            //serviceInstance IDs will be ranging from  [InstanceId] to [InstanceId + TotalInstances - 1], and
            //serviceInstanceName will be [InstanceName + InstanceId].

            //[Optional] Configure additional logger(s), if required.
            //var nlogger = NLog.LogManager.GetCurrentClassLogger();
            //SPGMI.ContainerHost.ServiceHelper.RegistertInstance<NLog.ILogger>(nlogger);
        }
    }


    public static class ConfigurationProvider //: IDisposable
    {
        private static object syncLock = new object();
        private static readonly string ConfigurationValueSection = "ConfigurationValues";


        public static void GetPersistentStorageConfiguration(ref AppSettings appSettings, ref LoggerConfig loggerConfig)
        {
            lock (syncLock)
            {
                try
                {
                    using (SPGMI.Config.Interface.IConfigurationSource configSource = ConfigurationSource.Create("ResearchIndexer"))
                    {
                        ConfigReader reader = new ConfigReader(configSource.Configuration.GetSubKey(ConfigurationValueSection));
                        if (reader != null)
                        {
                            appSettings.BatchSize = Convert.ToInt32(reader.ReadString("BatchSize"));
                            appSettings.BatchTimeoutMs = Convert.ToInt32(reader.ReadString("BatchTimeoutMs"));
                            appSettings.FileCollectionKeyObject = Convert.ToInt32(reader.ReadString("FileCollectionKeyObject"));
                            appSettings.FileVersionKeyitem = Convert.ToInt32(reader.ReadString("FileVersionKeyitem"));
                            appSettings.FileVersionKeyObject = Convert.ToInt32(reader.ReadString("FileVersionKeyObject"));
                            appSettings.HoldingQueueAgeThreshold = Convert.ToInt32(reader.ReadString("HoldingQueueAgeThreshold"));

                            appSettings.HQworkerTimeout = Convert.ToInt32(reader.ReadString("HqWorkerTimeOut"));
                            appSettings.IsIndexingEnabled = Convert.ToBoolean(reader.ReadString("IsIndexingEnabled"));

                            appSettings.KafkaBootStrapServer = reader.ReadString("KafkaBootStrapServer");
                            appSettings.KafkaTopic = reader.ReadString("KafkaTopic");
                            appSettings.KeyFileCollectionKeyKeyItem = Convert.ToInt32(reader.ReadString("KeyFileCollectionKeyKeyItem"));
                            appSettings.MaxObjects = Convert.ToInt32(reader.ReadString("MaxObjects"));
                            appSettings.MaxPayloads = Convert.ToInt32(reader.ReadString("MaxPayloads"));
                            appSettings.MaxRetryCount = Convert.ToInt32(reader.ReadString("MaxRetryCount"));
                            appSettings.MinObjects = Convert.ToInt32(reader.ReadString("MinObjects"));
                            appSettings.MinPayloads = Convert.ToInt32(reader.ReadString("MinPayloads"));
                            appSettings.QueueSize = Convert.ToInt32(reader.ReadString("QueueSize"));
                            appSettings.ResponseackTopic = reader.ReadString("ResponseackTopic");
                            appSettings.ResumeThreshold = Convert.ToInt32(reader.ReadString("ResumeThreshold"));
                            appSettings.SubjectPrefix = reader.ReadString("SubjectPrefix");
                            appSettings.SearchApiKey = reader.ReadString("SearchApiKey");
                            appSettings.TransportProfile = reader.ReadString("TransportProfile");
                            appSettings.User = reader.ReadString("User");
                            appSettings.HoldRetryCount = Convert.ToInt32(reader.ReadString("HoldRetryCount"));

                            appSettings.ElectorSubject = reader.ReadString("ElectorSubject");
                            appSettings.ElectorTransport = reader.ReadString("ElectorTransport");
                            appSettings.SchedulerMaxDispatchSize = Convert.ToInt32(reader.ReadString("SchedulerMaxDispatchSize"));
                            appSettings.EnvironmentSector = reader.ReadString("EnvironmentSector");
                            appSettings.MappingEnvironmentSector = reader.ReadString("MappingEnvironmentSector");
                            appSettings.SubscriberName = reader.ReadString("SubscriberName");
                            appSettings.SubscriptionNamespace = reader.ReadString("SubscriptionNamespace");
                            appSettings.PersistData = Convert.ToBoolean(reader.ReadString("PersistData"));
                            appSettings.BatchProcessor = Convert.ToBoolean(reader.ReadString("BatchProcessor"));
                            appSettings.AccessKey = reader.ReadString("AccessKey");
                            appSettings.AppName = reader.ReadString("AppName");
                            appSettings.Domain = reader.ReadString("Domain");
                            appSettings.ODataErrors = reader.ReadString("ODataErrors").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            appSettings.ODataEndpoint = reader.ReadString("ODataEndpoint");
                            appSettings.SecurityServiceUrl = reader.ReadString("SecurityServiceUrl");
                            appSettings.ODataAppKey = reader.ReadString("ODataAppKey");
                            appSettings.ODataClientId = reader.ReadString("ODataClientId");

                            appSettings.ReplicationFirst = Convert.ToBoolean(reader.ReadString("ReplicationFirst"));
                            appSettings.AlertReadySubject = reader.ReadString("AlertReadySubject");

                            loggerConfig.ApplicationName = reader.ReadString("ApplicationName");
                            loggerConfig.BrokerList = reader.ReadString("BrokerList");
                            loggerConfig.LogStrategy = reader.ReadString("LogStrategy");
                            loggerConfig.Topic = reader.ReadString("Topic");
                        }
                    }
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
                    Console.WriteLine($"Unhandled Exception while Starting at BootStrapper {exmessage}");
                }
            }
        }
    }
}
