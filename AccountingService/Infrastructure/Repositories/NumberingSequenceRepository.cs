// Infrastructure/Repositories/NumberingSequenceRepository.cs
using AccountingService.Infrastructure.Data;        // tu DbContext concreto
using AccountingService.Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace AccountingService.Infrastructure.Repositories
{
    public class NumberingSequenceRepository : INumberingSequenceRepository
    {
        private readonly AccountingDbContext _ctx; // usar el DbContext concreto
        public NumberingSequenceRepository(AccountingDbContext ctx) => _ctx = ctx;

        public async Task<int> GetNextAsync(string scope, CancellationToken ct = default)
        {
            await using var tx = await _ctx.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, ct);

            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM ledger.NumberingSequences WITH (UPDLOCK, HOLDLOCK) WHERE Scope = @scope)
                BEGIN
                    INSERT INTO ledger.NumberingSequences (Scope, NextNumber) VALUES (@scope, 1);
                END

                DECLARE @current int;

                UPDATE ledger.NumberingSequences WITH (ROWLOCK, UPDLOCK)
                SET @current = NextNumber,
                    NextNumber = NextNumber + 1
                WHERE Scope = @scope;

                SELECT @current;";

            var conn = _ctx.Database.GetDbConnection();
            await _ctx.Database.OpenConnectionAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.Transaction = _ctx.Database.CurrentTransaction?.GetDbTransaction();
            cmd.CommandText = sql;

            var pScope = cmd.CreateParameter();
            pScope.ParameterName = "@scope";
            pScope.Value = scope;
            cmd.Parameters.Add(pScope);

            var obj = await cmd.ExecuteScalarAsync(ct);
            var issued = Convert.ToInt32(obj);

            await tx.CommitAsync(ct);
            return issued;
        }

    }
}
