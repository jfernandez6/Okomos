using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Okomos.SharedKernel.Abstractions.Multitenancy;
using Okomos.SharedKernel.Exceptions;

namespace Identity.Application.Features.Register;

public sealed class RegisterHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantProvider _tenantProvider;

    public RegisterHandler(
        UserManager<ApplicationUser> userManager,
        ITenantProvider tenantProvider)
    {
        _userManager = userManager;
        _tenantProvider = tenantProvider;
    }

    public async Task<RegisterResponse> Handle(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            TenantId = _tenantProvider.CurrentTenantId ?? Guid.Empty,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            throw new ValidationException(errors);
        }

        await _userManager.AddToRoleAsync(user, "User");

        return new RegisterResponse(user.Id, user.Email!);
    }
}
