using Sales.Domain;

namespace Sales.Infrastructure;

public interface ISalesRepository
{
    IReadOnlyCollection<SalesOrder> GetAll();
}

internal sealed class InMemorySalesRepository : ISalesRepository
{
    public IReadOnlyCollection<SalesOrder> GetAll() => Application.SalesContracts.Seed;
}
