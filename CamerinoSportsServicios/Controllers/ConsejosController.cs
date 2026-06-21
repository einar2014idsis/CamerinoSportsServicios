using CamerinoSportsServicios.Data;
using CamerinoSportsServicios.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CamerinoSportsServicios.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsejosController : ControllerBase
{
    private readonly CamerinoSportsContext _context;

    public ConsejosController(CamerinoSportsContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Consejo>>> GetConsejos()
    {
        return await _context.Consejos.AsNoTracking()
            .OrderByDescending(consejo => consejo.Id)
            .ToListAsync();
    }

    [HttpGet("activos")]
    public async Task<ActionResult<List<Consejo>>> GetActivos()
    {
        return await _context.Consejos.AsNoTracking()
            .Where(consejo => consejo.Activo)
            .OrderBy(consejo => consejo.Id)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Consejo>> GetConsejo(int id)
    {
        var consejo = await _context.Consejos.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        return consejo == null ? NotFound() : consejo;
    }

    [HttpPost]
    public async Task<ActionResult<Consejo>> Crear(Consejo consejo)
    {
        consejo.Id = 0;
        _context.Consejos.Add(consejo);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetConsejo), new { id = consejo.Id }, consejo);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, Consejo consejo)
    {
        var guardado = await _context.Consejos.FindAsync(id);
        if (guardado == null)
        {
            return NotFound();
        }

        guardado.Titulo = consejo.Titulo.Trim();
        guardado.Descripcion = consejo.Descripcion.Trim();
        guardado.Categoria = consejo.Categoria.Trim();
        guardado.Imagen = consejo.Imagen?.Trim();
        guardado.Activo = consejo.Activo;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var consejo = await _context.Consejos.FindAsync(id);
        if (consejo == null)
        {
            return NotFound();
        }

        _context.Consejos.Remove(consejo);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
