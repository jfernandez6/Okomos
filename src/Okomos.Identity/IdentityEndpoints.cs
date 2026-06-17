using Okomos.Identity.Features.Login;
using Okomos.Identity.Features.Refresh;
using Okomos.Identity.Features.Register;
using Okomos.SharedKernel.Abstractions.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Okomos.Identity;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identity")
            .WithTags("Identity");

        group.MapPost("/register", async (
            RegisterCommand command,
            ICommandHandler<RegisterCommand, RegisterResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return Results.Created($"/api/identity/users/{result.UserId}", result);
        })
        .WithName("Register")
        .AllowAnonymous();

        group.MapPost("/login", async (
            LoginCommand command,
            ICommandHandler<LoginCommand, LoginResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return Results.Ok(result);
        })
        .WithName("Login")
        .AllowAnonymous();

        group.MapPost("/refresh", async (
            RefreshTokenCommand command,
            ICommandHandler<RefreshTokenCommand, RefreshTokenResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(command, ct);
            return Results.Ok(result);
        })
        .WithName("RefreshToken")
        .AllowAnonymous();

        return app;
    }
}
