using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Features.GetProductById;

public sealed class GetProductByIdHandler
{
    private readonly InventoryDbContext _db;

    public GetProductByIdHandler(InventoryDbContext db)
    {
        _db = db;
    }

    public async Task<ProductDto?> Handle(GetProductByIdRequest req, CancellationToken ct)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == req.ProductId)
            .Select(p => new ProductDto(
                p.Id,
                p.TenantId,
                p.Name,
                p.Sku,
                p.QuantityOnHand,
                p.UnitPrice))
            .FirstOrDefaultAsync(ct);
    }
}
