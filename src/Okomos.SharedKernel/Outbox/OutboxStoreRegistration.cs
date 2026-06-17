using Microsoft.EntityFrameworkCore;

namespace Okomos.SharedKernel.Outbox;

public sealed class OutboxStoreRegistration(Type dbContextType)
{
    public Type DbContextType { get; } = dbContextType;
}
