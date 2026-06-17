using Okomos.Billing.Features.CreateInvoice;
using Okomos.Billing.Features.GetInvoiceById;
using Okomos.SharedKernel.Abstractions.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Okomos.Billing;

public static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/billing")
            .WithTags("Billing")
            .RequireAuthorization();

        group.MapPost("/invoices", async (
            CreateInvoiceCommand command,
            ICommandHandler<CreateInvoiceCommand, Guid> handler,
            CancellationToken ct) =>
        {
            var id = await handler.HandleAsync(command, ct);
            return Results.Created($"/api/billing/invoices/{id}", new { Id = id });
        })
        .WithName("CreateInvoice");

        group.MapGet("/invoices/{invoiceId:guid}", async (
            Guid invoiceId,
            IQueryHandler<GetInvoiceByIdQuery, InvoiceDto?> handler,
            CancellationToken ct) =>
        {
            var invoice = await handler.HandleAsync(new GetInvoiceByIdQuery(invoiceId), ct);
            return invoice is null ? Results.NotFound() : Results.Ok(invoice);
        })
        .WithName("GetInvoiceById");

        return app;
    }
}
