﻿using SFA.DAS.EAS.Domain;
using SFA.DAS.EAS.Domain.Models.ApprenticeshipProvider;

namespace SFA.DAS.EAS.Application.Queries.GetProvider
{
    public class GetProviderQueryResponse
    {
        public ProvidersView ProvidersView { get; set; }
    }
}