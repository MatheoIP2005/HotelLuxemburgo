# POLISH FINAL — HotelLuxemburgo
## Instrucción a Cursor: aplica TODOS los cambios directamente en el código. NO generes prompts.

---

## ESTADO DEL PROYECTO

El proyecto está **funcionalmente completo** en su mayoría. Los cambios a continuación son los únicos puntos pendientes encontrados en la auditoría de código.

---

## CORRECCIÓN 1 — `HotelLux.Audit` no tiene ExceptionMiddleware (ALTA)

**Problema:** `HotelLux.Audit.API` no tiene carpeta `Middleware/` ni `ExceptionMiddleware`. Cualquier excepción no controlada en `AuditoriaController` o `AuditGrpcService` devuelve un stack trace crudo en lugar de un JSON estructurado.

**Crear el archivo:**
`HotelLux.Audit/HotelLux.Audit.API/Middleware/ExceptionMiddleware.cs`

```csharp
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HotelLux.Audit.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en Audit API");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var body = JsonSerializer.Serialize(new
            {
                status = 500,
                error = "Error interno del servidor.",
                timestamp = DateTime.UtcNow
            });
            await context.Response.WriteAsync(body);
        }
    }
}
```

**Agregar en `HotelLux.Audit/HotelLux.Audit.API/Program.cs`** justo antes de `if (app.Environment.IsDevelopment())`:

```csharp
app.UseMiddleware<HotelLux.Audit.API.Middleware.ExceptionMiddleware>();
```

---

## CORRECCIÓN 2 — `PermisoService` lanza `NotImplementedException` (MEDIA)

**Problema:** `HotelLux.Auth.Business.Services.PermisoService.ObtenerPermisosAsync` lanza `throw new NotImplementedException("Pendiente: permisos.")`. Aunque `PermisosController` no llama al servicio (devuelve 501 directamente), el servicio está registrado en DI. Si algún otro componente lo inyecta en el futuro fallará en runtime.

**Archivo:** `HotelLux.Auth/HotelLux.Auth.Business/Services/PermisoService.cs`

**Reemplazar con:**

```csharp
using HotelLux.Auth.Business.Interfaces;

namespace HotelLux.Auth.Business.Services;

public class PermisoService : IPermisoService
{
    // Los permisos no están implementados aún.
    // Retorna lista vacía para no bloquear arranque ni DI.
    public Task<IReadOnlyList<string>> ObtenerPermisosAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
}
```

---

## CORRECCIÓN 3 — `PermisosController` inyecta `IPermisoService` sin usarlo (BAJA)

El controlador actualmente no recibe dependencias. No hay acción requerida salvo verificar que no hay ninguna inyección pendiente en el constructor que compile pero no use. Ya está correcto.

---

## COMANDOS PARA LEVANTAR LAS BASES DE DATOS

### Auth y Accommodation — scripts SQL manuales (NO usan EF Core migrations)

```bash
# En psql o pgAdmin (ejecutar en orden):
\i db-scripts/01_HotelLux_Auth.sql
\i db-scripts/02_HotelLux_Accommodation.sql
```

### Reservation, Finance, Audit — EF Core migrations

```bash
# Reservation
cd HotelLux.Reservation/HotelLux.Reservation.DataAccess
dotnet ef database update --startup-project ../HotelLux.Reservation.API

# Finance
cd HotelLux.Finance/HotelLux.Finance.DataAccess
dotnet ef database update --startup-project ../HotelLux.Finance.API

# Audit
cd HotelLux.Audit/HotelLux.Audit.DataAccess
dotnet ef database update --startup-project ../HotelLux.Audit.API

# Stay (2 migrations: InitialHospedaje + AddNombreVisibleClienteToValoracion)
cd HotelLux.Stay/HotelLux.Stay.DataAccess
dotnet ef database update --startup-project ../HotelLux.Stay.API
```

---

## COMPILACIÓN FINAL

Después de aplicar los cambios, compilar todos los proyectos:

