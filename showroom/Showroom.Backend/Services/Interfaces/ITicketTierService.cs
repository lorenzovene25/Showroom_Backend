using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services.Interfaces
{
    public interface ITicketTierService
    {
        Task<TicketTierDto> CreateAsync(CreateTicketTierDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TicketTierDto>> GetAllAsync(string culture = "en");
        Task<TicketTierDto?> GetByIdAsync(int id, string culture = "en");
        Task<TicketTierDto?> PatchAsync(int id, PatchTicketTierDto dto, string culture = "en");
        Task<TicketTierDto?> UpdateAsync(int id, UpdateTicketTierDto dto, string culture = "en");
    }
}