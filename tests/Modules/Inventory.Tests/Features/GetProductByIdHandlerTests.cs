using Inventory.Application.Features.GetProductById;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Tests.Features;

public class GetProductByIdHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Product_When_Exists()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new InventoryDbContext(options, tenantProvider);
        var product = Product.Create(tenantId, "Item", "SKU-99", 5, 15m);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var handler = new GetProductByIdHandler(dbContext);
        var result = await handler.Handle(
            new GetProductByIdRequest { ProductId = product.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Sku.Should().Be("SKU-99");
    }
}
