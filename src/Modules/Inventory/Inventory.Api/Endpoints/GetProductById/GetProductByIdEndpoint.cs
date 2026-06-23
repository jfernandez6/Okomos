using FastEndpoints;
using Inventory.Application.Features.GetProductById;

namespace Inventory.Api.Endpoints.GetProductById;

public sealed class GetProductByIdEndpoint : Endpoint<GetProductByIdRequest, ProductDto>
{
    private readonly GetProductByIdHandler _handler;

    public GetProductByIdEndpoint(GetProductByIdHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/products/{ProductId}");
        Group<ModuleRoutes>();
        AuthSchemes("Bearer");
    }

    public override async Task HandleAsync(GetProductByIdRequest req, CancellationToken ct)
    {
        var product = await _handler.Handle(req, ct);

        if (product is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(product, cancellation: ct);
    }
}
