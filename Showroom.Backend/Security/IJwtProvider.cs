using Showroom.Backend.Dtos;

namespace Showroom.Backend.Security;

public interface IJwtProvider
{
    string GenerateToken(UserDto user);
}