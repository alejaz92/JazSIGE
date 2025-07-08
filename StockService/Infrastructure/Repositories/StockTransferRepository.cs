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
                .ThenBy(st => st.Id)
                .ToListAsync();
    }
}
