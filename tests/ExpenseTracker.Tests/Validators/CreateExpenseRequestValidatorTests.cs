namespace ExpenseTracker.Tests.Validators;

public sealed class CreateExpenseRequestValidatorTests
{
    private readonly CreateExpenseRequestValidator _sut = new();

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
        // Arrange
        var request = ValidRequest() with { Title = string.Empty };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void TitleOver200Chars_ShouldFailWithLengthMessage()
    {
        // Arrange
        var request = ValidRequest() with { Title = new string('A', 201) };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Title must not exceed 200 characters.");
    }

    // -------------------------------------------------------------------------
    // Amount
    // -------------------------------------------------------------------------

    [Fact]
    public void ZeroAmount_ShouldFailWithGreaterThanZeroMessage()
    {
        // Arrange
        var request = ValidRequest() with { Amount = 0 };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
              .WithErrorMessage("Amount must be greater than zero.");
    }

    [Fact]
    public void NegativeAmount_ShouldFailValidation()
    {
        // Arrange
        var request = ValidRequest() with { Amount = -1m };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void AmountOver1Million_ShouldFailWithMaxMessage()
    {
        // Arrange
        var request = ValidRequest() with { Amount = 1_000_001m };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount)
              .WithErrorMessage("Amount must not exceed 1,000,000.");
    }

    // -------------------------------------------------------------------------
    // UserId
    // -------------------------------------------------------------------------

    [Fact]
    public void EmptyUserId_ShouldFailWithRequiredMessage()
    {
        // Arrange
        var request = ValidRequest() with { UserId = string.Empty };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("UserId is required.");
    }

    // -------------------------------------------------------------------------
    // Date
    // -------------------------------------------------------------------------

    [Fact]
    public void FarFutureDate_ShouldFailValidation()
    {
        // Arrange
        var request = ValidRequest() with { Date = DateTime.UtcNow.AddDays(10) };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Date)
              .WithErrorMessage("Date cannot be in the future.");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static CreateExpenseRequest ValidRequest() => new(
        Title: "Team Lunch",
        Amount: 45.50m,
        Category: Category.Food,
        Date: DateTime.UtcNow.AddDays(-1),
        UserId: "user-001",
        Notes: null);
}
