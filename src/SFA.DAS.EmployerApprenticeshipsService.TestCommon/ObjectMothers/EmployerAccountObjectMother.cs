using System;
using SFA.DAS.EAS.Web.ViewModels;

namespace SFA.DAS.EAS.TestCommon.ObjectMothers
{
    public class EmployerAccountObjectMother
    {
        public static CreateAccountViewModel CreateViewModel(string userId, string orgainsationName)
        {
            return new CreateAccountViewModel
            {
                UserId = userId,
                AccessToken = Guid.NewGuid().ToString(),
                RefreshToken = Guid.NewGuid().ToString(),
                OrganisationDateOfInception = new DateTime(2016, 01, 01),
                PayeReference =
                    $"{Guid.NewGuid().ToString().Substring(0, 3)}/{Guid.NewGuid().ToString().Substring(0, 7)}",
                OrganisationName = orgainsationName,
                OrganisationReferenceNumber = "123456TGB" + Guid.NewGuid().ToString().Substring(0, 6),
                OrganisationAddress = "Address Line 1",
                OrganisationStatus = "active"
            };
        }
    }
}
