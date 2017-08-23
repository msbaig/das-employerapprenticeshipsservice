using System.Web.Mvc;
using NUnit.Framework;
using SFA.DAS.EAS.Web.ViewModels.Organisation;

namespace SFA.DAS.EAS.Web.UnitTests.Controllers.SearchOrganisationControllerTests
{
    public class WhenIConfirmAnAddress : SearchOrganisationControllerTestsBase
    {
        public override void CustomSetup()
        { }          


        [Test]
        public void IfIHaveNoAccountIdThenShowsFindAddressFromEmployerAccountOrganisation()
        {
            var model = new OrganisationDetailsViewModel();
            var result = _controller.Confirm(null, model) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewName.Contains("FindAddress"));
            Assert.IsTrue(result.ViewName.Contains("EmployerAccountOrganisation/FindAddress"));
            Assert.IsTrue(result.Model.GetType()==typeof(OrchestratorResponse<FindOrganisationAddressViewModel>));
            Assert.AreEqual("true", result.ViewBag.HideNav);
        }

        [Test]
        public void IfIHaveAnAccountIdThenShowsFindAddressFromOrganisation()
        {
            var model = new OrganisationDetailsViewModel();
            var result = _controller.Confirm("hasAccountId", model) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewName.Contains("FindAddress"));
            Assert.IsTrue(result.ViewName.Contains("Organisation/FindAddress"));
            Assert.IsTrue(result.Model.GetType() == typeof(OrchestratorResponse<FindOrganisationAddressViewModel>));
            Assert.NotNull(result.ViewData);
            Assert.AreEqual("false", result.ViewBag.HideNav);
        }
    }
}
