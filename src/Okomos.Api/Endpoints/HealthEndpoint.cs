using FastEndpoints;

namespace Okomos.Api.Endpoints;

public sealed class HealthEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/health");
        AllowAnonymous();
        Tags("Health");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(new { Status = "Healthy", Timestamp = DateTime.UtcNow }, cancellation: ct);
    }
}
