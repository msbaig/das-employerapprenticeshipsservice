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
        public void IfIHaveNoAccountIdThenShowsFindAddressWithNavHidden()
        {
            var model = new OrganisationDetailsViewModel();
            var result = _controller.Confirm(null, model) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewName.Contains("FindAddress"));
            Assert.IsTrue(result.Model.GetType()==typeof(OrchestratorResponse<FindOrganisationAddressViewModel>));
            Assert.AreEqual("true", result.ViewBag.HideNav);
            Assert.IsNull(result.ViewBag.Section);
        }

        [Test]
        public void IfIHaveAnAccountIdThenShowsFindAddressWithNavShown()
        {
            var model = new OrganisationDetailsViewModel();
            var result = _controller.Confirm("hasAccountId", model) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewName.Contains("FindAddress"));
            Assert.IsTrue(result.Model.GetType() == typeof(OrchestratorResponse<FindOrganisationAddressViewModel>));
            Assert.AreEqual("false", result.ViewBag.HideNav);
            Assert.AreEqual("organisations", result.ViewBag.Section);
        }
    }
}
