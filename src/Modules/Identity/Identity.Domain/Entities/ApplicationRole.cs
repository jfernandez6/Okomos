using Microsoft.AspNetCore.Identity;

namespace Identity.Domain.Entities;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public Guid TenantId { get; set; }
}
