using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Domain.Models.Organisation;
using SFA.DAS.EAS.Domain.Models.ReferenceData;
using SFA.DAS.EAS.Web.Authentication;
using SFA.DAS.EAS.Web.Orchestrators;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.EAS.Web.ViewModels.Organisation;

namespace SFA.DAS.EAS.Web.Controllers
{
    [Authorize]
    [RoutePrefix("accounts")]
    public class SearchOrganisationController : BaseController
    {
        private readonly SearchOrganisationOrchestrator _orchestrator;
        //This is temporary until the existing add org function is replaced, at which point the method used can be moved to the org search orchestrator
        private readonly IMapper _mapper;


        public SearchOrganisationController(IOwinWrapper owinWrapper,
            SearchOrganisationOrchestrator orchestrator,
            IFeatureToggle featureToggle,
            IMultiVariantTestingService multiVariantTestingService,
            ICookieStorageService<FlashMessageViewModel> flashMessage,
            IMapper mapper)
            : base(owinWrapper, featureToggle, multiVariantTestingService, flashMessage)
        {
            _orchestrator = orchestrator;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("{HashedAccountId}/organisations/search", Order = 0)]
        [Route("organisations/search", Order = 1)]
        public ActionResult SearchForOrganisation(string hashedAccountId)
        {
            TakeActionOnWhetherACurrentUser(hashedAccountId);
            return View("SearchForOrganisation");
        }

        [HttpPost]
        [Route("{HashedAccountId}/organisations/search", Order = 0)]
        [Route("organisations/search", Order = 1)]
        public ActionResult SearchForOrganisation(string hashedAccountId, string searchTerm)
        {
            TakeActionOnWhetherACurrentUser(hashedAccountId);
            if (string.IsNullOrEmpty(searchTerm))
            {
                return View("SearchForOrganisation");
            }

            return RedirectToAction("SearchForOrganisationResults", new { hashedAccountId, searchTerm });
        }

        [Route("{HashedAccountId}/organisations/search/results", Order = 0)]
        [Route("organisations/search/results", Order = 1)]
        public async Task<ActionResult> SearchForOrganisationResults(string hashedAccountId, string searchTerm, int pageNumber = 1, OrganisationType? organisationType = null)
        {
            TakeActionOnWhetherACurrentUser(hashedAccountId);
            OrchestratorResponse<SearchOrganisationResultsViewModel> model;
            if (string.IsNullOrEmpty(searchTerm))
            {
                var viewModel = new SearchOrganisationResultsViewModel { Results = new PagedResponse<OrganisationDetailsViewModel>() };
                model = CreateSearchTermValidationErrorModel(viewModel);
            }
            else
            {
                model = await _orchestrator.SearchOrganisation(searchTerm, pageNumber, organisationType, hashedAccountId, OwinWrapper.GetClaimValue(@"sub"));
            }

            return View("SearchForOrganisationResults", model);
        }

        [HttpPost]
        [Route("{HashedAccountId}/organisations/search/confirm", Order = 0)]
        [Route("organisations/search/confirm", Order = 1)]
        public ActionResult Confirm(string hashedAccountId, OrganisationDetailsViewModel viewModel)
        {
            viewModel.NewSearch = true;

            TakeActionOnWhetherACurrentUser(hashedAccountId);
            if (string.IsNullOrWhiteSpace(viewModel.Address))
            {
                return FindAddress(viewModel);
            }
            CreateOrganisationCookieData(viewModel);

            if (string.IsNullOrEmpty(hashedAccountId))
            {
                return RedirectToAction("GatewayInform", "EmployerAccount");
            }

            
            var response = new OrchestratorResponse<OrganisationDetailsViewModel> { Data = viewModel };
            
            return View("ConfirmOrganisationDetails", response);
        }

