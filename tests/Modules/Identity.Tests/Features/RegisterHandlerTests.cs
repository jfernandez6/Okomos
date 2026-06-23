using Identity.Application.Features.Register;
using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Okomos.SharedKernel.Exceptions;

namespace Identity.Tests.Features;

public class RegisterHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantProvider _tenantProvider;
    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _tenantProvider = Substitute.For<ITenantProvider>();
        _handler = new RegisterHandler(_userManager, _tenantProvider);
    }

    [Fact]
    public async Task Handle_Should_Create_User_And_Return_Response()
    {
        var tenantId = Guid.NewGuid();
        _tenantProvider.CurrentTenantId.Returns(tenantId);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        var request = new RegisterRequest("test@test.com", "Password123!", "John", "Doe");

        var result = await _handler.Handle(request);

        result.UserId.Should().NotBeEmpty();
        result.Email.Should().Be("test@test.com");
        await _userManager.Received(1).CreateAsync(Arg.Any<ApplicationUser>(), "Password123!");
        await _userManager.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), "User");
    }

    [Fact]
    public async Task Handle_Should_Throw_ValidationException_When_Create_Fails()
    {
        var tenantId = Guid.NewGuid();
        _tenantProvider.CurrentTenantId.Returns(tenantId);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email already exists." }));

        var request = new RegisterRequest("test@test.com", "Password123!", "John", "Doe");

        var act = () => _handler.Handle(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_Should_Set_TenantId_On_User()
    {
        var tenantId = Guid.NewGuid();
        _tenantProvider.CurrentTenantId.Returns(tenantId);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        var request = new RegisterRequest("test@test.com", "Password123!", "John", "Doe");

        await _handler.Handle(request);

        await _userManager.Received(1).CreateAsync(
            Arg.Is<ApplicationUser>(u => u.TenantId == tenantId),
            Arg.Any<string>());
    }
}
