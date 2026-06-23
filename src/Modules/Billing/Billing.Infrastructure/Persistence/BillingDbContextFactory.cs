using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Okomos.Infrastructure.Persistence;

namespace Billing.Infrastructure.Persistence;

public sealed class BillingDbContextFactory : IDesignTimeDbContextFactory<BillingDbContext>
{
    public BillingDbContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConfiguration.GetConnectionString("BillingConnection");

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new BillingDbContext(options, new DesignTimeTenantProvider());
    }
}
