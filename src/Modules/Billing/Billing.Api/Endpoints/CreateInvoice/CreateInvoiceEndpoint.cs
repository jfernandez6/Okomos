using Billing.Application.Features.CreateInvoice;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Billing.Api.Endpoints.CreateInvoice;

public sealed class CreateInvoiceEndpoint : Endpoint<CreateInvoiceRequest, CreateInvoiceResponse>
{
    private readonly CreateInvoiceHandler _handler;

    public CreateInvoiceEndpoint(CreateInvoiceHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/invoices");
        Group<ModuleRoutes>();
        AuthSchemes("Bearer");
    }

    public override async Task HandleAsync(CreateInvoiceRequest req, CancellationToken ct)
    {
        var result = await _handler.Handle(req, ct);
        HttpContext.Response.Headers.Location = $"/billing/invoices/{result.Id}";
        await SendAsync(result, StatusCodes.Status201Created, ct);
    }
}
