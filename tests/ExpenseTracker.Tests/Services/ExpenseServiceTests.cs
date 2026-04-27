using ExpenseTracker.Application.Mappings;

namespace ExpenseTracker.Tests.Services;

public sealed class ExpenseServiceTests
{
    // -------------------------------------------------------------------------
    // Test fixtures
    // -------------------------------------------------------------------------

    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IExpenseRepository> _repoMock = new();
    private readonly Mock<IValidator<CreateExpenseRequest>> _createValidatorMock = new();
    private readonly Mock<IValidator<UpdateExpenseRequest>> _updateValidatorMock = new();
    private readonly IMapper _mapper;
    private readonly ExpenseService _sut;

    public ExpenseServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ExpenseMappingProfile>());
        _mapper = config.CreateMapper();

        _unitOfWorkMock.Setup(u => u.Expenses).Returns(_repoMock.Object);

        // Default: validators always pass
        _createValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateExpenseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateExpenseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _sut = new ExpenseService(
            _unitOfWorkMock.Object,
            _mapper,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            NullLogger<ExpenseService>.Instance);
    }

    // -------------------------------------------------------------------------
    // GetAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ReturnsListOfExpenseDtos()
    {
        // Arrange
        var userId = "user-001";
        var expenses = new List<Expense>
        {
            MakeExpense(title: "Lunch", amount: 12m, userId: userId),
            MakeExpense(title: "Taxi", amount: 25m, userId: userId, category: Category.Travel)
        };

        _repoMock
            .Setup(r => r.GetAllAsync(userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);

        // Act
        var result = await _sut.GetAllAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Select(e => e.Title).Should().BeEquivalentTo("Lunch", "Taxi");
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsExpenseDto()
    {
        // Arrange
        var expense = MakeExpense(title: "Coffee", amount: 4.50m);

        _repoMock
            .Setup(r => r.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        // Act
        var result = await _sut.GetByIdAsync(expense.Id);

        // Assert
        result.Id.Should().Be(expense.Id);
        result.Title.Should().Be("Coffee");
        result.Amount.Should().Be(4.50m);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expense?)null);

        // Act
        var act = async () => await _sut.GetByIdAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{id}*");
    }

    // -------------------------------------------------------------------------
    // CreateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedDto()
    {
        // Arrange
        var request = new CreateExpenseRequest(
            Title: "Hotel",
            Amount: 150m,
            Category: Category.Travel,
            Date: DateTime.UtcNow.AddDays(-1),
            UserId: "user-001",
            Notes: "Conference");

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Title.Should().Be("Hotel");
        result.Amount.Should().Be(150m);
        result.Category.Should().Be(Category.Travel);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ExistingId_ReturnsUpdatedDto()
    {
        // Arrange
        var expense = MakeExpense(title: "Old Title", amount: 10m);
        var request = new UpdateExpenseRequest(
            Title: "New Title",
            Amount: 99m,
            Category: Category.Office,
            Date: DateTime.UtcNow.AddDays(-2),
            Notes: "Updated");

        _repoMock
            .Setup(r => r.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateAsync(expense.Id, request);

        // Assert
        result.Title.Should().Be("New Title");
        result.Amount.Should().Be(99m);
        _repoMock.Verify(r => r.Update(It.IsAny<Expense>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateExpenseRequest("T", 1m, Category.Food, DateTime.UtcNow, null);

        _repoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expense?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesSuccessfully()
    {
        // Arrange
        var expense = MakeExpense();

        _repoMock
            .Setup(r => r.GetByIdAsync(expense.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expense);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(expense.Id);

        // Assert
        _repoMock.Verify(r => r.Delete(expense), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expense?)null);

        // Act
        var act = async () => await _sut.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    // -------------------------------------------------------------------------
    // GetSummaryAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetSummaryAsync_ReturnsCorrectTotals()
    {
        // Arrange
        var userId = "user-001";
        var summary = new Dictionary<Category, decimal>
        {
            { Category.Food, 45.50m },
            { Category.Travel, 200m }
        };

        _repoMock
            .Setup(r => r.GetSummaryAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        // Act
        var result = await _sut.GetSummaryAsync(userId);

        // Assert
        result.GrandTotal.Should().Be(245.50m);
        result.ByCategory.Should().ContainKey("Food").WhoseValue.Should().Be(45.50m);
        result.ByCategory.Should().ContainKey("Travel").WhoseValue.Should().Be(200m);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Expense MakeExpense(
        string title = "Test Expense",
        decimal amount = 10m,
        string userId = "user-001",
        Category category = Category.Food) => new()
    {
        Id = Guid.NewGuid(),
        Title = title,
        Amount = amount,
        Category = category,
        Date = DateTime.UtcNow.AddDays(-1),
        UserId = userId,
        Notes = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
