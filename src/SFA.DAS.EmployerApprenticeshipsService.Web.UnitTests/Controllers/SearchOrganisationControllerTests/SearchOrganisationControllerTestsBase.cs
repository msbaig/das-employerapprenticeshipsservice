using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Web.Authentication;
using SFA.DAS.EAS.Web.Controllers;
using SFA.DAS.EAS.Web.Orchestrators;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EAS.Web.UnitTests.Controllers.SearchOrganisationControllerTests
{
    public abstract class SearchOrganisationControllerTestsBase
    {
        protected SearchOrganisationController _controller;
        protected Mock<IOwinWrapper> _owinWrapper;
        protected Mock<SearchOrganisationOrchestrator> _orchestrator;
        protected Mock<IFeatureToggle> _featureToggle;
        protected Mock<IMultiVariantTestingService> _multiVariantTestingService;
        protected Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
        protected Mock<IMapper> _mapper;

        public void Setup()
        {
            _owinWrapper = new Mock<IOwinWrapper>();
            _orchestrator = new Mock<SearchOrganisationOrchestrator>();
            _featureToggle = new Mock<IFeatureToggle>();
            _multiVariantTestingService = new Mock<IMultiVariantTestingService>();
            _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();
            _mapper = new Mock<IMapper>();

            _controller = new SearchOrganisationController(_owinWrapper.Object, null, _featureToggle.Object, _multiVariantTestingService.Object, _flashMessage.Object, _mapper.Object);
        }
    }
}
