using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services
{
    public interface ISouvenirService
    {
        Task<SouvenirDto> CreateAsync(CreateSouvenirDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<SouvenirDto>> GetAllAsync(string culture = "en");
        Task<IEnumerable<SouvenirDto>> GetByCategoryAsync(int categoryId, string culture = "en");
        Task<SouvenirDto?> GetByIdAsync(int id, string culture = "en");
        Task<IEnumerable<SouvenirDto>> GetInStockAsync(string culture = "en");
        Task<SouvenirDto?> PatchAsync(int id, PatchSouvenirDto dto, string culture = "en");
        Task<SouvenirDto?> UpdateAsync(int id, UpdateSouvenirDto dto, string culture = "en");
    }
}