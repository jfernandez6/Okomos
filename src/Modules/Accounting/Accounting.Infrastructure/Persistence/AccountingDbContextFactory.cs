using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Okomos.Infrastructure.Persistence;

namespace Accounting.Infrastructure.Persistence;

public sealed class AccountingDbContextFactory : IDesignTimeDbContextFactory<AccountingDbContext>
{
    public AccountingDbContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConfiguration.GetConnectionString("AccountingConnection");

        var options = new DbContextOptionsBuilder<AccountingDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AccountingDbContext(options, new DesignTimeTenantProvider());
    }
}
