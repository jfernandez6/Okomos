using FluentAssertions;
using Okomos.Accounting.Features.CreateJournalEntry;

namespace Okomos.Accounting.Tests.Features;

public class CreateJournalEntryCommandValidatorTests
{
    private readonly CreateJournalEntryCommandValidator _validator = new();

    [Fact]
    public async Task Should_Fail_When_Debit_And_Credit_Differ()
    {
        var result = await _validator.ValidateAsync(new CreateJournalEntryCommand("Entry", 100m, 50m));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateJournalEntryCommand.Credit));
    }

    [Fact]
    public async Task Should_Pass_When_Balanced()
    {
        var result = await _validator.ValidateAsync(new CreateJournalEntryCommand("Entry", 100m, 100m));

        result.IsValid.Should().BeTrue();
    }
}