        [HttpGet]
        [Route("{HashedAccountId}/organisations/search/manualAdd", Order = 0)]
        [Route("organisations/search/manualAdd", Order = 1)]
        public ActionResult AddOtherOrganisationDetails(string hashedAccountId)
        {
            TakeActionOnWhetherACurrentUser(hashedAccountId);

            return RedirectToAction("AddOtherOrganisationDetails");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("custom/add")]
        public async Task<ActionResult> AddOtherOrganisationDetails(OrganisationDetailsViewModel model)
        {
            var response = await _orchestrator.ValidateLegalEntityName(model);

            if (response.Status == HttpStatusCode.BadRequest)
            {
                return View("AddOtherOrganisationDetails", response);
            }

            model.Type = OrganisationType.Other;

            return FindAddress(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{HashedAccountId}/address/select", Order = 0)]
        [Route("address/select", Order = 1)]       
        public async Task<ActionResult> SelectAddress(FindOrganisationAddressViewModel request)
        {
            var response = await _orchestrator.GetAddressesFromPostcode(request);

            if (response?.Data?.Addresses != null && response.Data.Addresses.Count == 1)
            {
                var viewModel = _mapper.Map<AddOrganisationAddressViewModel>(request);

                viewModel.Address = response.Data.Addresses.Single();

                var addressResponse = new OrchestratorResponse<AddOrganisationAddressViewModel>
                {
                    Data = viewModel,
                    Status = HttpStatusCode.OK
                };

                return View("AddOrganisationAddress", addressResponse);
            }

            return View(response);
        }

        private ActionResult FindAddress(OrganisationDetailsViewModel organisation)
        {
            var addressViewModel = _mapper.Map<FindOrganisationAddressViewModel>(organisation);
            var response = new OrchestratorResponse<FindOrganisationAddressViewModel> { Data = addressViewModel };

            return View("FindAddress", response);
        }

        private void TakeActionOnWhetherACurrentUser(string hashedAccountId)
        {
            if (string.IsNullOrEmpty(hashedAccountId))
            {
                ViewBag.HideNav = "true";
            }
            else
            {
                ViewBag.HideNav = "false";
                ViewBag.Section = "organisations";
            }
        }

        private OrchestratorResponse<T> CreateSearchTermValidationErrorModel<T>(T data)
        {
            var model = new OrchestratorResponse<T> { Data = data };
            SetSearchTermValidationModelProperties(model);
            return model;
        }

        private static void SetSearchTermValidationModelProperties(OrchestratorResponse model)
        {
            model.Status = HttpStatusCode.BadRequest;
            model.FlashMessage = FlashMessageViewModel.CreateErrorFlashMessageViewModel(new Dictionary<string, string> { { "searchTerm", "Enter organisation name" } });
        }

        private void CreateOrganisationCookieData(OrganisationDetailsViewModel viewModel)
        {
            EmployerAccountData data;
            if (viewModel?.Name != null)
            {
                data = new EmployerAccountData
                {
                    OrganisationType = viewModel.Type,
                    OrganisationReferenceNumber = viewModel.ReferenceNumber,
                    OrganisationName = viewModel.Name,
                    OrganisationDateOfInception = viewModel.DateOfInception,
                    OrganisationRegisteredAddress = viewModel.Address,
                    OrganisationStatus = viewModel.Status ?? string.Empty,
                    PublicSectorDataSource = viewModel.PublicSectorDataSource,
                    Sector = viewModel.Sector,
                    NewSearch = viewModel.NewSearch
                };
            }
            else
            {
                var existingData = _orchestrator.GetCookieData(HttpContext);

                data = new EmployerAccountData
                {
                    OrganisationType = existingData.OrganisationType,
                    OrganisationReferenceNumber = existingData.OrganisationReferenceNumber,
                    OrganisationName = existingData.OrganisationName,
                    OrganisationDateOfInception = existingData.OrganisationDateOfInception,
                    OrganisationRegisteredAddress = existingData.OrganisationRegisteredAddress,
                    OrganisationStatus = existingData.OrganisationStatus,
                    PublicSectorDataSource = existingData.PublicSectorDataSource,
                    Sector = existingData.Sector,
                    NewSearch = existingData.NewSearch
                };
            }

            _orchestrator.CreateCookieData(HttpContext, data);
        }

        [HttpGet]
        [Route("{HashedAccountId}/address/update", Order = 0)]
        [Route("address/update", Order =1)]
        public ActionResult AddOrganisationAddress(AddOrganisationAddressViewModel request)
        {

            if (!string.IsNullOrEmpty(request.OrganisationAddress))
            {
                var organisationDetailsViewModel = _orchestrator.StartConfirmOrganisationDetails(request);
                return View("ConfirmOrganisationDetails", organisationDetailsViewModel);
            }

            if (request.Address == null)
            {
                request.Address = new AddressViewModel();
            }

            var response = new OrchestratorResponse<AddOrganisationAddressViewModel>
            {
                Data = request,
                Status = HttpStatusCode.OK
            };

            return View("AddOrganisationAddress", response);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{HashedAccountId}/address/update", Order = 0)]
        [Route("address/update", Order=1)]
        public ActionResult UpdateOrganisationAddress(AddOrganisationAddressViewModel request)
        {
            var response = _orchestrator.AddOrganisationAddress(request);

            if (response.Status == HttpStatusCode.BadRequest)
            {
                request.Address = request.Address ?? new AddressViewModel();
                request.Address.ErrorDictionary = response.Data.ErrorDictionary;

                var errorResponse = new OrchestratorResponse<AddOrganisationAddressViewModel>
                {
                    Data = request,
                    Status = HttpStatusCode.BadRequest,
                    Exception = response.Exception,
                    FlashMessage = response.FlashMessage
                };

                return View("AddOrganisationAddress", errorResponse);
            }
            CreateOrganisationCookieData(response.Data);

            return RedirectToAction("GatewayInform", "EmployerAccount", response.Data);
        }
    }
}