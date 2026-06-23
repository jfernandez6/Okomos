using Billing.Application.Features.GetInvoiceById;
using FastEndpoints;

namespace Billing.Api.Endpoints.GetInvoiceById;

public sealed class GetInvoiceByIdEndpoint : Endpoint<GetInvoiceByIdRequest, InvoiceDto>
{
    private readonly GetInvoiceByIdHandler _handler;

    public GetInvoiceByIdEndpoint(GetInvoiceByIdHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/invoices/{InvoiceId}");
        Group<ModuleRoutes>();
        AuthSchemes("Bearer");
    }

    public override async Task HandleAsync(GetInvoiceByIdRequest req, CancellationToken ct)
    {
        var invoice = await _handler.Handle(req, ct);

        if (invoice is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(invoice, cancellation: ct);
    }
}
