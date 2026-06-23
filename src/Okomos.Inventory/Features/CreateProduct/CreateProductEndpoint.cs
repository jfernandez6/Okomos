using FastEndpoints;
using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Inventory.Features.CreateProduct;

public sealed class CreateProductEndpoint : Endpoint<CreateProductCommand, CreateProductResponse>
{
    public override void Configure()
    {
        Post("/api/inventory/products");
        AuthSchemes("Bearer");
        Tags("Inventory");
    }

    public override async Task HandleAsync(CreateProductCommand req, CancellationToken ct)
    {
        var handler = Resolve<Okomos.SharedKernel.Abstractions.CQRS.ICommandHandler<CreateProductCommand, Guid>>();
        var id = await handler.HandleAsync(req, ct);
        HttpContext.Response.Headers.Location = $"/api/inventory/products/{id}";
        await SendAsync(new CreateProductResponse(id), StatusCodes.Status201Created, ct);
    }
}

public sealed record CreateProductResponse(Guid Id);
