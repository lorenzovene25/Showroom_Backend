using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services.Interfaces;

public interface IUserService
{
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByEmailAsync(string email);
    Task<UserDto?> GetByIdAsync(int id);
    Task<IEnumerable<TicketDto>> GetTicketsAsync(int id, string culture = "en");
    Task<IEnumerable<OrderDto>> GetOrdersAsync(int id, string culture = "en");
    Task<OrderDto?> GetOrderByIdAsync(int id, int orderId, string culture = "en");
    Task<CartDto?> GetCartAsync(int id, string culture = "en");
    Task<string?> GetPasswordHashAsync(int id);
    Task<UserDto?> PatchAsync(int id, PatchUserDto dto);
    Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto);

    Task<string?> LoginAsync(LoginUserDto dto);
    Task<bool> RegisterAsync(CreateUserDto dto);
    Task<bool> ChangePasswordAsync(ChangePasswordUserDto dto);
}