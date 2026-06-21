using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CamerinoSportsServicios.Models;

public class Promocion
{
    public int Id { get; set; }
    public int ProductoId { get; set; }

    [Range(0.01, double.MaxValue)]
    public double PrecioOriginal { get; set; }

    [Range(0.01, double.MaxValue)]
    public double PrecioOferta { get; set; }

    [Required, StringLength(20)]
    public string FechaInicio { get; set; } = "";

    [Required, StringLength(20)]
    public string FechaFin { get; set; } = "";

    public bool Activa { get; set; } = true;

    [JsonIgnore]
    public Producto Producto { get; set; } = null!;
}
