using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services.Interfaces;

public interface IExhibitionService
{
    Task<IEnumerable<ExhibitionDto>> GetAllAsync(string culture = "en");
    Task<IEnumerable<ExhibitionDto>> GetByStatusAsync(string status, string culture = "en");
    Task<ExhibitionDto?> GetByIdAsync(int id, string culture = "en");
    Task<ExhibitionDto> CreateAsync(CreateExhibitionDto dto);
    Task<ExhibitionDto?> UpdateAsync(int id, UpdateExhibitionDto dto, string culture = "en");
    Task<ExhibitionDto?> PatchAsync(int id, PatchExhibitionDto dto, string culture = "en");
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<ArtworkDto>> GetAllArtworksAsync(int exhibitionId, string culture = "en");
    Task<IEnumerable<ExhibitionTimeSlotDto>> GetAllTimeSlotsAsync(int exhibitionId, string culture = "en");
}