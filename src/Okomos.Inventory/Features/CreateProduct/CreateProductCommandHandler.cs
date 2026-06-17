using Okomos.Inventory.Persistence;
using Okomos.Inventory.Persistence.Entities;
using Okomos.SharedKernel.Abstractions.CQRS;
using Okomos.SharedKernel.Abstractions.Multitenancy;

namespace Okomos.Inventory.Features.CreateProduct;

public sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly InventoryDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public CreateProductCommandHandler(InventoryDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.CurrentTenantId
            ?? throw new InvalidOperationException("Tenant is required.");

        var product = Product.Create(tenantId, command.Name, command.Sku, command.QuantityOnHand, command.UnitPrice);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
