﻿using MediatR;

namespace SFA.DAS.EAS.Application.Events.ProcessPayment
{
    public class ProcessPaymentEvent : IAsyncNotification
    {
        public long AccountId { get; set; }
    }
}