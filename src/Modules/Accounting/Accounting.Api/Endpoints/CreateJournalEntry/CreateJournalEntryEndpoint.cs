using Accounting.Application.Features.CreateJournalEntry;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Accounting.Api.Endpoints.CreateJournalEntry;

public sealed class CreateJournalEntryEndpoint : Endpoint<CreateJournalEntryRequest, CreateJournalEntryResponse>
{
    private readonly CreateJournalEntryHandler _handler;

    public CreateJournalEntryEndpoint(CreateJournalEntryHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/journal-entries");
        Group<ModuleRoutes>();
        AuthSchemes("Bearer");
    }

    public override async Task HandleAsync(CreateJournalEntryRequest req, CancellationToken ct)
    {
        var result = await _handler.Handle(req, ct);
        HttpContext.Response.Headers.Location = $"/accounting/journal-entries/{result.Id}";
        await SendAsync(result, StatusCodes.Status201Created, ct);
    }
}
