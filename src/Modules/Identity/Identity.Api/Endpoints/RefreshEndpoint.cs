using FastEndpoints;
using Identity.Application.Features.Refresh;

namespace Identity.Api.Endpoints;

public sealed class RefreshEndpoint : Endpoint<RefreshRequest, RefreshResponse>
{
    private readonly RefreshHandler _handler;

    public RefreshEndpoint(RefreshHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post($"{ModuleRoutes.RoutePrefix}/refresh");
        AllowAnonymous();
        Tags("Identity");
    }

    public override async Task HandleAsync(RefreshRequest req, CancellationToken ct)
    {
        var result = await _handler.Handle(req, ct);
        await SendAsync(result, cancellation: ct);
    }
}
