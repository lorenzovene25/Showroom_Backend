using Showroom.Backend.Dtos;
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
        Task<string?> GetPasswordHashAsync(int id);
        Task<UserDto?> PatchAsync(int id, PatchUserDto dto);
        Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto);
    }
}