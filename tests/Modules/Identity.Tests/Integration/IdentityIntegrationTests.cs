using Identity.Application.Features.Login;
using Identity.Application.Features.Register;
using Identity.Domain.Entities;
using Identity.Infrastructure.Services;
using Identity.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Okomos.SharedKernel.Abstractions.Multitenancy;

namespace Identity.Tests.Integration;

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
        var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();

        var registerHandler = new RegisterHandler(userManager, tenantProvider);
        var registerResult = await registerHandler.Handle(
            new RegisterRequest("identity.test@okomos.com", "Password123!", "Test", "User"));

        registerResult.Email.Should().Be("identity.test@okomos.com");

        var loginHandler = new LoginHandler(userManager, signInManager, jwtService);
        var loginResult = await loginHandler.Handle(
            new LoginRequest("identity.test@okomos.com", "Password123!"));

        loginResult.AccessToken.Should().NotBeNullOrWhiteSpace();
        loginResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
        loginResult.UserId.Should().Be(registerResult.UserId);
    }
}
