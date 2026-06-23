namespace Inventory.Application.Features.CreateProduct;

public sealed record CreateProductRequest(string Name, string Sku, int QuantityOnHand, decimal UnitPrice);
