using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Web.ViewModels.Organisation;

namespace SFA.DAS.EAS.Web.UnitTests.Controllers.SearchOrganisationControllerTests
{
    class WhenIEnterAPostcodeForAnAddress : SearchOrganisationControllerTestsBase
    {
        private SelectOrganisationAddressViewModel _viewModel;

        public override void CustomSetup()
        {
            _viewModel = new SelectOrganisationAddressViewModel();

            _orchestrator.Setup(x => x.GetAddressesFromPostcode(It.IsAny<FindOrganisationAddressViewModel>()))
                .ReturnsAsync(new OrchestratorResponse<SelectOrganisationAddressViewModel>()
                {
                    Data = _viewModel,
                    Status = HttpStatusCode.OK
                });

            _mapper.Setup(x => x.Map<AddOrganisationAddressViewModel>(It.IsAny<FindOrganisationAddressViewModel>()))
                .Returns(new AddOrganisationAddressViewModel()
                {
                    Address = new AddressViewModel()
                });
        }

        [Test]
        public async Task ThenIfASingleAddressIsFoundTheAddressViewShouldBePresented()
        {
            //Arange
            _viewModel.Addresses = new List<AddressViewModel>
            {
                new AddressViewModel()
            };

            //Act
            var viewResult = await _controller.SelectAddress(new FindOrganisationAddressViewModel()) as ViewResult;

            //Assert
            Assert.AreEqual("AddOrganisationAddress", viewResult?.ViewName);
        }

        [Test]
        public async Task ThenIfMultiAddressesAreFoundTheSelectAddressViewShouldBePresented()
        {
            //Arange
            _viewModel.Addresses = new List<AddressViewModel>
            {
                new AddressViewModel(),
                new AddressViewModel()
            };

            //Act
            var viewResult = await _controller.SelectAddress(new FindOrganisationAddressViewModel()) as ViewResult;

            //Assert
            Assert.IsEmpty(viewResult?.ViewName); //Empty view name will go to the select address View
        }
    }
}
