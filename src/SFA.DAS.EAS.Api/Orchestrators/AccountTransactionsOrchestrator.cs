﻿using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EAS.Application.Queries.AccountTransactions.GetAccountTransactionSummary;
using SFA.DAS.EAS.Application.Queries.GetEmployerAccountTransactions;
using SFA.DAS.EAS.Domain.Models.Transaction;
using SFA.DAS.NLog.Logger;
using TransactionItemType = SFA.DAS.EAS.Account.Api.Types.TransactionItemType;

namespace SFA.DAS.EAS.Api.Orchestrators
{
    public class AccountTransactionsOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ILog _logger;

        public AccountTransactionsOrchestrator(IMediator mediator, ILog logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<OrchestratorResponse<TransactionsViewModel>> GetAccountTransactions(string hashedAccountId, int year, int month)
        {
            var data =
                await
                    _mediator.SendAsync(new GetEmployerAccountTransactionsQuery
                    {
                        Year = year,
                        Month = month,
                        HashedAccountId = hashedAccountId
                    });

            var response = new OrchestratorResponse<TransactionsViewModel>
            {
                Data = new TransactionsViewModel
                {
                    HasPreviousTransactions = data.AccountHasPreviousTransactions,
                    Year = data.Year,
                    Month = data.Month
                }
            };

            response.Data.AddRange(data.Data.TransactionLines.Select(ConvertToTransactionViewModel));
            return response;
        }

        public async Task<OrchestratorResponse<AccountResourceList<TransactionSummaryViewModel>>> GetAccountTransactionSummary(string hashedAccountId)
        {
            var data = await _mediator.SendAsync(new GetAccountTransactionSummaryRequest { HashedAccountId = hashedAccountId });

            var response = new OrchestratorResponse<AccountResourceList<TransactionSummaryViewModel>>
            {
                Data = new AccountResourceList<TransactionSummaryViewModel>(data.Data.Select(ConvertToTransactionSummaryViewModel))
            };
            
            return response;
        }

        private TransactionSummaryViewModel ConvertToTransactionSummaryViewModel(TransactionSummary transactionSummary)
        {
            return new TransactionSummaryViewModel
            {
                Amount = transactionSummary.Amount,
                Year = transactionSummary.Year,
                Month = transactionSummary.Month
            };
        }

        private TransactionViewModel ConvertToTransactionViewModel(TransactionLine transactionLine)
        {
            return new TransactionViewModel
            {
                Amount = transactionLine.Amount,
                Balance = transactionLine.Balance,
                Description = transactionLine.Description,
                PayrollYear = transactionLine.PayrollYear,
                PayrollMonth = transactionLine.PayrollMonth,
                PayrollDate = transactionLine.PayrollDate,
                TransactionType = (TransactionItemType)transactionLine.TransactionType,
                DateCreated = transactionLine.DateCreated,
                SubTransactions = transactionLine.SubTransactions?.Select(ConvertToTransactionViewModel).ToList(),
                TransactionDate = transactionLine.TransactionDate
            };
        }
    }
}