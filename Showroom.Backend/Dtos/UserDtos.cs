namespace Showroom.Backend.Dtos;

// ══════════════════════════════════════════════════════════════════
//  USER
// ══════════════════════════════════════════════════════════════════

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public int? CartId { get; set; }
    public DateTime CreatedAt { get; set; }
}

// PasswordHash is expected pre-hashed by the caller (controller / auth layer)
public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsAdmin { get; set; } = false;
}

public class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public string? PasswordHash { get; set; }
}

public class PatchUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public bool? IsAdmin { get; set; }
    public string? PasswordHash { get; set; }
}

public class ChangePasswordUserDto
{
    public string? Email { get; set; }
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
}

/// <summary>
/// Usata sia per registrazione che per login, servono per entrambi solamente email e password, che saranno poi gestite dall'auth layer (hashing, validazione, ecc.)
/// </summary>
public class LoginUserDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}