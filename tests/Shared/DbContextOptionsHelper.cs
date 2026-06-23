using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Okomos.Tests.Shared;

public static class DbContextOptionsHelper
{
    public static DbContextOptionsBuilder<TContext> UseTransactionalInMemoryDatabase<TContext>(
        this DbContextOptionsBuilder<TContext> builder,
        string databaseName)
        where TContext : DbContext
    {
        return builder
            .UseInMemoryDatabase(databaseName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    }
}
