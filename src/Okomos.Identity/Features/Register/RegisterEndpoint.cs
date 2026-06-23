using FastEndpoints;
using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Identity.Features.Register;

public sealed class RegisterEndpoint : Endpoint<RegisterCommand, RegisterResponse>
{
    public override void Configure()
    {
        Post("/api/identity/register");
        AllowAnonymous();
        Tags("Identity");
    }

    public override async Task HandleAsync(RegisterCommand req, CancellationToken ct)
    {
        var handler = Resolve<Okomos.SharedKernel.Abstractions.CQRS.ICommandHandler<RegisterCommand, RegisterResponse>>();
        var result = await handler.HandleAsync(req, ct);
        HttpContext.Response.Headers.Location = $"/api/identity/users/{result.UserId}";
        await SendAsync(result, StatusCodes.Status201Created, ct);
    }
}
