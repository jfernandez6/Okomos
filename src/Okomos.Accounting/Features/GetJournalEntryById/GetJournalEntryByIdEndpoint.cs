using FastEndpoints;
using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Accounting.Features.GetJournalEntryById;

public sealed class GetJournalEntryByIdRequest
{
    public Guid EntryId { get; set; }
}

public sealed class GetJournalEntryByIdEndpoint : Endpoint<GetJournalEntryByIdRequest, JournalEntryDto>
{
    public override void Configure()
    {
        Get("/api/accounting/journal-entries/{EntryId}");
        AuthSchemes("Bearer");
        Tags("Accounting");
    }

    public override async Task HandleAsync(GetJournalEntryByIdRequest req, CancellationToken ct)
    {
        var handler = Resolve<Okomos.SharedKernel.Abstractions.CQRS.IQueryHandler<GetJournalEntryByIdQuery, JournalEntryDto?>>();
        var entry = await handler.HandleAsync(new GetJournalEntryByIdQuery(req.EntryId), ct);

        if (entry is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(entry, cancellation: ct);
    }
}
