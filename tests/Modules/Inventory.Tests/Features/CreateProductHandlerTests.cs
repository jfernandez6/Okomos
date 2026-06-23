using Inventory.Application.Features.CreateProduct;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okomos.Infrastructure.Events;
using Okomos.Infrastructure.Persistence;

namespace Inventory.Tests.Features;

public class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Product()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseTransactionalInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new InventoryDbContext(options, tenantProvider);
        var unitOfWork = new UnitOfWork<InventoryDbContext>(
            dbContext,
            new TestEventBus(),
            Substitute.For<ILogger<DomainEventDispatcher>>());

        var handler = new CreateProductHandler(dbContext, tenantProvider, unitOfWork);

        var result = await handler.Handle(
            new CreateProductRequest("Widget", "W-001", 10, 99.99m), CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        var product = dbContext.Products.Local.Single();
        product.Name.Should().Be("Widget");
        product.Sku.Should().Be("W-001");
        product.TenantId.Should().Be(tenantId);
    }
}
