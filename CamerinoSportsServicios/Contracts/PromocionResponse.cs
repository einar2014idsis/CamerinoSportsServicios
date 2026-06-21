using System.ComponentModel.DataAnnotations;

namespace CamerinoSportsServicios.Contracts;

public record GuardarPromocionRequest(
    [Range(0, int.MaxValue)]
    int ProductoId,
    [Range(0.01, 100000)]
    double PrecioOriginal,
    [Range(0.01, 100000)]
    double PrecioOferta,
    [Required, RegularExpression(@"^\d{4}-\d{2}-\d{2}$")]
    string FechaInicio,
    [Required, RegularExpression(@"^\d{4}-\d{2}-\d{2}$")]
    string FechaFin,
    bool Activa);

public record PromocionResponse(
    int Id,
    int ProductoId,
    double PrecioOriginal,
    double PrecioOferta,
    string FechaInicio,
    string FechaFin,
    bool Activa,
    string NombreProducto,
    string? ImagenProducto);
