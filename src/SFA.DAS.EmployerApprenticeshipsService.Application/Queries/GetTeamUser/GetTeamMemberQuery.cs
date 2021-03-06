﻿using MediatR;

namespace SFA.DAS.EAS.Application.Queries.GetTeamUser
{
    public class GetTeamMemberQuery : IAsyncRequest<GetTeamMemberResponse>
    {
        public string HashedAccountId { get; set; }
        public string TeamMemberId { get; set; }
    }
}
