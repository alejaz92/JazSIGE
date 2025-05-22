
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SalesService.Infrastructure.Data;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.SalesQuote;

namespace SalesService.Infrastructure.Repositories
{
    public class SalesQuoteRepository : GenericRepository<SalesQuote>, ISalesQuoteRepository
    {
        private readonly SalesDbContext _context;

        public SalesQuoteRepository(SalesDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SalesQuote?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.SalesQuotes
                .Include(sq => sq.Articles)
                .FirstOrDefaultAsync(sq => sq.Id == id);
        }
    }
}
