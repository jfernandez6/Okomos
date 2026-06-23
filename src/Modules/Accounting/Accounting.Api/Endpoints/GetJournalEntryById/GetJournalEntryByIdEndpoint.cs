using Accounting.Application.Features.GetJournalEntryById;
using FastEndpoints;

namespace Accounting.Api.Endpoints.GetJournalEntryById;

public sealed class GetJournalEntryByIdEndpoint : Endpoint<GetJournalEntryByIdRequest, JournalEntryDto>
{
    private readonly GetJournalEntryByIdHandler _handler;

    public GetJournalEntryByIdEndpoint(GetJournalEntryByIdHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/journal-entries/{EntryId}");
        Group<ModuleRoutes>();
        AuthSchemes("Bearer");
    }

    public override async Task HandleAsync(GetJournalEntryByIdRequest req, CancellationToken ct)
    {
        var entry = await _handler.Handle(req, ct);

        if (entry is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(entry, cancellation: ct);
    }
}
