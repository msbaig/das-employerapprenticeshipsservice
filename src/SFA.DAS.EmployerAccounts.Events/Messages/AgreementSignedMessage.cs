﻿namespace SFA.DAS.EmployerAccounts.Events.Messages
{
    public class AgreementSignedMessage
    {
        public long AccountId { get; set; }
        public long LegalEntityId { get; set; }
        public long AgreementId { get; set; }
    }
}
