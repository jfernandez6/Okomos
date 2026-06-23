namespace Okomos.Infrastructure.Outbox;

public sealed class OutboxStoreRegistration(Type dbContextType)
{
    public Type DbContextType { get; } = dbContextType;
}
