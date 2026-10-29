using System.ComponentModel.DataAnnotations;

namespace ManConstruc.Api.Contracts;

public class RegisterRequest
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [MaxLength(180)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres.")]
    [MaxLength(160)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(160)]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}

public record UserDto(Guid Id, string Email, string FullName, string CompanyName);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);
