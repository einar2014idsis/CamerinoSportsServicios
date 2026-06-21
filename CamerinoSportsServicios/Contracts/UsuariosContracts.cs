using System.ComponentModel.DataAnnotations;

namespace CamerinoSportsServicios.Contracts;

public record RegistrarUsuarioRequest(
    [Required, StringLength(100, MinimumLength = 2),
     RegularExpression(@"^[\p{L}]+(?:[ '\-][\p{L}]+)*$",
         ErrorMessage = "El nombre solo puede contener letras.")] string Nombre,
    [Required, StringLength(120, MinimumLength = 2),
     RegularExpression(@"^[\p{L}]+(?:[ '\-][\p{L}]+)*$",
         ErrorMessage = "Los apellidos solo pueden contener letras.")] string Apellidos,
    [Required, EmailAddress, StringLength(180)] string Correo,
    [Required, MinLength(6), StringLength(100)] string Password);

public record LoginRequest(
    [Required, EmailAddress] string Correo,
    [Required] string Password);

public record ActualizarUsuarioRequest(
    [Required, StringLength(100, MinimumLength = 2),
     RegularExpression(@"^[\p{L}]+(?:[ '\-][\p{L}]+)*$")] string Nombre,
    [Required, StringLength(120, MinimumLength = 2),
     RegularExpression(@"^[\p{L}]+(?:[ '\-][\p{L}]+)*$")] string Apellidos,
    string? Password);

public record UsuarioResponse(
    int Id,
    string Nombre,
    string Apellidos,
    string Correo,
    string Rol);
