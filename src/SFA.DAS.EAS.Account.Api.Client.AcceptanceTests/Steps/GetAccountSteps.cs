using System;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Infrastructure.DependencyResolution;
using SFA.DAS.EAS.TestCommon.DependencyResolution;
using SFA.DAS.EAS.TestCommon.Models;
using SFA.DAS.EAS.TestCommon.ScenarioCommonSteps;
using SFA.DAS.EAS.Web.Authentication;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Messaging;
using StructureMap;
using TechTalk.SpecFlow;

namespace SFA.DAS.EAS.Account.Api.Client.AcceptanceTests.Steps
{
    [Binding]
    public class GetAccountSteps
    {
        private IContainer _container;
        private readonly AccountSteps _accountSteps;
        private readonly AccountApiClient _apiClient;

        public GetAccountSteps()
        {
            var messagePublisher = new Mock<IMessagePublisher>();
            var owinWrapper = new Mock<IOwinWrapper>();
            var cookieService = new Mock<ICookieStorageService<EmployerAccountData>>();
            var eventsApi = new Mock<IEventsApi>();
            var commitmentsApi = new Mock<IEmployerCommitmentApi>();

            _container = IoC.CreateContainer(messagePublisher, owinWrapper, cookieService, eventsApi, commitmentsApi);

            _accountSteps = new AccountSteps();

            var config = ConfigurationHelper.GetConfiguration<IAccountApiConfiguration>("SFA.DAS.EmployerAccountAPI");

            _apiClient = new AccountApiClient(config);
        }

        [Given(@"I am a user")]
        public void GivenIAmAUser()
        {
            var user = _accountSteps.CreateUser();

            ScenarioContext.Current.Add("User", user);
        }

        [Given(@"I have an account named (.*)")]
        public void GivenIHaveAnAccount(string accountName)
        {
            var user = ScenarioContext.Current["User"] as UserViewModel;

            var account = _accountSteps.CreateDasAccount(user?.UserId, accountName);

            ScenarioContext.Current.Add("Account_" + accountName, account);
        }
        
        [When(@"I lookup (.*) account details using the accounts hashed ID")]
        public void WhenILookupMyAccountDetailsUsingTheAccountsHashedID(string accountName)
        {
            var expectedAccountDetails = ScenarioContext.Current["Account_" + accountName] as CreatedTestAccountDetails;

            var actualAccountDetails = _apiClient.GetAccount(expectedAccountDetails?.HashedAccountId).Result;

            ScenarioContext.Current.Add("Actual_Account_" + accountName, actualAccountDetails);
        }
        
        [When(@"I lookup (.*) account details using the accounts ID")]
        public void WhenILookupMyAccountDetailsUsingTheAccountsID(string accountName)
        {
            var expectedAccountDetails = ScenarioContext.Current["Account_" + accountName] as CreatedTestAccountDetails;

            var actualAccountDetails = _apiClient.GetAccount(expectedAccountDetails?.AccountId ?? 0).Result;

            ScenarioContext.Current.Add("Actual_Account_" + accountName, actualAccountDetails);
        }
        
        [Then(@"the (.*) account details should be the same as the account I created")]
        public void ThenTheAccountDetailsShouldBeTheSameAsTheAccountICreated(string accountName)
        {
            var expectedAccountDetails = ScenarioContext.Current["Account_" + accountName] as CreatedTestAccountDetails;
            var actualAccountDetails = ScenarioContext.Current["Actual_Account_" + accountName] as AccountDetailViewModel;

            Assert.IsNotNull(expectedAccountDetails);
            Assert.IsNotNull(actualAccountDetails);

            Assert.AreEqual(expectedAccountDetails.AccountId, actualAccountDetails.AccountId);
            Assert.AreEqual(expectedAccountDetails.HashedAccountId, actualAccountDetails.HashedAccountId);
            Assert.AreEqual(expectedAccountDetails.OrganisationName, actualAccountDetails.DasAccountName);
            Assert.AreEqual(DateTime.Now.ToString("d"), actualAccountDetails.DateRegistered.ToString("d"));
        }
    }
}
