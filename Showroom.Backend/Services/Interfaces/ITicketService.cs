using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services.Interfaces
{
    public interface ITicketService
    {
        Task<TicketDto> CreateAsync(CreateTicketDto dto, string culture = "en");
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TicketDto>> GetAllAsync(string culture = "en");
        Task<IEnumerable<TicketDto>> GetByExhibitionAsync(int exhibitionId, string culture = "en");
        Task<TicketDto?> GetByIdAsync(int id, string culture = "en");
        Task<IEnumerable<TicketDto>> GetByUserAsync(int userId, string culture = "en");
        Task<IEnumerable<TicketTierDto>> GetAllTicketTiersAsync(string culture = "en");
    }
}