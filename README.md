# Camerino Sports | API de servicios

API REST para el catálogo y los pedidos de una tienda deportiva. Desarrollada con C# y ASP.NET Core, concentra la persistencia y las operaciones que consume la aplicación móvil de Camerino Sports.

## Funcionalidades

- Registro, inicio de sesión y actualización de usuarios; consulta por correo y verificación de existencia.
- Gestión de productos, promociones y consejos, incluyendo consultas de promociones y consejos activos.
- Creación y consulta de pedidos, consulta de detalles, cambio de estado y cancelación.
- Validación de existencias al crear pedidos: el servidor obtiene el precio y stock de la base de datos, calcula subtotales y guarda los cambios en una transacción.
- Conservación de datos históricos del producto en el detalle de pedido.

## Tecnologías y arquitectura

| Área | Implementación |
|---|---|
| API | C#, ASP.NET Core 10, controladores REST |
| Persistencia | Entity Framework Core 10, SQL Server y migraciones |
| Contraseñas | PBKDF2-SHA256 con sal y comparación en tiempo constante |

`Controllers/` expone las operaciones; `Contracts/` define solicitudes y respuestas; `Models/` contiene entidades; `Data/` reúne el contexto, migraciones y datos iniciales; `Services/PasswordService.cs` gestiona las contraseñas. El contexto relaciona usuarios con pedidos, pedidos con detalles y productos con detalles y promociones. El correo de usuario tiene un índice único.

## API: rutas principales

| Recurso | Operaciones disponibles |
|---|---|
| `/api/Productos` | GET, POST, PUT y DELETE; GET por id |
| `/api/Promociones` | CRUD; consultas de activas y por producto |
| `/api/Consejos` | CRUD y consulta de activos |
| `/api/Usuarios` | Registro, login, consulta y actualización |
| `/api/Pedidos` | Crear, consultar, cambiar estado, cancelar y eliminar |

Por ejemplo, `GET /api/Pedidos/usuario/1` consulta pedidos asociados a un usuario y `GET /api/Pedidos/1/detalles` obtiene sus detalles. Los contratos exactos están en `Contracts/` y los controladores.

## Instalación y ejecución

1. Instala el SDK de .NET 10 y prepara una instancia de SQL Server accesible.
2. Clona el repositorio y entra en `CamerinoSportsServicios/`.
3. Configura la cadena `ConnectionStrings:CamerinoSports` con variables de entorno o secretos de usuario. Por ejemplo, en PowerShell:

   ```powershell
   $env:ConnectionStrings__CamerinoSports = 'Server=localhost;Database=CamerinoSports;Integrated Security=True;TrustServerCertificate=True'
   dotnet restore
   dotnet run
   ```

   Ajusta servidor y autenticación a tu instalación. La aplicación aplica las migraciones e inicializa datos al arrancar, por lo que requiere permisos adecuados sobre la base de datos. Consulta `Properties/launchSettings.json` para los perfiles locales.

## Seguridad y alcance

El código verifica contraseñas almacenadas con PBKDF2. El inicio de sesión devuelve datos de usuario, pero la configuración actual no establece un esquema de autenticación basado en tokens o cookies ni políticas de autorización para proteger los controladores. Antes de exponer la API públicamente, incorpora autenticación, autorización por recurso, HTTPS y gestión segura de secretos; revisa también los datos iniciales.
