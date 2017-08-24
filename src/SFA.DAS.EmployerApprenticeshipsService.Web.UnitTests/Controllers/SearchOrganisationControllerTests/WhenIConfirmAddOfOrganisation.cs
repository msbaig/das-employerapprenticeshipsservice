using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Web.ViewModels.Organisation;

namespace SFA.DAS.EAS.Web.UnitTests.Controllers.SearchOrganisationControllerTests
{
    public class WhenIConfirmAddOfOrganisation : SearchOrganisationControllerTestsBase
    {
        private const string HashedId = "hashedId";

        public override void CustomSetup()
        {
            _orchestrator.Setup(a => a.GetCookieData(It.IsAny<HttpContextBase>())).Returns(new EmployerAccountData());
        }

        [Test]
        public void ThenIfIHaveNoAddressDetailsThenBackToFindAddress()
        {
            //Act
            var result = _controller.Confirm(HashedId, new OrganisationDetailsViewModel()) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewName.Contains("FindAddress"));
        }

        [Test]
        public void ThenIfIHaveAnAccountAmThenConfirmOrganisationDetails()
        {
            //Act
            var result = _controller.Confirm(HashedId, new OrganisationDetailsViewModel(){Address = "address"}) as ViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewName.Contains("ConfirmOrganisationDetails"));
        }

        [Test]
        public void ThenIfIDoNotHaveAnAccountAmThenGoToGatewayInform()
        {
            //Act
            var result = _controller.Confirm(null, new OrganisationDetailsViewModel() { Address = "address" }) as RedirectToRouteResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("GatewayInform", result.RouteValues["Action"]);
        }
    }
}
