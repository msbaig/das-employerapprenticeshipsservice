﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SFA.DAS.EAS.Domain.Attributes;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Infrastructure.DependencyResolution;
using SFA.DAS.Messaging;
using SFA.DAS.Messaging.AzureServiceBus;
using SFA.DAS.Messaging.FileSystem;
using StructureMap;

namespace SFA.DAS.EAS.Web.UnitTests.Infrastructure.StructureMapRegistrationTests
{
    public class WhenAddingMessaging
    {
        private Container _container;

        [SetUp]
        public void Arrange()
        {
            _container = new Container(
                c =>
                    {
                        c.AddRegistry<TestRegistry>();
                        c.Policies.Add(new MessagePolicy<EmployerApprenticeshipsServiceConfiguration>("SFA.DAS.EmployerApprenticeshipsService"));
                    }
                );
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        [Test]
        public void ThenTheMessageQueueIsTakenFromTheQueueNameAttribute()
        {
            //Act
            var actual = _container.GetInstance<TestClass>();

            //Assert
            Assert.IsAssignableFrom<AzureServiceBusMessageService>(actual.MessagePublisher);
            var actualMessageService = actual.MessagePublisher as AzureServiceBusMessageService;

            Assert.IsNotNull(actualMessageService);
            var queueNameField = actualMessageService
                                    .GetType()
                                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                                    .ToList()
                                    .FirstOrDefault(c => c.Name == "_queueName");
            Assert.IsNotNull(queueNameField);
            Assert.AreEqual(nameof(TestClass.das_queue_name), queueNameField.GetValue(actualMessageService));
            
        }

        [Test]
        public void ThenFileBasedMessageQueueIsResolvedIfNoConnnectionStringForServiceBusIsSupplied()
        {
            //Arrange
            _container = new Container(
                c =>
                    {
                        c.AddRegistry<TestRegistry>();
                        c.Policies.Add(new MessagePolicy<EmployerApprenticeshipsServiceConfiguration>("SFA.DAS.EmployerApprenticeshipsService_no_SB"));
                    }
                );

            //Act
            var actual = _container.GetInstance<TestClass>();

            //Assert
            Assert.IsAssignableFrom<FileSystemMessageService>(actual.MessagePublisher);
            var actualMessageService = actual.MessagePublisher as FileSystemMessageService;

            Assert.IsNotNull(actualMessageService);
            var storageDirectoryField = actualMessageService
                                    .GetType()
                                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                                    .ToList()
                                    .FirstOrDefault(c => c.Name == "_storageDirectory");
            Assert.IsNotNull(storageDirectoryField);
            var expectedDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(TestClass.das_queue_name));
            Assert.AreEqual(expectedDirectory, storageDirectoryField.GetValue(actualMessageService));
        }

        [Test]
        public void ThenThePollingReceiverIsResolvedThroughThePolicy()
        {
            //Arrange
            _container = new Container(
                c =>
                    {
                        c.AddRegistry<TestRegistryPolling>();
                        c.Policies.Add(new MessagePolicy<EmployerApprenticeshipsServiceConfiguration>("SFA.DAS.EmployerApprenticeshipsService"));
                    }
                );

            //Act
            var actual = _container.GetInstance<TestClassPolling>();

            Assert.IsAssignableFrom<AzureServiceBusMessageService>(actual.PollingMessageReceiver);
            var actualMessageService = actual.PollingMessageReceiver as AzureServiceBusMessageService;

            Assert.IsNotNull(actualMessageService);
            var queueNameField = actualMessageService
                                    .GetType()
                                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                                    .ToList()
                                    .FirstOrDefault(c => c.Name == "_queueName");
            Assert.IsNotNull(queueNameField);
            Assert.AreEqual(nameof(TestClassPolling.das_polling_queue_name), queueNameField.GetValue(actualMessageService));
        }

