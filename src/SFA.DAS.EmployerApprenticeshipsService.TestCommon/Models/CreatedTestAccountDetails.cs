using System;
using SFA.DAS.EAS.Domain.Models.Organisation;

namespace SFA.DAS.EAS.TestCommon.Models
{
    public class CreatedTestAccountDetails
    {
        public long AccountId { get; set; }
        public string HashedAccountId { get; set; }
        public string UserId { get; set; }
        public OrganisationType OrganisationType { get; set; }
        public string OrganisationName { get; set; }
        public string OrganisationReferenceNumber { get; set; }
        public string OrganisationAddress { get; set; }
        public DateTime? OrganisationDateOfInception { get; set; }
        public string OrganisationStatus { get; set; }
       
    }
}
