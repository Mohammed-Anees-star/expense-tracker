namespace ExpenseTracker.Tests.Validators;

public sealed class UpdateExpenseRequestValidatorTests
{
    private readonly UpdateExpenseRequestValidator _sut = new();

    // -------------------------------------------------------------------------
    // Happy path
    // -------------------------------------------------------------------------

    [Fact]
    public void ValidRequest_ShouldPassAllRules()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    // -------------------------------------------------------------------------
    // Title
    // -------------------------------------------------------------------------

    [Fact]
    public void EmptyTitle_ShouldFailWithRequiredMessage()
    {
        var request = ValidRequest() with { Title = string.Empty };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void TitleOver200Chars_ShouldFailWithLengthMessage()
    {
        var request = ValidRequest() with { Title = new string('X', 201) };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Title must not exceed 200 characters.");
    }

    // -------------------------------------------------------------------------
    // Amount
    // -------------------------------------------------------------------------

    [Fact]
    public void ZeroAmount_ShouldFail()
    {
        var request = ValidRequest() with { Amount = 0 };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Amount)
              .WithErrorMessage("Amount must be greater than zero.");
    }

    [Fact]
    public void NegativeAmount_ShouldFail()
    {
        var request = ValidRequest() with { Amount = -50m };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void AmountOver1Million_ShouldFail()
    {
        var request = ValidRequest() with { Amount = 1_000_001m };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Amount)
              .WithErrorMessage("Amount must not exceed 1,000,000.");
    }

    // -------------------------------------------------------------------------
    // Date
    // -------------------------------------------------------------------------

    [Fact]
    public void FarFutureDate_ShouldFail()
    {
        var request = ValidRequest() with { Date = DateTime.UtcNow.AddDays(10) };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Date)
              .WithErrorMessage("Date cannot be in the future.");
    }

    // -------------------------------------------------------------------------
    // Notes (optional but bounded)
    // -------------------------------------------------------------------------

    [Fact]
    public void NotesOver1000Chars_ShouldFail()
    {
        var request = ValidRequest() with { Notes = new string('N', 1001) };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Notes)
              .WithErrorMessage("Notes must not exceed 1,000 characters.");
    }

    [Fact]
    public void NullNotes_ShouldPassValidation()
    {
        var request = ValidRequest() with { Notes = null };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Notes);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static UpdateExpenseRequest ValidRequest() => new(
        Title: "Updated Lunch",
        Amount: 55m,
        Category: Category.Food,
        Date: DateTime.UtcNow.AddDays(-1),
        Notes: null);
}
