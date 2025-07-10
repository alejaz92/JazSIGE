using Microsoft.EntityFrameworkCore;
using StockService.Infrastructure.Data;
using StockService.Infrastructure.Interfaces;
using StockService.Infrastructure.Models;

namespace StockService.Infrastructure.Repositories
{
    public class StockTransferRepository : IStockTransferRepository
    {
        private readonly StockDbContext _context;
        public StockTransferRepository(StockDbContext context)
        {
            _context = context;
        }        
        public async Task AddAsync(StockTransfer stockTransfer)
        {
            if (stockTransfer == null)
            {
                throw new ArgumentNullException(nameof(stockTransfer), "Stock transfer cannot be null");
            }
            await _context.StockTransfers.AddAsync(stockTransfer);
            await _context.SaveChangesAsync();
        }
        public async Task<StockTransfer?> GetByIdAsync(int id) => await _context.StockTransfers
                .Include(st => st.Articles)
                .FirstOrDefaultAsync(st => st.Id == id);
        public async Task<IEnumerable<StockTransfer>> GetAllAsync() => await _context.StockTransfers
                .Include(st => st.Articles)
                .OrderByDescending(st => st.Date)
                .ThenByDescending(st => st.Id)
                .ToListAsync();

        public async Task<string> GenerateNextCodeAsync()
        {
            var lastCode = await _context.StockTransfers
                .OrderByDescending(t => t.Id)
                .Select(t => t.Code)
                .FirstOrDefaultAsync();

            if (int.TryParse(lastCode, out int lastNumber))
            {
                return (lastNumber + 1).ToString("D8");
            }

            return "00000001";
        }
    }
}
