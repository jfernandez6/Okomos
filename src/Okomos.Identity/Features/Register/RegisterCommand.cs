using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Identity.Features.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : ICommand<RegisterResponse>;

public sealed record RegisterResponse(Guid UserId, string Email);
