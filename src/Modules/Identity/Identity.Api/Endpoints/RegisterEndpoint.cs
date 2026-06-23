using FastEndpoints;
using Identity.Application.Features.Register;
using Microsoft.AspNetCore.Http;

namespace Identity.Api.Endpoints;

public sealed class RegisterEndpoint : Endpoint<RegisterRequest, RegisterResponse>
{
    private readonly RegisterHandler _handler;

    public RegisterEndpoint(RegisterHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post($"{ModuleRoutes.RoutePrefix}/register");
        AllowAnonymous();
        Tags("Identity");
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var result = await _handler.Handle(req, ct);
        HttpContext.Response.Headers.Location = $"{ModuleRoutes.RoutePrefix}/users/{result.UserId}";
        await SendAsync(result, StatusCodes.Status201Created, ct);
    }
}
