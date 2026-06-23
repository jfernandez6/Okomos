using FastEndpoints;
using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Identity.Features.Refresh;

public sealed class RefreshEndpoint : Endpoint<RefreshTokenCommand, RefreshTokenResponse>
{
    public override void Configure()
    {
        Post("/api/identity/refresh");
        AllowAnonymous();
        Tags("Identity");
    }

    public override async Task HandleAsync(RefreshTokenCommand req, CancellationToken ct)
    {
        var handler = Resolve<Okomos.SharedKernel.Abstractions.CQRS.ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>>();
        var result = await handler.HandleAsync(req, ct);
        await SendAsync(result, cancellation: ct);
    }
}
