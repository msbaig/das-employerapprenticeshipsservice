using System;
using System.Linq;
using System.Web;
using MediatR;
using Moq;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.EAS.Application.Queries.GetAccountPayeSchemes;
using SFA.DAS.EAS.Application.Queries.GetUserAccounts;
using SFA.DAS.EAS.Application.Validation;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.TestCommon.DependencyResolution;
using SFA.DAS.EAS.TestCommon.Models;
using SFA.DAS.EAS.TestCommon.ObjectMothers;
using SFA.DAS.EAS.Web.Authentication;
using SFA.DAS.EAS.Web.Orchestrators;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Messaging;
using StructureMap;
using TechTalk.SpecFlow;

namespace SFA.DAS.EAS.TestCommon.ScenarioCommonSteps
{
    public class AccountSteps
    {
        private readonly IContainer _container;

        public AccountSteps()
        {
            var messagePublisher = new Mock<IMessagePublisher>();
            var owinWrapper = new Mock<IOwinWrapper>();
            var cookieService = new Mock<ICookieStorageService<EmployerAccountData>>();
            var eventsApi = new Mock<IEventsApi>();
            var validator = new Mock<IValidator<GetAccountPayeSchemesQuery>>();
            var employerCommitmentsApi = new Mock<IEmployerCommitmentApi>();

            validator.Setup(x => x.ValidateAsync(It.IsAny<GetAccountPayeSchemesQuery>()))
                .ReturnsAsync(new ValidationResult());

            _container = IoC.CreateContainer(messagePublisher, owinWrapper, cookieService, eventsApi, employerCommitmentsApi);

            _container.Inject(validator.Object);
        }

        public static void SetAccountIdForUser(IMediator mediator, ScenarioContext scenarioContext)
        {
            var accountOwnerId = scenarioContext["AccountOwnerUserId"].ToString();
            var getUserAccountsQueryResponse = mediator.SendAsync(new GetUserAccountsQuery { UserRef = accountOwnerId }).Result;

            var account = getUserAccountsQueryResponse.Accounts.AccountList.FirstOrDefault();

            scenarioContext["AccountId"] = account?.Id;
            scenarioContext["HashedAccountId"] = account?.HashedId;
        }

        public void CreateAccountWithOwner()
        {
            var user = CreateUser();

            CreateDasAccount(user.UserId, "Test Company");
        }

        public UserViewModel CreateUser()
        {
            var accountOwnerUserId = Guid.NewGuid().ToString();
            ScenarioContext.Current["AccountOwnerUserId"] = accountOwnerUserId;

            var signInUserModel = new UserViewModel
            {
                UserId = accountOwnerUserId,
                Email = "accountowner@test.com" + Guid.NewGuid().ToString().Substring(0, 6),
                FirstName = "Test",
                LastName = "Tester"
            };
            var userCreationSteps = new UserSteps();
            userCreationSteps.UpsertUser(signInUserModel);

            return userCreationSteps.GetExistingUserAccount();
        }

        public CreatedTestAccountDetails CreateDasAccount(string ownerUserId, string organisationName)
        {
            var orchestrator = _container.GetInstance<EmployerAccountOrchestrator>();

            var accountViewModel = EmployerAccountObjectMother.CreateViewModel(ownerUserId, organisationName);

            var result = orchestrator.CreateAccount(accountViewModel, new Mock<HttpContextBase>().Object).Result;

            return new CreatedTestAccountDetails
            {
                AccountId = result.Data.EmployerAgreement.AccountId,
                HashedAccountId = result.Data.EmployerAgreement.HashedAccountId,
                UserId = ownerUserId,
                OrganisationName = accountViewModel.OrganisationName,
                OrganisationAddress = accountViewModel.OrganisationAddress,
                OrganisationDateOfInception = accountViewModel.OrganisationDateOfInception,
                OrganisationReferenceNumber = accountViewModel.OrganisationReferenceNumber,
                OrganisationStatus = accountViewModel.OrganisationStatus,
                OrganisationType = accountViewModel.OrganisationType
            };
        }
    }
}
