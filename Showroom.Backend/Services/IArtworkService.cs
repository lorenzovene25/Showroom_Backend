using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services
{
    public interface IArtworkService
    {
        Task<ArtworkDto> CreateAsync(CreateArtworkDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ArtworkDto>> GetAllAsync(string culture = "en");
        Task<IEnumerable<ArtworkDto>> GetByExhibitionAsync(int exhibitionId, string culture = "en");
        Task<ArtworkDto?> GetByIdAsync(int id, string culture = "en");
        Task<bool> LinkExhibitionAsync(int artworkId, int exhibitionId);
        Task<ArtworkDto?> PatchAsync(int id, PatchArtworkDto dto, string culture = "en");
        Task<bool> UnlinkExhibitionAsync(int artworkId, int exhibitionId);
        Task<ArtworkDto?> UpdateAsync(int id, UpdateArtworkDto dto, string culture = "en");
    }
}