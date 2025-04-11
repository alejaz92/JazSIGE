using Microsoft.EntityFrameworkCore;
using PurchaseService.Infrastructure.Data;
using PurchaseService.Infrastructure.Interfaces;
using PurchaseService.Infrastructure.Models;

namespace PurchaseService.Infrastructure.Repositories
{
    public class DispatchRepository : IDispatchRepository
    {
        private readonly PurchaseDbContext _context;
        public DispatchRepository(PurchaseDbContext context)
        {
            _context = context;
        }
        public async Task<Dispatch> AddAsync(Dispatch dispatch)
        {
            await _context.Dispatches.AddAsync(dispatch);
            return dispatch;
        }

        public async Task<Dispatch?> GetByIdAsync(int id) => await _context.Dispatches
                .Include(d => d.Purchase)
                    .ThenInclude(p => p.Articles)
                .FirstOrDefaultAsync(d => d.Id == id);

        public async Task<IEnumerable<Dispatch>> GetAllAsync() => await _context.Dispatches
                .Include(d => d.Purchase)
                    .ThenInclude(p => p.Articles)
                .OrderByDescending(d => d.Date)
                .ToListAsync();

        public async Task<IEnumerable<Dispatch>> GetAllAsync(int pageNumber, int pageSize) => await _context.Dispatches
                .Include(d => d.Purchase)
                    .ThenInclude(p => p.Articles)
                .OrderByDescending(d => d.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        public async Task<int> GetTotalCountAsync() => await _context.Dispatches.CountAsync();
    }
}
