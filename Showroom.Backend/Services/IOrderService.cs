using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateAsync(CreateOrderDto dto, string culture = "en");
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<OrderDto>> GetAllAsync(string culture = "en");
        Task<OrderDto?> GetByIdAsync(int id, string culture = "en");
        Task<IEnumerable<OrderDto>> GetByUserAsync(int userId, string culture = "en");
        Task<OrderDto?> PatchStatusAsync(int id, PatchOrderStatusDto dto, string culture = "en");
    }
}