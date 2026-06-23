using FastEndpoints;
using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Identity.Features.Login;

public sealed class LoginEndpoint : Endpoint<LoginCommand, LoginResponse>
{
    public override void Configure()
    {
        Post("/api/identity/login");
        AllowAnonymous();
        Tags("Identity");
    }

    public override async Task HandleAsync(LoginCommand req, CancellationToken ct)
    {
        var handler = Resolve<Okomos.SharedKernel.Abstractions.CQRS.ICommandHandler<LoginCommand, LoginResponse>>();
        var result = await handler.HandleAsync(req, ct);
        await SendAsync(result, cancellation: ct);
    }
}
