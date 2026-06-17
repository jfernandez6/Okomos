using Okomos.Inventory.Features.CreateProduct;
using Okomos.Inventory.Features.GetProductById;
using Okomos.SharedKernel.Abstractions.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Okomos.Inventory;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory")
            .WithTags("Inventory")
            .RequireAuthorization();

        group.MapPost("/products", async (
            CreateProductCommand command,
            ICommandHandler<CreateProductCommand, Guid> handler,
            CancellationToken ct) =>
        {
            var id = await handler.HandleAsync(command, ct);
            return Results.Created($"/api/inventory/products/{id}", new { Id = id });
        })
        .WithName("CreateProduct");

        group.MapGet("/products/{productId:guid}", async (
            Guid productId,
            IQueryHandler<GetProductByIdQuery, ProductDto?> handler,
            CancellationToken ct) =>
        {
            var product = await handler.HandleAsync(new GetProductByIdQuery(productId), ct);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithName("GetProductById");

        return app;
    }
}
