using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CamerinoSportsServicios.Models;

public class Producto : IValidatableObject
{
    public int Id { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    [RegularExpression(@"^[\p{L}\d]+(?:[ .,'\-/][\p{L}\d]+)*$")]
    public string Nombre { get; set; } = "";

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Required, StringLength(50), RegularExpression("^(Calzado|Ropa|Accesorios)$")]
    public string Categoria { get; set; } = "";

    [StringLength(30)]
    public string? Talla { get; set; }

    [StringLength(50)]
    [RegularExpression(@"^$|^[\p{L}]+(?:[ /\-][\p{L}]+)*$")]
    public string? Color { get; set; }

    [Range(0.01, 100000)]
    public double Precio { get; set; }

    [Range(0, 100000)]
    public int Stock { get; set; }

    [Required, StringLength(2000, MinimumLength = 1)]
    public string? Imagen { get; set; }

    public bool Sostenible { get; set; }

    [JsonIgnore]
    public ICollection<PedidoDetalle> DetallesPedido { get; set; } = new List<PedidoDetalle>();

    [JsonIgnore]
    public ICollection<Promocion> Promociones { get; set; } = new List<Promocion>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        string talla = Talla?.Trim().ToUpperInvariant() ?? "";

        if (Categoria == "Ropa" &&
            !new[] { "XXS", "XS", "S", "M", "L", "XL", "XXL", "XXXL", "ÚNICA", "UNICA" }.Contains(talla))
        {
            yield return new ValidationResult(
                "La talla de ropa no es válida.",
                [nameof(Talla)]);
        }

        if (Categoria == "Calzado" &&
            (!double.TryParse(talla, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out double numero) ||
             numero < 20 || numero > 50))
        {
            yield return new ValidationResult(
                "La talla de calzado debe estar entre 20 y 50.",
                [nameof(Talla)]);
        }
    }
}
