﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Configuration;
using Moq;
using SFA.DAS.EAS.Application.Messages;
using SFA.DAS.EAS.Application.Queries.GetHMRCLevyDeclaration;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.LevyDeclarationProvider.Worker.Providers;
using SFA.DAS.EAS.TestCommon.DependencyResolution;
using SFA.DAS.EAS.TestCommon.ObjectMothers;
using SFA.DAS.Messaging;
using SFA.DAS.Messaging.Attributes;
using StructureMap;
using TechTalk.SpecFlow;

namespace SFA.DAS.EAS.TestCommon.ScenarioCommonSteps
{
    public class LevyWorkerSteps : IDisposable
    {
        [QueueName]
        public string get_employer_levy { get; set; }

        private readonly IContainer _container;
        private readonly Mock<IPollingMessageReceiver> _messageReceiver;
        private readonly Mock<IHmrcService> _hmrcService;

        public LevyWorkerSteps()
        {
            //Used to set is processing of declarations should occur
            WebConfigurationManager.AppSettings["DeclarationsEnabled"] = "both";

            _messageReceiver = new Mock<IPollingMessageReceiver>();
            _hmrcService = new Mock<IHmrcService>();

            _container = IoC.CreateLevyWorkerContainer(new Mock<IMessagePublisher>().Object, _messageReceiver.Object, _hmrcService.Object);
        }

        public void RunWorker(IEnumerable<GetHMRCLevyDeclarationResponse> hmrcLevyResponses)
        {
            var hashingService = _container.GetInstance<IHashingService>();
            var levyDeclaration = _container.GetInstance<ILevyDeclaration>();
            var levyDeclarationResponses = hmrcLevyResponses as GetHMRCLevyDeclarationResponse[] ?? hmrcLevyResponses.ToArray();

            var cancellationTokenSource = new CancellationTokenSource();

            var accountId = GetCurrentAccountId(hashingService);

            var payeSchemes = levyDeclarationResponses.Select(x => x.Empref).Distinct();

            SetupRefreshLevyMockMessageQueue(payeSchemes, accountId, cancellationTokenSource);

            foreach (var declarationResponse in levyDeclarationResponses)
            {
                _hmrcService.Setup(x => x.GetLevyDeclarations(declarationResponse.Empref, It.IsAny<DateTime?>()))
                    .ReturnsAsync(declarationResponse.LevyDeclarations);
            }

            levyDeclaration.RunAsync(cancellationTokenSource.Token).Wait(5000);
        }

        private static long GetCurrentAccountId(IHashingService hashingService)
        {
            var hashedAccountId = ScenarioContext.Current["HashedAccountId"] as string;
            var accountId = hashingService.DecodeValue(hashedAccountId);
            return accountId;
        }

        private void SetupRefreshLevyMockMessageQueue(IEnumerable<string> payeSchemes, long accountId,
            CancellationTokenSource cancellationTokenSource)
        {
            var setupSequence = _messageReceiver.SetupSequence(x => x.ReceiveAsAsync<EmployerRefreshLevyQueueMessage>());

            foreach (var scheme in payeSchemes)
            {
                var queueMessage = new EmployerRefreshLevyQueueMessage
                {
                    AccountId = accountId,
                    PayeRef = scheme
                };

                var mockMessage = MessageObjectMother.Create(queueMessage, cancellationTokenSource.Cancel, null);

                setupSequence = setupSequence.ReturnsAsync(mockMessage);
            }
        }

        public void Dispose()
        {
            _container?.Dispose();
        }
    }
}