        [Test]
        public void ThenThePollingRecieverWillUseTheConnectionCollectionIfTheAttributeConstructorPropertiesAreSet()
        {
            //Arrange
            _container = new Container(
                c =>
                    {
                        c.AddRegistry<TestRegistryPolling>();
                        c.Policies.Add(new MessagePolicy<EmployerApprenticeshipsServiceConfiguration>("SFA.DAS.EmployerApprenticeshipsService"));
                    }
                );

            //Act
            var actual = _container.GetInstance<TestClass2>();

            //Assert
            Assert.IsAssignableFrom<AzureServiceBusMessageService>(actual.MessagePublisher);
            var actualMessageService = actual.MessagePublisher as AzureServiceBusMessageService;

            Assert.IsNotNull(actualMessageService);
            var connectionString = actualMessageService
                                    .GetType()
                                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                                    .ToList()
                                    .FirstOrDefault(c => c.Name == "_connectionString");
            Assert.IsNotNull(connectionString);
            Assert.AreEqual("Test_ServiceBusConnectionString2", connectionString.GetValue(actualMessageService));
        }

        [Test]
        public void ThenThePollingRecieverWillUseTheConstructorAttributeIfPresentToDetermineTheConnection()
        {
            //Arrange
            _container = new Container(
                c =>
                {
                    c.AddRegistry<TestRegistryPolling>();
                    c.Policies.Add(new MessagePolicy<EmployerApprenticeshipsServiceConfiguration>("SFA.DAS.EmployerApprenticeshipsService"));
                }
                );

            //Act
            var actual = _container.GetInstance<TestClass3>();

            //Assert
            Assert.IsAssignableFrom<AzureServiceBusMessageService>(actual.MessagePublisher);
            var actualMessageService = actual.MessagePublisher as AzureServiceBusMessageService;

            Assert.IsNotNull(actualMessageService);
            var connectionString = actualMessageService
                                    .GetType()
                                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                                    .ToList()
                                    .FirstOrDefault(c => c.Name == "_connectionString");
            Assert.IsNotNull(connectionString);
            Assert.AreEqual("Test_ServiceBusConnectionString1", connectionString.GetValue(actualMessageService));
        }

        public interface ITestClass{}
        public interface ITestClass2{}
        public interface ITestClass3{}

        public class TestClass : ITestClass
        {
            [Messaging.Attributes.QueueName()]
            public string das_queue_name { get; set; }

            public readonly IMessagePublisher MessagePublisher;

            public TestClass(IMessagePublisher messagePublisher)
            {
                MessagePublisher = messagePublisher;
            }
        }

        public class TestClass2 : ITestClass2
        {
            [Messaging.Attributes.QueueName("employer_levy")]
            public string das_queue_name { get; set; }

            public readonly IMessagePublisher MessagePublisher;

            [ServiceBusConnectionKey("employer_payment")]
            public TestClass2(IMessagePublisher messagePublisher)
            {
                MessagePublisher = messagePublisher;
            }
        }

        public class TestClass3 : ITestClass3
        {
            public readonly IMessagePublisher MessagePublisher;

            [ServiceBusConnectionKey("employer_payment")]
            public TestClass3(IMessagePublisher messagePublisher)
            {
                MessagePublisher = messagePublisher;
            }
        }

        public class TestClassPolling : ITestClass
        {
            [Messaging.Attributes.QueueName]
            public string das_polling_queue_name { get; set; }

            public readonly IPollingMessageReceiver PollingMessageReceiver;

            public TestClassPolling(IPollingMessageReceiver pollingMessageReceiver)
            {
                PollingMessageReceiver = pollingMessageReceiver;
            }
        }

        public class TestClassPolling2 : ITestClass2
        {
            [Messaging.Attributes.QueueName]
            public string das_polling_queue_name { get; set; }

            public readonly IPollingMessageReceiver PollingMessageReceiver;

            public TestClassPolling2(IPollingMessageReceiver pollingMessageReceiver)
            {
                PollingMessageReceiver = pollingMessageReceiver;
            }
        }

        public class TestRegistry : Registry
        {
            public TestRegistry()
            {
                For<ITestClass>().Use<TestClass>();
                For<ITestClass2>().Use<TestClass2>();
                For<ITestClass3>().Use<TestClass3>();
            }
        }

        public class TestRegistryPolling : Registry
        {
            public TestRegistryPolling()
            {
                For<ITestClass>().Use<TestClassPolling>();
                For<ITestClass2>().Use<TestClassPolling2>();
            }
        }
    }
}
