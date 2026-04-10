using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services;

public interface IExhibitionService
{
    Task<IEnumerable<ExhibitionDto>> GetAllAsync(string culture = "en");
    Task<IEnumerable<ArtworkDto>> GetAllArtworksAsync(string culture = "en");
    Task<IEnumerable<ExhibitionTimeSlotDto>> GetAllTimeSlotsAsync(string culture = "en");
    Task<IEnumerable<ExhibitionDto>> GetByStatusAsync(string status, string culture = "en");
    Task<ExhibitionDto?> GetByIdAsync(int id, string culture = "en");
}