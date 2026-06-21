using CamerinoSportsServicios.Contracts;
using CamerinoSportsServicios.Data;
using CamerinoSportsServicios.Models;
using CamerinoSportsServicios.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CamerinoSportsServicios.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly CamerinoSportsContext _context;
    private readonly PasswordService _passwordService;

    public UsuariosController(CamerinoSportsContext context, PasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UsuarioResponse>>> GetUsuarios()
    {
        return await _context.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Nombre)
            .Select(usuario => new UsuarioResponse(
                usuario.Id,
                usuario.Nombre,
                usuario.Apellidos,
                usuario.Correo,
                usuario.Rol))
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioResponse>> GetUsuario(int id)
    {
        var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return usuario == null ? NotFound() : Mapear(usuario);
    }

    [HttpGet("correo/{correo}")]
    public async Task<ActionResult<UsuarioResponse>> GetUsuarioPorCorreo(string correo)
    {
        string correoNormalizado = NormalizarCorreo(correo);
        var usuario = await _context.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Correo == correoNormalizado);

        return usuario == null ? NotFound() : Mapear(usuario);
    }

    [HttpGet("existe")]
    public async Task<ActionResult<bool>> ExisteCorreo([FromQuery] string correo)
    {
        string correoNormalizado = NormalizarCorreo(correo);
        return await _context.Usuarios.AnyAsync(usuario => usuario.Correo == correoNormalizado);
    }

    [HttpGet("clientes/total")]
    public async Task<ActionResult<int>> ContarClientes()
    {
        return await _context.Usuarios.CountAsync(usuario => usuario.Rol == "cliente");
    }

    [HttpPost("registro")]
    public async Task<ActionResult<UsuarioResponse>> Registrar(RegistrarUsuarioRequest request)
    {
        // los correos se guardan en minusculas para evitar registros duplicados
        string correo = NormalizarCorreo(request.Correo);
        if (await _context.Usuarios.AnyAsync(usuario => usuario.Correo == correo))
        {
            return Conflict("Este correo ya está registrado.");
        }

        var usuario = new Usuario
        {
            Nombre = request.Nombre.Trim(),
            Apellidos = request.Apellidos.Trim(),
            Correo = correo,
            // nunca se guarda la contraseña directamente
            PasswordHash = _passwordService.CrearHash(request.Password),
            Rol = "cliente"
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, Mapear(usuario));
    }

    [HttpPost("login")]
    public async Task<ActionResult<UsuarioResponse>> Login(LoginRequest request)
    {
        string correo = NormalizarCorreo(request.Correo);
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(item => item.Correo == correo);

        // se compara la contraseña con el hash almacenado
        if (usuario == null || !_passwordService.Verificar(request.Password, usuario.PasswordHash))
        {
            return Unauthorized("Correo o contraseña incorrectos.");
        }

        return Ok(Mapear(usuario));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, ActualizarUsuarioRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        return await ActualizarUsuario(usuario, request);
    }

    [HttpPut("correo/{correo}")]
    public async Task<IActionResult> ActualizarPorCorreo(string correo, ActualizarUsuarioRequest request)
    {
        string correoNormalizado = NormalizarCorreo(correo);
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(item => item.Correo == correoNormalizado);

        return await ActualizarUsuario(usuario, request);
    }

    private async Task<IActionResult> ActualizarUsuario(
        Usuario? usuario,
        ActualizarUsuarioRequest request)
    {
        if (usuario == null)
        {
            return NotFound();
        }

        usuario.Nombre = request.Nombre.Trim();
        usuario.Apellidos = request.Apellidos.Trim();

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            // la contraseña solo cambia cuando el usuario escribe una nueva
            if (request.Password.Length < 6)
            {
                return BadRequest("La contraseña debe tener al menos 6 caracteres.");
            }

            usuario.PasswordHash = _passwordService.CrearHash(request.Password);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // elimina espacios y unifica el correo antes de buscarlo
    private static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();

    // devuelve solo los datos que la aplicacion necesita mostrar
    private static UsuarioResponse Mapear(Usuario usuario) =>
        new(usuario.Id, usuario.Nombre, usuario.Apellidos, usuario.Correo, usuario.Rol);
}
