namespace Showroom.Backend.Dtos;

// ══════════════════════════════════════════════════════════════════
//  USER
// ══════════════════════════════════════════════════════════════════

public record UserDto(
    int Id, string FirstName, string LastName, string Email,
    bool IsAdmin, int? CartId, DateTime CreatedAt);

// PasswordHash is expected pre-hashed by the caller (controller / auth layer)
public record CreateUserDto(
    string FirstName, string LastName,
    string Email, string PasswordHash,
    bool IsAdmin = false);

public record UpdateUserDto(
    string FirstName, string LastName, string Email,
    bool IsAdmin, string? PasswordHash = null);

public record PatchUserDto(
    string? FirstName = null, string? LastName = null,
    string? Email = null, bool? IsAdmin = null, string? PasswordHash = null);
