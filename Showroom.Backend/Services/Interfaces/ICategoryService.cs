using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<IEnumerable<SouvenirDto>> GetSouvenirsByCategoryIdAsync(int id, string culture = "en");
    Task<CategoryDto?> GetBySlugAsync(string slug, string culture = "en");
    Task<CategoryDto?> PatchAsync(int id, PatchCategoryDto dto, string culture = "en");
    Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto, string culture = "en");
}