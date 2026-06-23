using FastEndpoints;
using Identity.Application.Features.Login;

namespace Identity.Api.Endpoints;

public sealed class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly LoginHandler _handler;

    public LoginEndpoint(LoginHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post($"{ModuleRoutes.RoutePrefix}/login");
        AllowAnonymous();
        Tags("Identity");
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var result = await _handler.Handle(req, ct);
        await SendAsync(result, cancellation: ct);
    }
}
