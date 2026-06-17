using Okomos.Identity.Persistence;
using Okomos.Identity.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Okomos.Identity.Services;

public sealed class IdentityDataSeederHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IdentityDataSeederHostedService> _logger;

    public IdentityDataSeederHostedService(
        IServiceProvider serviceProvider,
        ILogger<IdentityDataSeederHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        try
        {
            if (!await context.Database.CanConnectAsync(cancellationToken))
            {
                _logger.LogWarning("Identity database is not available. Skipping seed.");
                return;
            }

            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Id = Guid.NewGuid(),
                    Name = "User",
                    NormalizedName = "USER",
                    TenantId = Guid.Empty
                });
                _logger.LogInformation("Seeded default 'User' role.");
            }

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    TenantId = Guid.Empty
                });
                _logger.LogInformation("Seeded default 'Admin' role.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Identity seed skipped. Run migrations first.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
