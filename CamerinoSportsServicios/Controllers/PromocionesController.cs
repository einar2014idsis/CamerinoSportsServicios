using CamerinoSportsServicios.Contracts;
using CamerinoSportsServicios.Data;
using CamerinoSportsServicios.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CamerinoSportsServicios.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromocionesController : ControllerBase
{
    private readonly CamerinoSportsContext _context;

    public PromocionesController(CamerinoSportsContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<PromocionResponse>>> GetPromociones()
    {
        return await Proyectar(_context.Promociones.AsNoTracking().OrderByDescending(promocion => promocion.Id))
            .ToListAsync();
    }

    [HttpGet("activas")]
    public async Task<ActionResult<List<PromocionResponse>>> GetActivas()
    {
        return await Proyectar(_context.Promociones.AsNoTracking()
                .Where(promocion => promocion.Activa)
                .OrderByDescending(promocion => promocion.Id))
            .ToListAsync();
    }

    [HttpGet("activas/total")]
    public async Task<ActionResult<int>> ContarActivas()
    {
        return await _context.Promociones.CountAsync(promocion => promocion.Activa);
    }

    [HttpGet("producto/{productoId:int}")]
    public async Task<ActionResult<PromocionResponse>> GetPorProducto(int productoId)
    {
        var promocion = await Proyectar(_context.Promociones.AsNoTracking()
                .Where(item => item.ProductoId == productoId && item.Activa))
            .FirstOrDefaultAsync();

        return promocion == null ? NotFound() : promocion;
    }

    [HttpPost]
    public async Task<ActionResult<PromocionResponse>> Crear(GuardarPromocionRequest request)
    {
        var error = await Validar(request);
        if (error != null)
        {
            return BadRequest(error);
        }

        var promocion = Mapear(request);
        _context.Promociones.Add(promocion);
        await _context.SaveChangesAsync();

        var respuesta = await Proyectar(_context.Promociones.AsNoTracking()
                .Where(item => item.Id == promocion.Id))
            .FirstAsync();
        return CreatedAtAction(nameof(GetPorProducto), new { productoId = promocion.ProductoId }, respuesta);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, GuardarPromocionRequest request)
    {
        var guardada = await _context.Promociones.FindAsync(id);
        if (guardada == null)
        {
            return NotFound();
        }

        var error = await Validar(request);
        if (error != null)
        {
            return BadRequest(error);
        }

        guardada.ProductoId = request.ProductoId;
        guardada.PrecioOriginal = request.PrecioOriginal;
        guardada.PrecioOferta = request.PrecioOferta;
        guardada.FechaInicio = request.FechaInicio.Trim();
        guardada.FechaFin = request.FechaFin.Trim();
        guardada.Activa = request.Activa;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var promocion = await _context.Promociones.FindAsync(id);
        if (promocion == null)
        {
            return NotFound();
        }

        _context.Promociones.Remove(promocion);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static IQueryable<PromocionResponse> Proyectar(IQueryable<Promocion> consulta)
    {
        // agrega el nombre y la imagen del producto a la respuesta
        return consulta.Select(promocion => new PromocionResponse(
                promocion.Id,
                promocion.ProductoId,
                promocion.PrecioOriginal,
                promocion.PrecioOferta,
                promocion.FechaInicio,
                promocion.FechaFin,
                promocion.Activa,
                promocion.Producto.Nombre,
                promocion.Producto.Imagen));
    }

    private async Task<string?> Validar(GuardarPromocionRequest promocion)
    {
        // una promocion solo puede pertenecer a un producto existente
        if (!await _context.Productos.AnyAsync(producto => producto.Id == promocion.ProductoId))
        {
            return "El producto indicado no existe.";
        }

        if (promocion.PrecioOriginal <= 0 ||
            promocion.PrecioOferta <= 0 ||
            promocion.PrecioOferta >= promocion.PrecioOriginal)
        {
            return "El precio de oferta debe ser positivo y menor al precio original.";
        }

        bool inicioValido = DateOnly.TryParseExact(
            promocion.FechaInicio, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateOnly inicio);
        bool finValido = DateOnly.TryParseExact(
            promocion.FechaFin, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateOnly fin);

        // la fecha final no puede estar antes de la fecha inicial
        if (!inicioValido || !finValido || fin < inicio)
        {
            return "El rango de fechas no es válido.";
        }

        return null;
    }

    private static Promocion Mapear(GuardarPromocionRequest request) =>
        new()
        {
            ProductoId = request.ProductoId,
            PrecioOriginal = request.PrecioOriginal,
            PrecioOferta = request.PrecioOferta,
            FechaInicio = request.FechaInicio.Trim(),
            FechaFin = request.FechaFin.Trim(),
            Activa = request.Activa
        };
}
