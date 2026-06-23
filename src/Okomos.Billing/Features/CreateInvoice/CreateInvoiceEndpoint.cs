using FastEndpoints;
using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Billing.Features.CreateInvoice;

public sealed class CreateInvoiceEndpoint : Endpoint<CreateInvoiceCommand, CreateInvoiceResponse>
{
    public override void Configure()
    {
        Post("/api/billing/invoices");
        AuthSchemes("Bearer");
        Tags("Billing");
    }

    public override async Task HandleAsync(CreateInvoiceCommand req, CancellationToken ct)
    {
        var handler = Resolve<Okomos.SharedKernel.Abstractions.CQRS.ICommandHandler<CreateInvoiceCommand, Guid>>();
        var id = await handler.HandleAsync(req, ct);
        HttpContext.Response.Headers.Location = $"/api/billing/invoices/{id}";
        await SendAsync(new CreateInvoiceResponse(id), StatusCodes.Status201Created, ct);
    }
}

public sealed record CreateInvoiceResponse(Guid Id);
