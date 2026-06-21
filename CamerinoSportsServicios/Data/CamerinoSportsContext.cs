using CamerinoSportsServicios.Models;
using Microsoft.EntityFrameworkCore;

namespace CamerinoSportsServicios.Data;

public class CamerinoSportsContext : DbContext
{
    public CamerinoSportsContext(DbContextOptions<CamerinoSportsContext> options)
        : base(options)
    {
    }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoDetalle> PedidoDetalles => Set<PedidoDetalle>();
    public DbSet<Promocion> Promociones => Set<Promocion>();
    public DbSet<Consejo> Consejos => Set<Consejo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>()
            .Property(producto => producto.Id)
            .ValueGeneratedNever();

        modelBuilder.Entity<Usuario>()
            .HasIndex(usuario => usuario.Correo)
            .IsUnique();

        modelBuilder.Entity<Pedido>()
            .HasOne(pedido => pedido.Usuario)
            .WithMany(usuario => usuario.Pedidos)
            .HasForeignKey(pedido => pedido.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PedidoDetalle>()
            .HasOne(detalle => detalle.Pedido)
            .WithMany(pedido => pedido.Detalles)
            .HasForeignKey(detalle => detalle.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PedidoDetalle>()
            .HasOne(detalle => detalle.Producto)
            .WithMany(producto => producto.DetallesPedido)
            .HasForeignKey(detalle => detalle.ProductoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Promocion>()
            .HasOne(promocion => promocion.Producto)
            .WithMany(producto => producto.Promociones)
            .HasForeignKey(promocion => promocion.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Promocion>()
            .HasIndex(promocion => promocion.ProductoId);
    }
}
