using System.Web.Mvc;
using AutoMapper;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Web.Authentication;
using SFA.DAS.EAS.Web.Controllers;
using SFA.DAS.EAS.Web.Orchestrators;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.EAS.Web.ViewModels.Organisation;

namespace SFA.DAS.EAS.Web.UnitTests.Controllers.SearchOrganisationControllerTests
{
    public class WhenIConfirmAnAddress
    {
        private SearchOrganisationController _controller;
        private Mock<IOwinWrapper> _owinWrapper;
        private Mock<SearchOrganisationOrchestrator> _orchestrator;
        private Mock<IFeatureToggle> _featureToggle;
        private Mock<IMultiVariantTestingService> _multiVariantTestingService;
        private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
        private Mock<IMapper> _mapper;

        [SetUp]
        public void Arrange()
        {
            _owinWrapper = new Mock<IOwinWrapper>();
            _orchestrator = new Mock<SearchOrganisationOrchestrator>();
            _featureToggle=new Mock<IFeatureToggle>();
            _multiVariantTestingService=new Mock<IMultiVariantTestingService>();
            _flashMessage=new Mock<ICookieStorageService<FlashMessageViewModel>>();
            _mapper=new Mock<IMapper>();

            _controller=new SearchOrganisationController(_owinWrapper.Object,null,_featureToggle.Object,_multiVariantTestingService.Object, _flashMessage.Object,_mapper.Object);
        }

        [Test]
        public void IfIHaveNoAccountIdThenShowsFindAddressFromEmployerAccountOrganisation()
        {
            var model = new OrganisationDetailsViewModel();
            var result = _controller.Confirm(null, model) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ViewName.Contains("FindAddress"));
            Assert.IsTrue(result.ViewName.Contains("EmployerAccountOrganisation/FindAddress"));
            Assert.IsTrue(result.Model.GetType()==typeof(OrchestratorResponse<FindOrganisationAddressViewModel>));
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
        }
    }
}
