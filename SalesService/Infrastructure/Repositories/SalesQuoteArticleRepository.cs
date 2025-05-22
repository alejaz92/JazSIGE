using CatalogService.Infrastructure.Repositories;
using SalesService.Infrastructure.Data;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.SalesQuote;

public class SalesQuoteArticleRepository : GenericRepository<SalesQuote_Article>, ISalesQuoteArticleRepository
{
    public SalesQuoteArticleRepository(SalesDbContext context) : base(context) { }
}
