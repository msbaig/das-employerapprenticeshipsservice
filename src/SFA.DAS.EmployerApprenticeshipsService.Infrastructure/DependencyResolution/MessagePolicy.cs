﻿using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.Azure;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Configuration.FileStorage;
using SFA.DAS.EAS.Domain.Attributes;
using SFA.DAS.Messaging;
using SFA.DAS.Messaging.AzureServiceBus;
using SFA.DAS.Messaging.FileSystem;
using StructureMap;
using StructureMap.Pipeline;
using IConfiguration = SFA.DAS.EAS.Domain.Interfaces.IConfiguration;
using QueueNameAttribute = SFA.DAS.Messaging.Attributes.QueueNameAttribute;

namespace SFA.DAS.EAS.Infrastructure.DependencyResolution
{
    public class MessagePolicy<T> : ConfiguredInstancePolicy where T : IConfiguration
    {
        private readonly string _serviceName;

        public MessagePolicy(string serviceName)
        {
            _serviceName = serviceName;
        }

        protected override void apply(Type pluginType, IConfiguredInstance instance)
        {
            var messagePublisher = instance?.Constructor?
                .GetParameters().FirstOrDefault(x => x.ParameterType == typeof(IMessagePublisher) || x.ParameterType == typeof(IPollingMessageReceiver));

            var environment = Environment.GetEnvironmentVariable("DASENV");
            if (string.IsNullOrEmpty(environment))
            {
                environment = CloudConfigurationManager.GetSetting("EnvironmentName");
            }

            if (messagePublisher != null)
            {
                var queueName = instance.SettableProperties()
                                .FirstOrDefault(c=>c.CustomAttributes.FirstOrDefault(
                                                    x=>x.AttributeType.Name == nameof(QueueNameAttribute)) != null);
               
                var configurationService = new ConfigurationService(GetConfigurationRepository(), new ConfigurationOptions(_serviceName, environment, "1.0"));

                var config = configurationService.Get<T>();
                if (string.IsNullOrEmpty(config.ServiceBusConnectionString))
                {
                    var queueFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    instance.Dependencies.AddForConstructorParameter(messagePublisher, new FileSystemMessageService(Path.Combine(queueFolder, queueName?.Name ?? string.Empty)));
                }
                else
                {
                    var serviceBusConnectionString = config.ServiceBusConnectionString;
                    
                    if (queueName?.CustomAttributes?.FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value != null)
                    {
                        var connectionKey = queueName.CustomAttributes?.FirstOrDefault()?.ConstructorArguments.FirstOrDefault().Value.ToString();
                            
                        serviceBusConnectionString = string.IsNullOrWhiteSpace(connectionKey)? serviceBusConnectionString : config.ServiceBusConnectionStrings[connectionKey];
                    }
                    else if (instance.Constructor?.CustomAttributes?.FirstOrDefault(x => x.AttributeType.Name == nameof(ServiceBusConnectionKeyAttribute))?.ConstructorArguments.FirstOrDefault().Value != null)
                    {
                        var connectionKey = instance.Constructor.CustomAttributes.FirstOrDefault(x => x.AttributeType.Name == nameof(ServiceBusConnectionKeyAttribute))
                                .ConstructorArguments.FirstOrDefault()
                                .Value.ToString();

                        serviceBusConnectionString = string.IsNullOrWhiteSpace(connectionKey) ? serviceBusConnectionString : config.ServiceBusConnectionStrings[connectionKey];
                    }
                        
                    instance.Dependencies.AddForConstructorParameter(messagePublisher, new AzureServiceBusMessageService(serviceBusConnectionString, queueName?.Name ?? string.Empty));
                }   
            }
        }

        private static IConfigurationRepository GetConfigurationRepository()
        {
            IConfigurationRepository configurationRepository;
            if (bool.Parse(ConfigurationManager.AppSettings["LocalConfig"]))
            {
                configurationRepository = new FileStorageConfigurationRepository();
            }
            else
            {
                configurationRepository = new AzureTableStorageConfigurationRepository(CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
            }
            return configurationRepository;
        }
        
    }
}