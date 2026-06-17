using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Inventory.Features.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Sku,
    int QuantityOnHand,
    decimal UnitPrice) : ICommand<Guid>;
