using FluentAssertions;
using Okomos.Inventory.Features.CreateProduct;
using Okomos.Inventory.Persistence;
using Okomos.Inventory.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Inventory.Tests.Features;

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Create_Product()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new InventoryDbContext(options, tenantProvider);
        var handler = new CreateProductCommandHandler(dbContext, tenantProvider);

        var productId = await handler.HandleAsync(new CreateProductCommand("Widget", "W-001", 10, 99.99m));

        productId.Should().NotBeEmpty();
        var product = dbContext.Products.Local.Single();
        product.Name.Should().Be("Widget");
        product.Sku.Should().Be("W-001");
        product.TenantId.Should().Be(tenantId);
        product.DomainEvents.Should().ContainSingle();
    }
}
