﻿using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.EmployerAccounts.Events.Messages
{
    [QueueName("agreement_signed_notifications")]
    public class AgreementSignedMessage
    {
        public long AccountId { get; set; }
        public long LegalEntityId { get; set; }
        public long AgreementId { get; set; }
    }
}
