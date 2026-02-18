using Sales.Domain;

namespace Sales.Application;

public static class SalesContracts
{
    public static IReadOnlyCollection<SalesOrder> Seed =>
    [
        new(Guid.Parse("41fd5ac9-a896-4a2f-a83f-1891f45d12f0"), "SO-1001", 1200m),
        new(Guid.Parse("f9a50f4d-1c9d-4cf5-9547-05aff94f5fa3"), "SO-1002", 820m)
    ];
}
