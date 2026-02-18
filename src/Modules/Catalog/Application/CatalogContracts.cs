using Catalog.Domain;

namespace Catalog.Application;

public static class CatalogContracts
{
    public static IReadOnlyCollection<CatalogItem> Seed =>
    [
        new(Guid.Parse("90f7e4b9-adf6-4c17-a563-f30917b43f90"), "Laptop", "LAP-001"),
        new(Guid.Parse("ca2734f9-8dc8-44ac-b539-4a26dc95c508"), "Mouse", "MSE-001")
    ];
}
