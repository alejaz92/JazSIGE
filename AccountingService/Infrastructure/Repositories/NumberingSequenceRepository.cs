using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Common;

namespace AccountingService.Infrastructure.Repositories
{
    public class NumberingSequenceRepository : INumberingSequenceRepository
    {
        private readonly DbContext _ctx;
        public NumberingSequenceRepository(DbContext ctx) => _ctx = ctx;

        public async Task<int> GetNextAsync(string scope, CancellationToken ct = default)
        {
            // Asegura transacción serializable para evitar carreras
            await using var tx = await _ctx.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

            // 1) Intentar actualizar y obtener el nuevo NextNumber de forma atómica
            var sql = @"
                IF NOT EXISTS (SELECT 1 FROM NumberingSequences WITH (UPDLOCK, HOLDLOCK) WHERE Scope = @scope)
                BEGIN
                    INSERT INTO NumberingSequences (Scope, NextNumber) VALUES (@scope, 1);
                END

                DECLARE @current int;

                UPDATE NumberingSequences WITH (ROWLOCK, UPDLOCK)
                SET NextNumber = NextNumber + 1,
                    @current = NextNumber
                WHERE Scope = @scope;

                -- @current ahora quedó en NextNumber actualizado; el siguiente a entregar es (@current - 1)
                SELECT @current - 1;
                ";

            var param = new SqlParameter("@scope", scope);
            var next = await _ctx.Database.ExecuteSqlRawAsync("/* ping */ SELECT 1", ct); // mantiene la conexión caliente (opcional)

            int issued;
            await foreach (var row in _ctx.Set<ScalarInt>().FromSqlRaw(sql, param).AsAsyncEnumerable().WithCancellation(ct))
            {
                issued = row.Value;
                await tx.CommitAsync(ct);
                return issued;
            }

            // Fallback (no debería ocurrir)
            await tx.RollbackAsync(ct);
            throw new InvalidOperationException("Failed to obtain next sequence number.");
        }

        private class ScalarInt
        {
            public int Value { get; set; }
        }
    }
}
