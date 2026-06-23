using FastEndpoints;
using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Billing.Features.GetInvoiceById;

public sealed class GetInvoiceByIdRequest
{
    public Guid InvoiceId { get; set; }
}

public sealed class GetInvoiceByIdEndpoint : Endpoint<GetInvoiceByIdRequest, InvoiceDto>
{
    public override void Configure()
    {
        Get("/api/billing/invoices/{InvoiceId}");
        AuthSchemes("Bearer");
        Tags("Billing");
    }

    public override async Task HandleAsync(GetInvoiceByIdRequest req, CancellationToken ct)
    {
        var handler = Resolve<Okomos.SharedKernel.Abstractions.CQRS.IQueryHandler<GetInvoiceByIdQuery, InvoiceDto?>>();
        var invoice = await handler.HandleAsync(new GetInvoiceByIdQuery(req.InvoiceId), ct);

        if (invoice is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(invoice, cancellation: ct);
    }
}
