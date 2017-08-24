using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using MediatR;
using SFA.DAS.EAS.Application;
using SFA.DAS.EAS.Application.Commands.CreateOrganisationAddress;
using SFA.DAS.EAS.Application.Queries.GetOrganisations;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Domain.Models.Organisation;
using SFA.DAS.EAS.Domain.Models.ReferenceData;
using SFA.DAS.EAS.Web.Helpers;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.EAS.Web.ViewModels.Organisation;
using SFA.DAS.EAS.Application.Queries.GetPostcodeAddress;

namespace SFA.DAS.EAS.Web.Orchestrators
{
    public class SearchOrganisationOrchestrator : UserVerificationOrchestratorBase
    {
        private readonly ICookieStorageService<EmployerAccountData> _cookieService;
        private const string CookieName = "sfa-das-employerapprenticeshipsservice-employeraccount";
        private readonly IMapper _mapper;

        protected SearchOrganisationOrchestrator() { }

        public SearchOrganisationOrchestrator(IMediator mediator, ICookieStorageService<EmployerAccountData> cookieService, IMapper mapper)
            : base(mediator)
        {
            _cookieService = cookieService;
            _mapper = mapper;
        }

        public async Task<OrchestratorResponse<SearchOrganisationResultsViewModel>> SearchOrganisation(string searchTerm, int pageNumber, OrganisationType? organisationType, string hashedAccountId, string userId)
        {
            var response = new OrchestratorResponse<SearchOrganisationResultsViewModel>();

            try
            {
                var result = await Mediator.SendAsync(new GetOrganisationsRequest { SearchTerm = searchTerm, PageNumber = pageNumber, OrganisationType = organisationType });
                response.Data = new SearchOrganisationResultsViewModel
                {
                    Results = CreateResult(result.Organisations),
                    SearchTerm = searchTerm,
                    OrganisationType = organisationType
                };

                if (!string.IsNullOrEmpty(hashedAccountId))
                {
                    await SetAlreadySelectedOrganisations(hashedAccountId, userId, response.Data.Results);
                }
            }
            catch (InvalidRequestException ex)
            {
                response.Exception = ex;
                response.FlashMessage = FlashMessageViewModel.CreateErrorFlashMessageViewModel(ex.ErrorMessages);
                response.Status = HttpStatusCode.BadRequest;
            }

            return response;
        }

        private async Task SetAlreadySelectedOrganisations(string hashedAccountId, string userId, PagedResponse<OrganisationDetailsViewModel> searchResults)
        {
            var accountLegalEntitiesHelper = new AccountLegalEntitiesHelper(Mediator);
            var accountLegalEntities = await accountLegalEntitiesHelper.GetAccountLegalEntities(hashedAccountId, userId);

            foreach (var searchResult in searchResults.Data)
            {
                searchResult.AddedToAccount = accountLegalEntitiesHelper.IsLegalEntityAlreadyAddedToAccount(accountLegalEntities, searchResult.Name, searchResult.ReferenceNumber, searchResult.Type);
            }
        }

        public virtual EmployerAccountData GetCookieData(HttpContextBase context)
        {
            return _cookieService.Get(CookieName);
        }

        public virtual void CreateCookieData(HttpContextBase context, EmployerAccountData data)
        {
            _cookieService.Create(data, CookieName, 365);
        }

        private PagedResponse<OrganisationDetailsViewModel> CreateResult(PagedResponse<Organisation> organisations)
        {
            return new PagedResponse<OrganisationDetailsViewModel>
            {
                PageNumber = organisations.PageNumber,
                TotalPages = organisations.TotalPages,
                TotalResults = organisations.TotalResults,
                Data = organisations.Data.Select(ConvertToViewModel).ToList()
            };
        }

        private OrganisationDetailsViewModel ConvertToViewModel(Organisation organisation)
        {
            return new OrganisationDetailsViewModel
            {
                Address = organisation.Address.GetAddress(),
                Name = organisation.Name,
                Type = organisation.Type,
                DateOfInception = organisation.RegistrationDate,
                ReferenceNumber = organisation.Code,
                PublicSectorDataSource = (short?)organisation.SubType,
                Sector = organisation.Sector
            };
        }

        //TC added

