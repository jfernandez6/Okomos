using FastEndpoints;
using Inventory.Application.Features.CreateProduct;
using Microsoft.AspNetCore.Http;

namespace Inventory.Api.Endpoints.CreateProduct;

public sealed class CreateProductEndpoint : Endpoint<CreateProductRequest, CreateProductResponse>
{
    private readonly CreateProductHandler _handler;

    public CreateProductEndpoint(CreateProductHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/products");
        Group<ModuleRoutes>();
        AuthSchemes("Bearer");
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var result = await _handler.Handle(req, ct);
        HttpContext.Response.Headers.Location = $"/inventory/products/{result.Id}";
        await SendAsync(result, StatusCodes.Status201Created, ct);
    }
}
