using Okomos.Inventory.Persistence;
using Okomos.SharedKernel.Abstractions.CQRS;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Inventory.Features.GetProductById;

public sealed class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly InventoryDbContext _dbContext;

    public GetProductByIdQueryHandler(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductDto?> HandleAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == query.ProductId)
            .Select(p => new ProductDto(
                p.Id,
                p.TenantId,
                p.Name,
                p.Sku,
                p.QuantityOnHand,
                p.UnitPrice))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