        public virtual async Task<OrchestratorResponse<SelectOrganisationAddressViewModel>> GetAddressesFromPostcode(
            FindOrganisationAddressViewModel request)
        {
            var viewModel = new SelectOrganisationAddressViewModel
            {
                Postcode = request.Postcode,
                OrganisationName = request.OrganisationName,
                OrganisationDateOfInception = request.OrganisationDateOfInception,
                OrganisationHashedId = request.OrganisationHashedId,
                OrganisationReferenceNumber = request.OrganisationReferenceNumber,
                OrganisationStatus = request.OrganisationStatus,
                OrganisationType = request.OrganisationType,
                PublicSectorDataSource = request.PublicSectorDataSource,
                Sector = request.Sector
            };

            try
            {
                var addresses = await Mediator.SendAsync(new GetPostcodeAddressRequest { Postcode = request.Postcode });

                viewModel.Addresses = addresses?.Addresses?.Select(x => _mapper.Map<AddressViewModel>(x)).ToList();

                return new OrchestratorResponse<SelectOrganisationAddressViewModel>
                {
                    Data = viewModel,
                    Status = HttpStatusCode.OK
                };
            }
            catch (InvalidRequestException e)
            {
                viewModel.ErrorDictionary = e.ErrorMessages;

                return new OrchestratorResponse<SelectOrganisationAddressViewModel>
                {
                    Data = viewModel,
                    Status = HttpStatusCode.BadRequest,
                    Exception = e
                };
            }
        }

        public virtual OrchestratorResponse<OrganisationDetailsViewModel> AddOrganisationAddress(
            AddOrganisationAddressViewModel viewModel)
        {
            try
            {
                var request = _mapper.Map<CreateOrganisationAddressRequest>(viewModel.Address);

                var response = Mediator.Send(request);

                return new OrchestratorResponse<OrganisationDetailsViewModel>
                {
                    Data = new OrganisationDetailsViewModel
                    {
                        HashedId = viewModel.OrganisationHashedId,
                        Name = viewModel.OrganisationName,
                        Address = response.Address,
                        DateOfInception = viewModel.OrganisationDateOfInception,
                        ReferenceNumber = viewModel.OrganisationReferenceNumber ?? string.Empty,
                        Type = viewModel.OrganisationType,
                        PublicSectorDataSource = viewModel.PublicSectorDataSource,
                        Status = viewModel.OrganisationStatus,
                        Sector = viewModel.Sector
                    }
                };
            }
            catch (InvalidRequestException e)
            {
                return new OrchestratorResponse<OrganisationDetailsViewModel>
                {
                    Data = new OrganisationDetailsViewModel
                    {
                        ErrorDictionary = e.ErrorMessages
                    },
                    Status = HttpStatusCode.BadRequest,
                    Exception = e,
                    FlashMessage = new FlashMessageViewModel
                    {
                        Headline = "Errors to fix",
                        Message = "Check the following details:",
                        ErrorMessages = e.ErrorMessages,
                        Severity = FlashMessageSeverityLevel.Error
                    }
                };
            }
        }

        public OrchestratorResponse<OrganisationDetailsViewModel> StartConfirmOrganisationDetails(AddOrganisationAddressViewModel request)
        {
            return new OrchestratorResponse<OrganisationDetailsViewModel>
            {
                Data = new OrganisationDetailsViewModel
                {
                    HashedId = request.OrganisationHashedId,
                    Name = request.OrganisationName,
                    Address = request.OrganisationAddress,
                    DateOfInception = request.OrganisationDateOfInception,
                    ReferenceNumber = request.OrganisationReferenceNumber ?? string.Empty,
                    Type = request.OrganisationType,
                    PublicSectorDataSource = request.PublicSectorDataSource,
                    Status = request.OrganisationStatus,
                    Sector = request.Sector
                }
            };
        }

        public OrchestratorResponse<OrganisationDetailsViewModel> StartConfirmOrganisationDetails(FindOrganisationAddressViewModel request)
        {
            return new OrchestratorResponse<OrganisationDetailsViewModel>
            {
                Data = new OrganisationDetailsViewModel
                {
                    HashedId = request.OrganisationHashedId,
                    Name = request.OrganisationName,
                    Address = request.OrganisationAddress,
                    DateOfInception = request.OrganisationDateOfInception,
                    ReferenceNumber = request.OrganisationReferenceNumber ?? string.Empty,
                    Type = request.OrganisationType,
                    PublicSectorDataSource = request.PublicSectorDataSource,
                    Status = request.OrganisationStatus,
                    Sector = request.Sector
                }
            };
        }
    }


}