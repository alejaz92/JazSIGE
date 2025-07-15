using FiscalDocumentationService.Infrastructure.Data;
using FiscalDocumentationService.Infrastructure.Interfaces;

namespace FiscalDocumentationService.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FiscalDocumentationDbContext _context;

        public IFiscalDocumentRepository FiscalDocumentRepository { get; }

        public UnitOfWork(FiscalDocumentationDbContext context, IFiscalDocumentRepository fiscalDocumentRepository)
        {
            _context = context;
            FiscalDocumentRepository = fiscalDocumentRepository;
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
