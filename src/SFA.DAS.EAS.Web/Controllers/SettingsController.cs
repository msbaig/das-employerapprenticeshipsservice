﻿using System.Threading.Tasks;
using System.Web.Mvc;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.UserProfile;
using SFA.DAS.EAS.Web.Authentication;
using SFA.DAS.EAS.Web.Orchestrators;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.EmployerUsers.WebClientComponents;

namespace SFA.DAS.EAS.Web.Controllers
{
    [RoutePrefix("settings")]
    [AuthoriseActiveUser]
    public class SettingsController : BaseController
    {
        private readonly UserSettingsOrchestrator _userSettingsOrchestrator;

        public SettingsController(IOwinWrapper owinWrapper,
            UserSettingsOrchestrator userSettingsOrchestrator,
            IFeatureToggle featureToggle,
            IMultiVariantTestingService multiVariantTestingService,
            ICookieStorageService<FlashMessageViewModel> flashMessage)
            : base(owinWrapper, featureToggle, multiVariantTestingService, flashMessage)
        {
            _userSettingsOrchestrator = userSettingsOrchestrator;
        }

        [HttpGet]
        [Route("notifications")]
        public async Task<ActionResult> NotificationSettings()
        {
            var userIdClaim = OwinWrapper.GetClaimValue(@"sub");
            var vm = await _userSettingsOrchestrator.GetNotificationSettingsViewModel(userIdClaim);

            var flashMessage = GetFlashMessageViewModelFromCookie();

            vm.FlashMessage = flashMessage;

            return View(vm);
        }

        [HttpPost]
        [Route("notifications")]
        public async Task<ActionResult> NotificationSettings(NotificationSettingsViewModel vm)
        {
            var userIdClaim = OwinWrapper.GetClaimValue(@"sub");

            await _userSettingsOrchestrator.UpdateNotificationSettings(userIdClaim,
                vm.NotificationSettings);

            var flashMessage = new FlashMessageViewModel
            {
                Severity = FlashMessageSeverityLevel.Success,
                Message = "Settings updated."
            };

            AddFlashMessageToCookie(flashMessage);

            return RedirectToAction("NotificationSettings");
        }

        [HttpGet]
        [Route("notifications/unsubscribe/{hashedAccountId}")]
        public async Task<ActionResult> NotificationUnsubscribe(string hashedAccountId)
        {
            var userIdClaim = OwinWrapper.GetClaimValue(@"sub");
            
            var url = Url.Action("NotificationSettings");
            var model = await _userSettingsOrchestrator.Unsubscribe(userIdClaim, hashedAccountId, url);

            return View(model);
        }
    }
}