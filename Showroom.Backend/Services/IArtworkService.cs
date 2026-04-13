using Npgsql;
using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services
{
    public interface IArtworkService
    {
        Task<IEnumerable<ArtworkDto>> GetAllAsync(string culture = "en");
        Task<ArtworkDto?> GetByIdAsync(int id, string culture = "en");
        Task<ArtworkDto> CreateAsync(CreateArtworkDto dto);
        Task<ArtworkDto?> UpdateAsync(int id, UpdateArtworkDto dto, string culture = "en");
        Task<ArtworkDto?> PatchAsync(int id, PatchArtworkDto dto, string culture = "en");
        Task<bool> DeleteAsync(int id);
        Task UpsertTranslationAsync(NpgsqlConnection conn, NpgsqlTransaction tx, int artworkId, string culture, string? title, string? description, string? historicalPeriod, string? support, string? camera);
        Task PatchTranslationAsync(NpgsqlConnection conn, NpgsqlTransaction tx, int artworkId, string culture, string? title, string? description, string? historicalPeriod, string? support, string? camera);
    }
}