using Inventory.Application.Features.CreateProduct;
using Inventory.Application.Features.GetProductById;
using Inventory.Infrastructure.Persistence;
using Inventory.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Okomos.Infrastructure.Events;
using Okomos.Infrastructure.Persistence;

namespace Inventory.Tests.Integration;

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

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;

        await using var dbContext = new InventoryDbContext(options, tenantProvider);

        var unitOfWork = new UnitOfWork<InventoryDbContext>(
            dbContext,
            new TestEventBus(),
            Substitute.For<ILogger<DomainEventDispatcher>>());

        var createHandler = new CreateProductHandler(dbContext, tenantProvider, unitOfWork);
        var createResult = await createHandler.Handle(
            new CreateProductRequest("SQL Product", "SQL-001", 20, 45m), CancellationToken.None);

        var queryHandler = new GetProductByIdHandler(dbContext);
        var product = await queryHandler.Handle(
            new GetProductByIdRequest { ProductId = createResult.Id }, CancellationToken.None);

        product.Should().NotBeNull();
        product!.Name.Should().Be("SQL Product");
    }
}
