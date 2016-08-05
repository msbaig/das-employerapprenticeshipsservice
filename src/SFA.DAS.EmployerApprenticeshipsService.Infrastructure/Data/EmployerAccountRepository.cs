﻿using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NLog;
using SFA.DAS.EmployerApprenticeshipsService.Domain;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Configuration;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Data;

namespace SFA.DAS.EmployerApprenticeshipsService.Infrastructure.Data
{
    public class EmployerAccountRepository : BaseRepository, IEmployerAccountRepository
    {
        
        public EmployerAccountRepository(EmployerApprenticeshipsServiceConfiguration configuration, ILogger logger)
            : base(configuration, logger)
        {
        }

        public async Task<Account> GetAccountById(long id)
        {
            var result = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", id, DbType.Int64);

                return await c.QueryAsync<Account>(
                    sql: "select a.* from [dbo].[Account] a where a.Id = @id;",
                    param: parameters,
                    commandType: CommandType.Text);
            });
            return result.SingleOrDefault();
        }
    }
}

