namespace Sales.Domain;

public sealed record SalesOrder(Guid Id, string Number, decimal Total);
