namespace FiscalDocumentationService.Infrastructure.Interfaces
{
    public interface IUnitOfWork
    {
        IFiscalDocumentRepository FiscalDocumentRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
