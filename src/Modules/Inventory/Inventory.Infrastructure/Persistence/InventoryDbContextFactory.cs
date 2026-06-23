using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Okomos.Infrastructure.Persistence;

namespace Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConfiguration.GetConnectionString("InventoryConnection");

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new InventoryDbContext(options, new DesignTimeTenantProvider());
    }
}
