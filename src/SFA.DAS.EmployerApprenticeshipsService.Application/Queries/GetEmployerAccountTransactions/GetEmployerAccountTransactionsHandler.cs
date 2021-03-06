﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EAS.Application.Validation;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Levy;
using SFA.DAS.EAS.Domain.Models.Payments;
using SFA.DAS.EAS.Domain.Models.Transaction;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EAS.Application.Queries.GetEmployerAccountTransactions
{
    public class GetEmployerAccountTransactionsHandler :
        IAsyncRequestHandler<GetEmployerAccountTransactionsQuery, GetEmployerAccountTransactionsResponse>
    {
        private readonly IDasLevyService _dasLevyService;
        private readonly IValidator<GetEmployerAccountTransactionsQuery> _validator;
        private readonly IApprenticeshipInfoServiceWrapper _apprenticeshipInfoServiceWrapper;
        private readonly IHashingService _hashingService;
        private readonly ILog _logger;

        public GetEmployerAccountTransactionsHandler(IDasLevyService dasLevyService, IValidator<GetEmployerAccountTransactionsQuery> validator, IApprenticeshipInfoServiceWrapper apprenticeshipInfoServiceWrapper, ILog logger, IHashingService hashingService)
        {
            _dasLevyService = dasLevyService;
            _validator = validator;
            _apprenticeshipInfoServiceWrapper = apprenticeshipInfoServiceWrapper;
            _logger = logger;
            _hashingService = hashingService;
        }

        public async Task<GetEmployerAccountTransactionsResponse> Handle(GetEmployerAccountTransactionsQuery message)
        {
            var result = await _validator.ValidateAsync(message);

            if (!result.IsValid())
            {
                throw new InvalidRequestException(result.ValidationDictionary);
            }

            if (result.IsUnauthorized)
            {
                throw new UnauthorizedAccessException();
            }

            var toDate = CalculateToDate(message);
            var fromDate = new DateTime(toDate.Year, toDate.Month, 1);
            
            var accountId = _hashingService.DecodeValue(message.HashedAccountId);
            var transactions = await _dasLevyService.GetAccountTransactionsByDateRange(accountId, fromDate, toDate);

            var hasPreviousTransactions = await _dasLevyService.GetPreviousAccountTransaction(accountId, fromDate) > 0;
            
            if (!transactions.Any())
            {
                return GetResponse(message.HashedAccountId, accountId, hasPreviousTransactions, toDate.Year, toDate.Month);
            }
            
            foreach (var transaction in transactions)
            {
                GenerateTransactionDescription(transaction);
            }
            
            return GetResponse(message.HashedAccountId, accountId, transactions, hasPreviousTransactions, toDate.Year, toDate.Month);
        }

        private static DateTime CalculateToDate(GetEmployerAccountTransactionsQuery message)
        {
            var year = message.Year == default(int) ? DateTime.Now.Year : message.Year;
            var month = message.Month == default(int) ? DateTime.Now.Month : message.Month;

            var daysInMonth = DateTime.DaysInMonth(year, month);

            var toDate = new DateTime(year, month, daysInMonth);
            return toDate;
        }

        private void GenerateTransactionDescription(TransactionLine transaction)
        {
            if (transaction.GetType() == typeof(LevyDeclarationTransactionLine))
            {
                transaction.Description = transaction.Amount >= 0 ? "Levy" : "Levy adjustment";
            }
            else if (transaction.GetType() == typeof(PaymentTransactionLine))
            {
                var paymentTransaction = (PaymentTransactionLine)transaction;

                transaction.Description = GetPaymentTransactionDescription(paymentTransaction);
            }
        }

        private string GetPaymentTransactionDescription(PaymentTransactionLine transaction)
        {
            var transactionPrefix = transaction.IsCoInvested ? "Co-investment - " : string.Empty;

            try
            {
                var ukprn = Convert.ToInt32(transaction.UkPrn);
                var providerName = _apprenticeshipInfoServiceWrapper.GetProvider(ukprn);

                return $"{transactionPrefix}{providerName.Provider.ProviderName}";
            }
            catch (Exception ex)
            {
                _logger.Info($"Provider not found for UkPrn:{transaction.UkPrn} - {ex.Message}");
            }

            return $"{transactionPrefix}Training provider - name not recognised";
        }

        private static GetEmployerAccountTransactionsResponse GetResponse(string hashedAccountId, long accountId, bool hasPreviousTransactions, int year, int month)
        {
            return GetResponse(hashedAccountId, accountId, new List<TransactionLine>(), hasPreviousTransactions, year, month);
        }

        private static GetEmployerAccountTransactionsResponse GetResponse(
            string hashedAccountId, long accountId, ICollection<TransactionLine> transactions, bool hasPreviousTransactions, int year, int month)
        {
            return new GetEmployerAccountTransactionsResponse
            {
                Data = new AggregationData
                {
                    HashedAccountId = hashedAccountId,
                    AccountId = accountId,
                    TransactionLines = transactions
                },
                AccountHasPreviousTransactions = hasPreviousTransactions,
                Year = year,
                Month = month
            };
        }
    }
}