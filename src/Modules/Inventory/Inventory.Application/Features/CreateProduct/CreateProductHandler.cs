using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Okomos.Infrastructure.Persistence;
using Okomos.SharedKernel.Abstractions.Multitenancy;

namespace Inventory.Application.Features.CreateProduct;

public sealed class CreateProductHandler
{
    private readonly InventoryDbContext _db;
    private readonly ITenantProvider _tenant;
    private readonly IUnitOfWork<InventoryDbContext> _unitOfWork;

    public CreateProductHandler(
        InventoryDbContext db,
        ITenantProvider tenant,
        IUnitOfWork<InventoryDbContext> unitOfWork)
    {
        _db = db;
        _tenant = tenant;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateProductResponse> Handle(CreateProductRequest req, CancellationToken ct)
    {
        var tenantId = _tenant.CurrentTenantId ?? throw new InvalidOperationException("Tenant is required.");
        var product = Product.Create(tenantId, req.Name, req.Sku, req.QuantityOnHand, req.UnitPrice);
        _db.Products.Add(product);
        await _unitOfWork.SaveChangesAsync(ct);
        return new CreateProductResponse(product.Id);
    }
}
