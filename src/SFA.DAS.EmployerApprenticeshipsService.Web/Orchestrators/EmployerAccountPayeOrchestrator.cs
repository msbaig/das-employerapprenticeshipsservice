﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using NLog;
using SFA.DAS.EmployerApprenticeshipsService.Application.Commands.AddPayeToNewLegalEntity;
using SFA.DAS.EmployerApprenticeshipsService.Application.Commands.AddPayeWithExistingLegalEntity;
using SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetAccountLegalEntities;
using SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetAccountPayeSchemes;
using SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetMember;
using SFA.DAS.EmployerApprenticeshipsService.Domain;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Configuration;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Entities.Account;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Interfaces;
using SFA.DAS.EmployerApprenticeshipsService.Web.Models;

namespace SFA.DAS.EmployerApprenticeshipsService.Web.Orchestrators
{
    public class EmployerAccountPayeOrchestrator : EmployerVerificationOrchestratorBase
    {
        public EmployerAccountPayeOrchestrator(IMediator mediator, ILogger logger, ICookieService cookieService, EmployerApprenticeshipsServiceConfiguration configuration, IEmpRefFileBasedService empRefFileBasedService) : base(mediator, logger, cookieService, configuration, empRefFileBasedService)
        {
        }

        public async Task<OrchestratorResponse<EmployerAccountPayeListViewModel>> Get(long accountId, string externalUserId)
        {
            var response = await Mediator.SendAsync(new GetAccountPayeSchemesRequest
            {
                AccountId = accountId,
                ExternalUserId = externalUserId
            });
            
            return new OrchestratorResponse<EmployerAccountPayeListViewModel>
            {
                Data = new EmployerAccountPayeListViewModel
                {
                    AccountId = accountId,
                    PayeSchemes = response.PayeSchemes
                }
            };
        }

        public async Task<OrchestratorResponse<AddNewPayeScheme>> GetPayeConfirmModel(long accountId, string code, string redirectUrl, NameValueCollection nameValueCollection)
        { 
            var response = await GetGatewayTokenResponse(code, redirectUrl, nameValueCollection);
            if (response.Status != HttpStatusCode.OK)
            {
                return new OrchestratorResponse<AddNewPayeScheme>
                {
                    Data = new AddNewPayeScheme
                    {
                        AccountId = accountId
                    },
                    Status = response.Status,
                    FlashMessage = response.FlashMessage
                };
            }

            var hmrcResponse = await GetHmrcEmployerInformation(response.Data.AccessToken, string.Empty);
            

            return new OrchestratorResponse < AddNewPayeScheme > { 
               Data = new AddNewPayeScheme
            {
                 
                AccountId = accountId,
                PayeScheme = hmrcResponse.Empref,
                AccessToken = !string.IsNullOrEmpty(hmrcResponse.Empref) ? response.Data.AccessToken : "",
                RefreshToken = !string.IsNullOrEmpty(hmrcResponse.Empref) ? response.Data.RefreshToken : ""
                }
            };
            
        }

        public async Task<OrchestratorResponse<BeginNewPayeScheme>> CheckUserIsOwner(long accountId, string email, string redirectUrl)
        {
            HttpStatusCode status = HttpStatusCode.OK;

            
            var response = await Mediator.SendAsync(new GetMemberRequest
            {
                AccountId = accountId,
                Email = email
            });

            if (response != null)
            {
                if (response.TeamMember.Role != Role.Owner)
                {
                    status = HttpStatusCode.Unauthorized;
                }
            }

            return new OrchestratorResponse<BeginNewPayeScheme>
            {
                Data = new BeginNewPayeScheme { AccountId = accountId } ,
                Status = status,
                FlashMessage = new FlashMessageViewModel
                {
                    RedirectButtonMessage = "Return to PAYE schemes"
                },
                RedirectUrl = redirectUrl
            };
        }

        public async Task<List<LegalEntity>> GetLegalEntities(long accountId, string userId)
        {
            var response = await Mediator.SendAsync(new GetAccountLegalEntitiesRequest
            {
                Id = accountId,
                UserId = userId
            });

            return response.Entites.LegalEntityList;
        }


        public async Task AddPayeSchemeToAccount(ConfirmNewPayeScheme model, string userId)
        {
            if (model.LegalEntityId == 0)
            {
                await Mediator.SendAsync(new AddPayeToNewLegalEntityCommand
                {
                    AccountId = model.AccountId,
                    AccessToken = model.AccessToken,
                    RefreshToken = model.RefreshToken,
                    LegalEntityCode = model.LegalEntityCode,
                    Empref = model.PayeScheme,
                    ExternalUserId = userId,
                    LegalEntityDate = model.LegalEntityDateOfIncorporation,
                    LegalEntityAddress = model.LegalEntityRegisteredAddress,
                    LegalEntityName = model.LegalEntityName
                });
            }
            else
            {
                await Mediator.SendAsync(new AddPayeToAccountForExistingLegalEntityCommand
                {
                    AccountId = model.AccountId,
                    ExternalUserId = userId,
                    EmpRef = model.PayeScheme,
                    LegalEntityId = model.LegalEntityId,
                    RefreshToken = model.RefreshToken,
                    AccessToken = model.AccessToken
                });
            }
            
        }
    }
}