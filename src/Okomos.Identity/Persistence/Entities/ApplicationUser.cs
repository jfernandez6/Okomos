using Microsoft.AspNetCore.Identity;

namespace Okomos.Identity.Persistence.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public Guid TenantId { get; set; }
}
