using Okomos.Identity.Features.Login;
using Okomos.Identity.Features.Refresh;
using Okomos.Identity.Features.Register;
using Okomos.Identity.Persistence;
using Okomos.Identity.Persistence.Entities;
using Okomos.Identity.Services;
using Okomos.SharedKernel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Okomos.Identity;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentityConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Identity connection string is not configured.");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "identity")));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddHostedService<IdentityDataSeederHostedService>();

        services.AddOutboxStore<IdentityDbContext>();

        services.AddCommandHandler<RegisterCommand, RegisterResponse, RegisterCommandHandler, IdentityDbContext>(
            useTransaction: false, useMultitenancy: false, useDomainEvents: false);

        services.AddCommandHandler<LoginCommand, LoginResponse, LoginCommandHandler, IdentityDbContext>(
            useTransaction: false, useMultitenancy: false, useDomainEvents: false);

        services.AddCommandHandler<RefreshTokenCommand, RefreshTokenResponse, RefreshTokenCommandHandler, IdentityDbContext>(
            useTransaction: false, useMultitenancy: false, useDomainEvents: false);

        return services;
    }
}
