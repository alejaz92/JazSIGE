using AccountingService.Infrastructure.Models;
using System.Threading.Tasks;

namespace JazSIGE.Accounting.Infrastructure.Interfaces
{
    public interface IReceiptRepository : IGenericRepository<Receipt>
    {
        Task<Receipt?> GetFullAsync(int id); // incluye Payments + Allocations
    }
}
