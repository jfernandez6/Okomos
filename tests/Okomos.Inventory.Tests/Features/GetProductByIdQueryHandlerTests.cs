using FluentAssertions;
using Okomos.Inventory.Features.GetProductById;
using Okomos.Inventory.Persistence;
using Okomos.Inventory.Persistence.Entities;
using Okomos.Inventory.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Inventory.Tests.Features;

public class GetProductByIdQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Product_When_Exists()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);
        var eventBus = new TestEventBus();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new InventoryDbContext(options, tenantProvider, eventBus);
        var product = Product.Create(tenantId, "Item", "SKU-99", 5, 15m);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var handler = new GetProductByIdQueryHandler(dbContext);
        var result = await handler.HandleAsync(new GetProductByIdQuery(product.Id));

        result.Should().NotBeNull();
        result!.Sku.Should().Be("SKU-99");
    }
}