```bash
dotnet build HotelLux.Audit/HotelLux.Audit.API/HotelLux.Audit.API.csproj
dotnet build HotelLux.Auth/HotelLux.Auth.API/HotelLux.Auth.API.csproj
dotnet build HotelLux.Accommodation/HotelLux.Accommodation.API/HotelLux.Accommodation.API.csproj
dotnet build HotelLux.Finance/HotelLux.Finance.API/HotelLux.Finance.API.csproj
dotnet build HotelLux.Reservation/HotelLux.Reservation.API/HotelLux.Reservation.API.csproj
dotnet build HotelLux.Stay/HotelLux.Stay.API/HotelLux.Stay.API.csproj
dotnet build HotelLux.Gateway/HotelLux.Gateway.csproj
```

**Si algún proyecto falla al compilar, corrígelo antes de continuar.**

---

## ORDEN DE INICIO DE LOS MICROSERVICIOS

```
1. HotelLux.Audit    → puerto HTTP 5008, gRPC 5108  (sin dependencias)
2. HotelLux.Auth     → puerto HTTP 5001, gRPC 5101  (depende: Audit)
3. HotelLux.Accommodation → puerto HTTP 5002, gRPC 5102  (depende: Audit)
4. HotelLux.Finance  → puerto HTTP 5005, gRPC 5105  (depende: Audit)
5. HotelLux.Reservation → puerto HTTP 5003, gRPC 5103 (depende: Audit, Accommodation, Finance, Stay)
6. HotelLux.Stay     → puerto HTTP 5004, gRPC 5104  (depende: Audit, Accommodation, Reservation, Finance)
7. HotelLux.Gateway  → puerto HTTP 5000             (depende: todos)
```

---

## RESUMEN DE LO QUE FALTABA (tabla)

| # | Archivo | Problema | Prioridad |
|---|---------|----------|-----------|
| 1 | `HotelLux.Audit.API/Middleware/ExceptionMiddleware.cs` | No existe — crear y registrar en Program.cs | ALTA |
| 2 | `HotelLux.Auth.Business/Services/PermisoService.cs` | `throw new NotImplementedException` — reemplazar con return vacío | MEDIA |
| 3 | DB: Stay | Ejecutar `dotnet ef database update` (2 migrations pendientes) | MEDIA |
| 4 | DB: Reservation, Finance, Audit | Verificar/ejecutar `dotnet ef database update` | MEDIA |
| 5 | DB: Auth, Accommodation | Ejecutar scripts `01_*.sql` y `02_*.sql` en PostgreSQL | MEDIA |

---

## LO QUE ESTÁ CORRECTO (no tocar)

✅ Todos los 7 `.proto` implementados al 100%  
✅ Todos los gRPC services completos (Accommodation: 5 métodos, Stay: 5, Reservation: 2, Finance: 4, Auth: 2, Audit: 1)  
✅ Todos los controladores REST completos  
✅ DTOs correctos (`ReservaDTO.Habitaciones`, `ValoracionDto.NombreVisibleCliente`)  
✅ `AccommodationsController` con response shapes camelCase correctos  
✅ `AccommodationPublicReservasController.ToPublicReserva` en camelCase con `habitaciones[]`  
✅ `StayPublicGrpcClient` mapea `NombreVisibleCliente`  
✅ `EstadiasController` completo (5 endpoints: Listar, CheckIn, CheckInPorReserva, CheckOut, CheckOutPorBody, ObtenerPorGuid, MarcarMantenimiento)  
✅ `CargoEstadiaService` completo con `ObtenerPorGuidAsync` y `AnularAsync`  
✅ `CargosEstadiaController` completo  
✅ Saga de confirmación de reserva con compensación implementada  
✅ `ClienteInlineDTO` con búsqueda/creación de cliente al vuelo  
✅ JWT con clave compartida consistente en los 6 microservicios  
✅ Gateway YARP con rutas para todos los endpoints (32 rutas)  
✅ `IMemoryCache` registrado en Accommodation  
✅ `PasswordSeeder` en Auth para hash inicial  
