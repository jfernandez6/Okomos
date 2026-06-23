using FastEndpoints;
using Okomos.SharedKernel.Abstractions.CQRS;

namespace Okomos.Accounting.Features.CreateJournalEntry;

public sealed class CreateJournalEntryEndpoint : Endpoint<CreateJournalEntryCommand, CreateJournalEntryResponse>
{
    public override void Configure()
    {
        Post("/api/accounting/journal-entries");
        AuthSchemes("Bearer");
        Tags("Accounting");
    }

    public override async Task HandleAsync(CreateJournalEntryCommand req, CancellationToken ct)
    {
        var handler = Resolve<Okomos.SharedKernel.Abstractions.CQRS.ICommandHandler<CreateJournalEntryCommand, Guid>>();
        var id = await handler.HandleAsync(req, ct);
        HttpContext.Response.Headers.Location = $"/api/accounting/journal-entries/{id}";
        await SendAsync(new CreateJournalEntryResponse(id), StatusCodes.Status201Created, ct);
    }
}

public sealed record CreateJournalEntryResponse(Guid Id);
