﻿using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.EAS.Api.Orchestrators;

namespace SFA.DAS.EAS.Api.Controllers
{
    [RoutePrefix("api/user/{userId}")]
    public class EmployerUserController : ApiController
    {
        private readonly UsersOrchestrator _orchestrator;

        public EmployerUserController(UsersOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [Route("accounts", Name = "Accounts")]
        [Authorize(Roles = "ReadUserAccounts")]
        [HttpGet]
        public async Task<IHttpActionResult> GetUserAccounts(string userId)
        {
            var result = await _orchestrator.GetUserAccounts(userId);

            if (result.Status == HttpStatusCode.OK)
            {
                return Ok(result.Data);
            }
            
            //TODO: Handle unhappy paths.
            return Conflict();
        }
    }
}