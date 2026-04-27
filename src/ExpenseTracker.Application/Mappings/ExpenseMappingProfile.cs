using AutoMapper;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Mappings;

/// <summary>
/// AutoMapper profile that defines all <see cref="Expense"/> ↔ DTO mappings.
/// </summary>
public sealed class ExpenseMappingProfile : Profile
{
    /// <summary>Registers all expense mappings.</summary>
    public ExpenseMappingProfile()
    {
        // Entity → DTO
        CreateMap<Expense, ExpenseDto>()
            .ConstructUsing((src, _) => new ExpenseDto(
                src.Id,
                src.Title,
                src.Amount,
                src.Category,
                src.Category.ToString(),
                src.Date,
                src.UserId,
                src.Notes,
                src.CreatedAt,
                src.UpdatedAt));

        // CreateRequest → Entity
        CreateMap<CreateExpenseRequest, Expense>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        // UpdateRequest → Entity (partial update applied by the service)
        CreateMap<UpdateExpenseRequest, Expense>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}
