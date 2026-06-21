using System.ComponentModel.DataAnnotations;

namespace CamerinoSportsServicios.Models;

public class Consejo
{
    public int Id { get; set; }

    [Required, StringLength(160)]
    public string Titulo { get; set; } = "";

    [Required, StringLength(1000)]
    public string Descripcion { get; set; } = "";

    [Required, StringLength(80)]
    public string Categoria { get; set; } = "";

    [StringLength(1000)]
    public string? Imagen { get; set; }

    public bool Activo { get; set; } = true;
}
