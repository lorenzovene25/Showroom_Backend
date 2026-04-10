using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services
{
    public interface IUserService
    {
        Task<UserDto> CreateAsync(CreateUserDto dto);
      
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByEmailAsync(string email);
        Task<UserDto?> GetByIdAsync(int id);
        Task<IEnumerable<TicketDto>> GetTicketsAsync(int id);
        Task<IEnumerable<OrderDto>> GetOrdersAsync(int id);
        Task<OrderDto?> GetOrderByIdAsync(int id, int orderId);
        Task<CartDto?> GetCartAsync(int id);
        Task<string?> GetPasswordHashAsync(int id);
        Task<UserDto?> PatchAsync(int id, PatchUserDto dto);
        Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto);
    }
}