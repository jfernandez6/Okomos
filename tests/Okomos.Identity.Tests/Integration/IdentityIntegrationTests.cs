using FluentAssertions;
using Okomos.Identity.Features.Login;
using Okomos.Identity.Features.Register;
using Okomos.Identity.Persistence.Entities;
using Okomos.Identity.Services;
using Okomos.Identity.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Okomos.Identity.Tests.Integration;

[Collection(nameof(IdentitySqlServerCollection))]
public class IdentityIntegrationTests
{
    private readonly IdentitySqlServerFixture _fixture;

    public IdentityIntegrationTests(IdentitySqlServerFixture fixture) => _fixture = fixture;

    [DockerFact]
    public async Task Register_And_Login_Should_Work_With_SqlServer()
    {
        var tenantId = Guid.NewGuid();
        await using var scope = await _fixture.CreateScopeAsync(tenantId);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "Okomos-Test-Secret-Key-Min-32-Characters!",
                ["Jwt:Issuer"] = "Okomos.Test",
                ["Jwt:Audience"] = "Okomos.Test",
                ["Jwt:AccessTokenExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "7"
            })
            .Build();

        var jwtService = new JwtTokenService(configuration);
        var tenantProvider = scope.ServiceProvider.GetRequiredService<Okomos.SharedKernel.Abstractions.Multitenancy.ITenantProvider>();

        var registerHandler = new RegisterCommandHandler(userManager, tenantProvider);
        var registerResult = await registerHandler.HandleAsync(
            new RegisterCommand("identity.test@okomos.com", "Password123!", "Test", "User"));

        registerResult.Email.Should().Be("identity.test@okomos.com");

        var loginHandler = new LoginCommandHandler(userManager, signInManager, jwtService);
        var loginResult = await loginHandler.HandleAsync(
            new LoginCommand("identity.test@okomos.com", "Password123!"));

        loginResult.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
        loginResult.UserId.Should().Be(registerResult.UserId);
    }
}
