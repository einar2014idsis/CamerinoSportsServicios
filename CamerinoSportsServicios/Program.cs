using CamerinoSportsServicios.Data;
using CamerinoSportsServicios.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<PasswordService>();

builder.Services.AddDbContext<CamerinoSportsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CamerinoSports")));

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CamerinoSportsContext>();
    var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();

    await db.Database.MigrateAsync();
    await DataSeeder.InicializarAsync(db, passwordService);
}

app.Run();