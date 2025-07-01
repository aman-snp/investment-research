using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SPGMI.ContainerHost.Interfaces;
using SPGMI.Logging;
using SPGMI.Pipeline.Configuration;
using SPGMI.Actors.InvestmentResearch.ResearchIndexResponseTracker;
using SPGMI.Actors.InvestmentResearch.ResearchIndexResponseTracker.Helpers;
using SPGMI.Pipeline;
using SPGMI.Config.Factory;
using SPGMI.Config.Reader;
using SPGMI.Pipeline.ObjectSets;

namespace SPGMI.Actors.InvestmentResearch.ResearchIndexResponseTracker
{
    public class Bootstrapper : IBootstrapper
    {
        public void SetUp(IContainerInstance[] containerInstances)
        {

            var env = Environment.GetEnvironmentVariable("DOTNETCORE_ENVIRONMENT");

            //[Optional] Read configs; Additional settings, if any.
            //pipeline settings, actor specific settings,  etc. 
            //Read from <appsettings.json>: add "ActorConfiguration" section and bind it to a class.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var actorConfig = configuration.GetSection("AppSettings").Get<AppSettings>();
            var loggerConfig = configuration.GetSection("MILogger").Get<LoggerConfig>();

            SPGMI.ContainerHost.ServiceHelper.RegistertInstance<AppSettings>(actorConfig);
            ConfigurationProvider.GetPersistentStorageConfiguration(ref actorConfig, ref loggerConfig);

            //[Optional] MI logger
            IServiceCollection services = new ServiceCollection() ;
            services.AddMILogger(options => {
                //configuration.GetSection("MILogger").Bind(options);
                options.ApplicationName = loggerConfig.ApplicationName;
                options.BrokerList = loggerConfig.BrokerList;
                options.LogStrategy = loggerConfig.LogStrategy;
                options.Topic = loggerConfig.Topic;
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var logProvider = serviceProvider.GetRequiredService<ILoggerProvider>();
            var logger = SPGMI.Actor.Core.LogHelper.CreateLogger(logProvider, "SPGMI.Actors.InvestmentResearch.ResearchIndexResponseTracker");

            SPGMI.ContainerHost.ServiceHelper.RegistertInstance<ILogger>(logger);
            logger.LogInformation("Starting research index Response Tracker");

            IPipeline pipeline = ContentPipeline.Create();
            SPGMI.ContainerHost.ServiceHelper.RegistertInstance<IPipeline>(pipeline);

            IObjectSetPool<ContentReadyObjectSet> pool = pipeline.CreatePool<ContentReadyObjectSet>();
            SPGMI.ContainerHost.ServiceHelper.RegistertInstance<IObjectSetPool<ContentReadyObjectSet>>(pool);

            PoolSettings poolSettings = new PoolSettings()
            {
                MaxObjects = actorConfig.MaxObjects,
                MinObjects = actorConfig.MinObjects,
                MaxPayloads = actorConfig.MaxPayloads,
                MinPayloads = actorConfig.MinPayloads
            };

            PipelineSettings pipelineSettings = new PipelineSettings()
            {
                Pooling = poolSettings,
                DisableHold = true
            };

            //SPGMI.Actor.Core.LogHelper.LoggerFactory.AddConsole(LogLevel.Debug);
            //[Note] If you're using serviceInstance Name/Id in the subscriber name, 
            //then please note that each instance will have different name and id unlike legacy winservicehost framework.
            //serviceInstance IDs will be ranging from  [InstanceId] to [InstanceId + TotalInstances - 1], and
            //serviceInstanceName will be [InstanceName + InstanceId].
            foreach (var containerInstance in containerInstances)
            {
                //some postfix
                containerInstance.InstanceName += actorConfig.AppName;
            }

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
                    using (SPGMI.Config.Interface.IConfigurationSource configSource = ConfigurationSource.Create("ResearchIndexResponseTracker"))
                    {
                        ConfigReader reader = new ConfigReader(configSource.Configuration.GetSubKey(ConfigurationValueSection));
                        if (reader != null)
                        {
                            appSettings.AccessKey = reader.ReadString("AccessKey");
                            appSettings.AppName = reader.ReadString("AppName");
                            appSettings.BatchProcessor = Convert.ToBoolean(reader.ReadString("BatchProcessor"));
                            appSettings.BatchSize = Convert.ToInt32(reader.ReadString("BatchSize"));
                            appSettings.BatchTimeoutMs = Convert.ToInt32(reader.ReadString("BatchTimeoutMs"));
                            appSettings.Domain = reader.ReadString("Domain");
                            appSettings.ElectorSubject = reader.ReadString("ElectorSubject");
                            appSettings.ElectorTransport = reader.ReadString("ElectorTransport");
                            appSettings.EnvironmentSector = reader.ReadString("EnvironmentSector");
                            appSettings.MappingEnvironmentSector = reader.ReadString("MappingEnvironmentSector");
                            appSettings.SubscriptionNamespace = reader.ReadString("SubscriptionNamespace");
                            appSettings.User = reader.ReadString("User");
                            appSettings.BrokerAddress = reader.ReadString("BrokerAddress");
                            appSettings.TopicName = reader.ReadString("TopicName");
                            appSettings.ConsumerGroupName = reader.ReadString("ConsumerGroupName");

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
