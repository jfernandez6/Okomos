using Okomos.Accounting.Features.CreateJournalEntry;
using Okomos.Accounting.Features.GetJournalEntryById;
using Okomos.SharedKernel.Abstractions.CQRS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Okomos.Accounting;

public static class AccountingEndpoints
{
    public static IEndpointRouteBuilder MapAccountingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounting")
            .WithTags("Accounting")
            .RequireAuthorization();

        group.MapPost("/journal-entries", async (
            CreateJournalEntryCommand command,
            ICommandHandler<CreateJournalEntryCommand, Guid> handler,
            CancellationToken ct) =>
        {
            var id = await handler.HandleAsync(command, ct);
            return Results.Created($"/api/accounting/journal-entries/{id}", new { Id = id });
        })
        .WithName("CreateJournalEntry");

        group.MapGet("/journal-entries/{entryId:guid}", async (
            Guid entryId,
            IQueryHandler<GetJournalEntryByIdQuery, JournalEntryDto?> handler,
            CancellationToken ct) =>
        {
            var entry = await handler.HandleAsync(new GetJournalEntryByIdQuery(entryId), ct);
            return entry is null ? Results.NotFound() : Results.Ok(entry);
        })
        .WithName("GetJournalEntryById");

        return app;
    }
}
