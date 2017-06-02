﻿CREATE PROCEDURE [employer_account].GetUserLegalEntitySettings
(
	@UserRef UNIQUEIDENTIFIER,
	@AccountId BIGINT
)
AS
BEGIN

	select
	a.Id 'EmployerAgreementId',
	s.UserId,
	l.Name 'LegalEntityName',
	s.ReceiveNotifications
	from [employer_account].[UserLegalEntitySettings] s
	join [employer_account].[EmployerAgreement] a on a.Id = s.EmployerAgreementId
	join [employer_account].[LegalEntity] l on l.Id = a.LegalEntityId
	join [employer_account].[Membership] m on m.AccountId = a.AccountId and  m.UserId = s.UserId
	join [employer_account].[User] u on u.Id = s.UserId
	where
	u.UserRef = @UserRef
	and a.AccountId = @AccountId
	and a.StatusId <> 5

END
