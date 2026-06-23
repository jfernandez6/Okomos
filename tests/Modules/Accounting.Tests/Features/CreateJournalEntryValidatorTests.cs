using Accounting.Application.Features.CreateJournalEntry;

namespace Accounting.Tests.Features;

public class CreateJournalEntryValidatorTests
{
    private readonly CreateJournalEntryValidator _validator = new();

    [Fact]
    public async Task Should_Fail_When_Debit_And_Credit_Differ()
    {
        var result = await _validator.ValidateAsync(new CreateJournalEntryRequest("Entry", 100m, 50m));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateJournalEntryRequest.Credit));
    }

    [Fact]
    public async Task Should_Pass_When_Balanced()
    {
        var result = await _validator.ValidateAsync(new CreateJournalEntryRequest("Entry", 100m, 100m));

        result.IsValid.Should().BeTrue();
    }
}
