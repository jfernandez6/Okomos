using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Okomos.Identity.Features.Register;
using Okomos.Identity.Persistence.Entities;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Okomos.SharedKernel.Exceptions;

namespace Okomos.Identity.Tests.Features;

public class RegisterCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantProvider _tenantProvider;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _tenantProvider = Substitute.For<ITenantProvider>();
        _handler = new RegisterCommandHandler(_userManager, _tenantProvider);
    }

    [Fact]
    public async Task HandleAsync_Should_Create_User_And_Return_Response()
    {
        var tenantId = Guid.NewGuid();
        _tenantProvider.CurrentTenantId.Returns(tenantId);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        var command = new RegisterCommand("test@test.com", "Password123!", "John", "Doe");

        var result = await _handler.HandleAsync(command);

        result.UserId.Should().NotBeEmpty();
        result.Email.Should().Be("test@test.com");
        await _userManager.Received(1).CreateAsync(Arg.Any<ApplicationUser>(), "Password123!");
        await _userManager.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), "User");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_ValidationException_When_Create_Fails()
    {
        var tenantId = Guid.NewGuid();
        _tenantProvider.CurrentTenantId.Returns(tenantId);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email already exists." }));

        var command = new RegisterCommand("test@test.com", "Password123!", "John", "Doe");

        var act = () => _handler.HandleAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task HandleAsync_Should_Set_TenantId_On_User()
    {
        var tenantId = Guid.NewGuid();
        _tenantProvider.CurrentTenantId.Returns(tenantId);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        var command = new RegisterCommand("test@test.com", "Password123!", "John", "Doe");

        await _handler.HandleAsync(command);

        await _userManager.Received(1).CreateAsync(
            Arg.Is<ApplicationUser>(u => u.TenantId == tenantId),
            Arg.Any<string>());
    }
}
