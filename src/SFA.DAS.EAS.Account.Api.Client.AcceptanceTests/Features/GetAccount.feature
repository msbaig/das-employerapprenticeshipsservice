Feature: GetAccount

Scenario: Get account using hashed ID
Given I am a user
And I have an account named Test Company
When I lookup Test Company account details using the accounts hashed ID
Then the Test Company account details should be the same as the account I created


Scenario: Get account using ID
Given I am a user
And I have an account named Test Company
When I lookup Test Company account details using the accounts ID
Then the Test Company account details should be the same as the account I created