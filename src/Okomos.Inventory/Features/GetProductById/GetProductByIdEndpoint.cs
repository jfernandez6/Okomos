using FastEndpoints;
using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Inventory.Features.GetProductById;

public sealed class GetProductByIdRequest
{
    public Guid ProductId { get; set; }
}

public sealed class GetProductByIdEndpoint : Endpoint<GetProductByIdRequest, ProductDto>
{
    public override void Configure()
    {
        Get("/api/inventory/products/{ProductId}");
        AuthSchemes("Bearer");
        Tags("Inventory");
    }

    public override async Task HandleAsync(GetProductByIdRequest req, CancellationToken ct)
    {
        var handler = Resolve<Okomos.SharedKernel.Abstractions.CQRS.IQueryHandler<GetProductByIdQuery, ProductDto?>>();
        var product = await handler.HandleAsync(new GetProductByIdQuery(req.ProductId), ct);

        if (product is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(product, cancellation: ct);
    }
}
