using FluentValidation;

namespace Showroom.Backend.Dtos;

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}

public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
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
    public string Email { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}

public class LoginUserDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

// validators

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PasswordHash).NotEmpty().When(x => x.PasswordHash != null);
    }
}

public class PatchUserDtoValidator : AbstractValidator<PatchUserDto>
{
    public PatchUserDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).When(x => x.FirstName != null);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).When(x => x.LastName != null);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.PasswordHash).NotEmpty().When(x => x.PasswordHash != null);
    }
}

public class ChangePasswordUserDtoValidator : AbstractValidator<ChangePasswordUserDto>
{
    public ChangePasswordUserDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.OldPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).NotEqual(x => x.OldPassword)
        .Matches(@"[A-Z]").WithMessage("La password deve contenere almeno una lettera maiuscola.")
        .Matches(@"[a-z]").WithMessage("La password deve contenere almeno una lettera minuscola.")
        .Matches(@"\d").WithMessage("La password deve contenere almeno un numero.")
        .Matches(@"[\W_]").WithMessage("La password deve contenere almeno un carattere speciale.");
    }
}

public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}