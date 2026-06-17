using Okomos.Identity.Persistence.Entities;
using Okomos.SharedKernel.Abstractions.CQRS;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Microsoft.AspNetCore.Identity;

namespace Okomos.Identity.Features.Register;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantProvider _tenantProvider;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITenantProvider tenantProvider)
    {
        _userManager = userManager;
        _tenantProvider = tenantProvider;
    }

    public async Task<RegisterResponse> HandleAsync(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            TenantId = _tenantProvider.CurrentTenantId ?? Guid.Empty,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            throw new SharedKernel.Exceptions.ValidationException(errors);
        }

        await _userManager.AddToRoleAsync(user, "User");

        return new RegisterResponse(user.Id, user.Email!);
    }
}
