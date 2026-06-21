using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CamerinoSportsServicios.Data.Migrations
{
    /// <inheritdoc />
    public partial class GuardarImagenHistoricaPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagenProducto",
                table: "PedidoDetalles",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagenProducto",
                table: "PedidoDetalles");
        }
    }
}
