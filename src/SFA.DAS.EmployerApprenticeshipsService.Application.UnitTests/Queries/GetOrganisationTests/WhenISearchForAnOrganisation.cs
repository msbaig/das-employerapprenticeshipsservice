﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Application.Queries.GetOrganisations;
using SFA.DAS.EAS.Application.Validation;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Organisation;
using SFA.DAS.EAS.Domain.Models.ReferenceData;

namespace SFA.DAS.EAS.Application.UnitTests.Queries.GetOrganisationTests
{
    public class WhenISearchForAnOrganisation :QueryBaseTest<GetOrganisationsQueryHandler, GetOrganisationsRequest, GetOrganisationsResponse>
    {
        private Mock<IReferenceDataService> _referenceDataService;
        public override GetOrganisationsRequest Query { get; set; }
        public override GetOrganisationsQueryHandler RequestHandler { get; set; }
        public override Mock<IValidator<GetOrganisationsRequest>> RequestValidator { get; set; }

        [SetUp]
        public void Arrange()
        {
            SetUp();

            _referenceDataService = new Mock<IReferenceDataService>();

            Query = new GetOrganisationsRequest {SearchTerm ="Company Search"};

            RequestHandler = new GetOrganisationsQueryHandler(RequestValidator.Object, _referenceDataService.Object);
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
        {
            //Arrange
            var expectedSearchTerm = "My Company";

            //Act
            await RequestHandler.Handle(new GetOrganisationsRequest {SearchTerm = expectedSearchTerm, PageNumber = 3, OrganisationType = OrganisationType.Charities });

            //Assert
            _referenceDataService.Verify(x => x.SearchOrganisations(expectedSearchTerm, 3, 20, OrganisationType.Charities), Times.Once);
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
        {
            //Arrange
            var expectedResponse = new PagedResponse<Organisation> { Data = new List<Organisation> { new Organisation() } };
            var expectedSearchTerm = "My Company";
            _referenceDataService.Setup(x => x.SearchOrganisations(expectedSearchTerm, 2, 20, null)).ReturnsAsync(expectedResponse);

            //Act
            var actual = await RequestHandler.Handle(new GetOrganisationsRequest { SearchTerm = expectedSearchTerm, PageNumber = 2 });

            //Assert
            Assert.AreSame(expectedResponse, actual.Organisations);
        }
    }
}
