using Catalog.Domain;

namespace Catalog.Infrastructure;

public interface ICatalogRepository
{
    IReadOnlyCollection<CatalogItem> GetAll();
}

internal sealed class InMemoryCatalogRepository : ICatalogRepository
{
    public IReadOnlyCollection<CatalogItem> GetAll() => Application.CatalogContracts.Seed;
}
