using FluentAssertions;
using Okomos.Inventory.Features.CreateProduct;
using Okomos.Inventory.Features.GetProductById;
using Okomos.Inventory.Persistence;
using Okomos.Inventory.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Okomos.Inventory.Tests.Integration;

[Collection(nameof(InventorySqlServerCollection))]
public class InventoryIntegrationTests
{
    private readonly InventorySqlServerFixture _fixture;

    public InventoryIntegrationTests(InventorySqlServerFixture fixture) => _fixture = fixture;

    [DockerFact]
    public async Task Should_Create_And_Get_Product_Using_SqlServer()
    {
        var tenantId = Guid.NewGuid();
        var tenantProvider = new TestTenantProvider(tenantId);
        var eventBus = new TestEventBus();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        await using var dbContext = new InventoryDbContext(options, tenantProvider, eventBus);

        var createHandler = new CreateProductCommandHandler(dbContext, tenantProvider);
        var productId = await createHandler.HandleAsync(new CreateProductCommand("SQL Product", "SQL-001", 20, 45m));

        var queryHandler = new GetProductByIdQueryHandler(dbContext);
        var product = await queryHandler.HandleAsync(new GetProductByIdQuery(productId));

        product.Should().NotBeNull();
        product!.Name.Should().Be("SQL Product");
    }
}
