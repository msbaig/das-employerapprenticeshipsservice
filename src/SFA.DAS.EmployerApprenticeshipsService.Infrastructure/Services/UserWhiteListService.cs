﻿using System;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Azure;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Configuration.FileStorage;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Interfaces;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Models.WhileList;
using SFA.DAS.EmployerApprenticeshipsService.Infrastructure.Caching;

namespace SFA.DAS.EmployerApprenticeshipsService.Infrastructure.Services
{
    public class UserWhiteListService : IUserWhiteList
    {
        private readonly ICacheProvider _cacheProvider;
        private const string ConfigurationName = "SFA.DAS.EmployerApprenticeshipsService.WhiteList";
        public UserWhiteListService(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }
        
        public bool IsEmailOnWhiteList(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            var whiteList = GetList();

            return whiteList?.EmailPatterns?.Any(pattern => Regex.IsMatch(email, pattern)) ?? false;
        }

        private UserWhiteListLookUp GetList()
        {
            var whiteListLookUp = _cacheProvider.Get<UserWhiteListLookUp>(nameof(UserWhiteListLookUp));

            if (whiteListLookUp != null)
                return whiteListLookUp;

            whiteListLookUp = GetWhiteList();

            if (whiteListLookUp?.EmailPatterns == null || !whiteListLookUp.EmailPatterns.Any())
                return null;

            _cacheProvider.Set(nameof(UserWhiteListLookUp), whiteListLookUp, new TimeSpan(0, 30, 0));

            return whiteListLookUp;
        }

        public virtual UserWhiteListLookUp GetWhiteList()
        {
            var environment = Environment.GetEnvironmentVariable("DASENV");
            if (string.IsNullOrEmpty(environment))
            {
                environment = CloudConfigurationManager.GetSetting("EnvironmentName");
            }

            var configurationRepository = GetDataFromAzure();
            var configurationService = new ConfigurationService(
               configurationRepository,
               new ConfigurationOptions(ConfigurationName, environment, "1.0"));

            var config = configurationService.Get<UserWhiteListLookUp>();

            return config;
        }


        private static IConfigurationRepository GetDataFromAzure()
        {
            IConfigurationRepository configurationRepository;
            if (bool.Parse(ConfigurationManager.AppSettings["LocalConfig"]))
            {
                configurationRepository = new FileStorageConfigurationRepository();
            }
            else
            {
                configurationRepository =
                    new AzureTableStorageConfigurationRepository(
                        CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
            }
            return configurationRepository;
        }
    }
}
