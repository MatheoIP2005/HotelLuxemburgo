# Guía de Deploy — HotelLuxemburgo
## Supabase (base de datos) + Render (backend)

---

## RESUMEN RÁPIDO

| Paso | Qué hacés | Tiempo estimado |
|------|-----------|-----------------|
| 1 | Subir código a GitHub | 5 min |
| 2 | Crear proyecto en Supabase y ejecutar scripts SQL | 15 min |
| 3 | Desplegar 7 servicios en Render | 30 min |
| 4 | Conectar todo con variables de entorno | 10 min |

> ⚠️ **Advertencia de cold start:** Render free duerme servicios después de 15 min sin tráfico.
> El primer request después de inactividad tarda ~30 segundos. Para demos, visitá cada URL antes de la presentación.

---

## PASO 1 — Subir el código a GitHub

El código ya está listo con todos los cambios para producción. Subilo a GitHub:

```bash
# En la carpeta HotelLuxemburgo/
git init
git add .
git commit -m "chore: configuración para deploy en Render"
git branch -M main
git remote add origin https://github.com/TU_USUARIO/HotelLuxemburgo.git
git push -u origin main
```

---

## PASO 2 — Configurar Supabase (base de datos)

### 2.1 Crear proyecto

1. Entrá a https://supabase.com y creá una cuenta (gratuita)
2. Clic en **"New project"**
3. Nombralo `hotelluxemburgo`
4. Elegí una región cercana (ej. US East)
5. Poné una contraseña fuerte y **guardala** — la necesitás después
6. Esperá ~2 minutos mientras aprovisiona

### 2.2 Obtener el connection string

1. En el panel del proyecto, andá a **Settings → Database**
2. Buscá la sección **"Connection string"** → tab **"URI"**
3. Copiá el string. Se va a ver así:
   ```
   postgresql://postgres:[TU-PASSWORD]@db.XXXX.supabase.co:5432/postgres
   ```
4. Para Npgsql (.NET) necesitás este formato:
   ```
   Host=db.XXXX.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=TU-PASSWORD;SSL Mode=Require;Trust Server Certificate=true
   ```
   Guardalo — lo usarás para cada servicio (solo cambia el `Search Path` al final).

### 2.3 Ejecutar scripts SQL (Auth y Accommodation)

Ve a **SQL Editor** en el panel de Supabase y ejecutá estos archivos en orden:

**Script 1:** Contenido de `db-scripts/01_HotelLux_Auth.sql`
**Script 2:** Contenido de `db-scripts/02_HotelLux_Accommodation.sql`

Copiá el contenido de cada archivo y pegálo en el SQL Editor → **Run**.

### 2.4 Fix de la tabla Reservation (parches aplicados en testing)

En el SQL Editor ejecutá esto también:

```sql
-- Estos cambios se aplicaron durante las pruebas y deben estar en producción
ALTER TABLE reservas.reserva
    ADD COLUMN IF NOT EXISTS creado_desde_ip VARCHAR(45) NULL;

ALTER TABLE reservas.reserva
    ALTER COLUMN fecha_inicio TYPE DATE USING fecha_inicio::DATE,
    ALTER COLUMN fecha_fin    TYPE DATE USING fecha_fin::DATE;

ALTER TABLE reservas.reserva_habitacion
    ALTER COLUMN fecha_inicio TYPE DATE USING fecha_inicio::DATE,
    ALTER COLUMN fecha_fin    TYPE DATE USING fecha_fin::DATE;
```

### 2.5 Ejecutar migraciones EF Core (Reservation, Finance, Audit, Stay)

Desde tu PC, con el proyecto abierto, ejecutá estos comandos apuntando a Supabase.
Reemplazá `CONNECTION_STRING_SUPABASE` con tu string del paso 2.2.

```bash
# Reservation (schema: reservas)
dotnet ef database update \
  --project HotelLux.Reservation/HotelLux.Reservation.DataAccess \
  --startup-project HotelLux.Reservation/HotelLux.Reservation.API \
  -- --connectionstrings:ReservationDb="Host=db.XXXX.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=TU-PASSWORD;SSL Mode=Require;Trust Server Certificate=true;Search Path=reservas"

# Finance (schema: finanzas)
dotnet ef database update \
  --project HotelLux.Finance/HotelLux.Finance.DataAccess \
  --startup-project HotelLux.Finance/HotelLux.Finance.API \
  -- --connectionstrings:FinanceDb="Host=db.XXXX.supabase.co;...;Search Path=finanzas"

# Audit (schema: auditoria)
dotnet ef database update \
  --project HotelLux.Audit/HotelLux.Audit.DataAccess \
  --startup-project HotelLux.Audit/HotelLux.Audit.API \
  -- --connectionstrings:AuditDb="Host=db.XXXX.supabase.co;...;Search Path=auditoria"

# Stay (schema: hospedaje)
dotnet ef database update \
  --project HotelLux.Stay/HotelLux.Stay.DataAccess \
  --startup-project HotelLux.Stay/HotelLux.Stay.API \
  -- --connectionstrings:StayDb="Host=db.XXXX.supabase.co;...;Search Path=hospedaje"
```

---

## PASO 3 — Desplegar servicios en Render

### 3.1 Crear cuenta en Render

Entrá a https://render.com → Sign up con GitHub (así conecta el repo automáticamente).

### 3.2 Crear los 7 servicios web

Para cada servicio, el proceso es el mismo:

1. Dashboard → **"New +"** → **"Web Service"**
2. Conectá tu repositorio de GitHub
3. Configurá estos campos:

| Campo | Valor |
|-------|-------|
| **Name** | Ver tabla abajo |
| **Region** | Oregon (US West) — o la más cercana a Supabase |
| **Branch** | main |
| **Root Directory** | *(vacío — raíz del repo)* |
| **Runtime** | Docker |
| **Dockerfile Path** | Ver tabla abajo |
| **Instance Type** | Free |

**Nombres y Dockerfiles:**

| Servicio | Name en Render | Dockerfile Path |
|----------|---------------|-----------------|
| Audit | `hotellux-audit` | `HotelLux.Audit/Dockerfile` |
| Auth | `hotellux-auth` | `HotelLux.Auth/Dockerfile` |
| Accommodation | `hotellux-accommodation` | `HotelLux.Accommodation/Dockerfile` |
| Finance | `hotellux-finance` | `HotelLux.Finance/Dockerfile` |
| Reservation | `hotellux-reservation` | `HotelLux.Reservation/Dockerfile` |
| Stay | `hotellux-stay` | `HotelLux.Stay/Dockerfile` |
| Gateway | `hotellux-gateway` | `HotelLux.Gateway/Dockerfile` |

> ℹ️ Render asigna URLs del tipo `https://hotellux-audit.onrender.com`
> Si el nombre ya existe, agrega un sufijo (ej. `hotellux-audit-abc1`). Anotá la URL real de cada servicio.

### 3.3 Dejar que todos hagan el primer build

Creá los 7 servicios pero **no les pongas variables de entorno todavía** — que Render compile primero.
El build de .NET tarda ~5-10 min la primera vez. El servicio va a quedar en error por falta de config — eso es normal.

---

## PASO 4 — Variables de entorno en Render

Una vez que tengas las URLs reales de cada servicio, andá a cada uno → **Environment** → y agregá estas variables:

### 🟠 AUDIT (`hotellux-audit`)
```
ConnectionStrings__AuditDb   = Host=db.XXXX.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=TU-PASS;SSL Mode=Require;Trust Server Certificate=true;Search Path=auditoria
Jwt__Key                     = HotelLuxemburgo_AccessSecret_MinimoTrentaYDosCaracteres_2026
```

### 🔵 AUTH (`hotellux-auth`)
```
ConnectionStrings__AuthDb              = Host=db.XXXX.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=TU-PASS;SSL Mode=Require;Trust Server Certificate=true
JwtSettings__JwtSecret                 = HotelLuxemburgo_AccessSecret_MinimoTrentaYDosCaracteres_2026
JwtSettings__JwtRefreshSecret          = HotelLuxemburgo_RefreshSecret_MinimoTrentaYDosCaracteres_2026
JwtSettings__Issuer                    = HotelLuxemburgo.Auth
JwtSettings__Audience                  = HotelLuxemburgo.Services
JwtSettings__JwtExpiresIn              = 3600
JwtSettings__JwtRefreshExpiresIn       = 604800
AuditService__GrpcAddress              = https://hotellux-audit.onrender.com
```

### 🟢 ACCOMMODATION (`hotellux-accommodation`)
```
ConnectionStrings__AccommodationDb   = Host=db.XXXX.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=TU-PASS;SSL Mode=Require;Trust Server Certificate=true;Search Path=accommodation
Jwt__Key                             = HotelLuxemburgo_AccessSecret_MinimoTrentaYDosCaracteres_2026
AuditService__GrpcAddress            = https://hotellux-audit.onrender.com
StayService__GrpcAddress             = https://hotellux-stay.onrender.com
```

### 🟡 FINANCE (`hotellux-finance`)
```
ConnectionStrings__FinanceDb   = Host=db.XXXX.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=TU-PASS;SSL Mode=Require;Trust Server Certificate=true;Search Path=finanzas
Jwt__Key                       = HotelLuxemburgo_AccessSecret_MinimoTrentaYDosCaracteres_2026
AuditService__GrpcAddress      = https://hotellux-audit.onrender.com
```

### 🟣 RESERVATION (`hotellux-reservation`)
```
ConnectionStrings__ReservationDb   = Host=db.XXXX.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=TU-PASS;SSL Mode=Require;Trust Server Certificate=true;Search Path=reservas
Jwt__Key                           = HotelLuxemburgo_AccessSecret_MinimoTrentaYDosCaracteres_2026
AccommodationService__GrpcAddress  = https://hotellux-accommodation.onrender.com
FinanceService__GrpcAddress        = https://hotellux-finance.onrender.com
StayService__GrpcAddress           = https://hotellux-stay.onrender.com
AuditService__GrpcAddress          = https://hotellux-audit.onrender.com
```

### 🔴 STAY (`hotellux-stay`)
```
ConnectionStrings__StayDb          = Host=db.XXXX.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=TU-PASS;SSL Mode=Require;Trust Server Certificate=true;Search Path=hospedaje
Jwt__Key                           = HotelLuxemburgo_AccessSecret_MinimoTrentaYDosCaracteres_2026
ReservationService__GrpcAddress    = https://hotellux-reservation.onrender.com
AccommodationService__GrpcAddress  = https://hotellux-accommodation.onrender.com
FinanceService__GrpcAddress        = https://hotellux-finance.onrender.com
AuditService__GrpcAddress          = https://hotellux-audit.onrender.com
```

### ⚫ GATEWAY (`hotellux-gateway`)
```
ReverseProxy__Clusters__accommodation__Destinations__api__Address   = https://hotellux-accommodation.onrender.com/
ReverseProxy__Clusters__reservation__Destinations__api__Address     = https://hotellux-reservation.onrender.com/
ReverseProxy__Clusters__stay__Destinations__api__Address            = https://hotellux-stay.onrender.com/
ReverseProxy__Clusters__auth__Destinations__api__Address            = https://hotellux-auth.onrender.com/
ReverseProxy__Clusters__finance__Destinations__api__Address         = https://hotellux-finance.onrender.com/
ReverseProxy__Clusters__audit__Destinations__api__Address           = https://hotellux-audit.onrender.com/
```

> ⚠️ **Importante:** Reemplazá cada URL con la URL real que te dio Render si difiere del nombre esperado (si Render agregó un sufijo, usá esa URL exacta).

---

## PASO 5 — Redeploy y verificación

1. Después de agregar las vars de entorno, hacé **"Manual Deploy"** en cada servicio
2. Verificá en los logs que arranque sin errores
3. El orden de arranque importa para el primer health check. Esperá que estén listos en este orden:
   - Audit → Auth → Finance → Accommodation → Reservation → Stay → Gateway

### URL pública de tu API

Una vez todo listo, el endpoint principal es:
```
https://hotellux-gateway.onrender.com
```

### Test rápido desde el browser
```
GET https://hotellux-gateway.onrender.com/api/v1/accommodations/search?destino=Quito&fechaInicio=2026-06-01&fechaFin=2026-06-05&num_adultos=2&num_habitaciones=1
```

---

## Seed de datos

Los scripts de Auth y Accommodation ya incluyen datos de prueba (seed). Para Reservation también necesitás
ejecutar el script `db-scripts/03_HotelLux_Reservation.sql` en el SQL Editor de Supabase si querés tener
reservas y clientes de prueba precargados.

---

## Cómo funciona gRPC en producción (Render)

El reverse proxy de Render recibe las conexiones HTTPS del cliente y las reenvía al contenedor en HTTP/1.1.
gRPC nativo requiere HTTP/2 y falla con `"Request protocol 'HTTP/1.1' is not supported"` en ese escenario.

**La solución implementada en el código usa gRPC-Web** (protocolo compatible con HTTP/1.1):

- Render inyecta automáticamente la variable `RENDER=true` en todos sus servicios.
- La clase `HotelLux.Shared.Grpc.GrpcChannelFactory` detecta esa variable y crea un `GrpcWebHandler` en vez de un `SocketsHttpHandler` nativo.
- Los servidores tienen `app.UseGrpcWeb()` + `EnableGrpcWeb()` para decodificar esas peticiones.
- Kestrel en producción escucha solo en HTTP/1.1 (PORT), sin listener HTTP/2 adicional.

**No necesitás configurar nada extra en Render.** El switch es automático.

Si desplegás en otra plataforma que también use un proxy HTTP/1.1 (Railway, Fly.io, etc.), podés forzar gRPC-Web manualmente agregando la variable de entorno:
```
GRPC_USE_WEB = true
```

Para forzar gRPC nativo (h2c) aunque estés en Render, usá:
```
GRPC_USE_WEB = false
```

---

## Troubleshooting frecuente

| Error | Causa | Solución |
|-------|-------|----------|
| `SSL connection is required` | Falta SSL en connection string | Agregar `SSL Mode=Require;Trust Server Certificate=true` |
| `Request protocol 'HTTP/1.1' is not supported` | `RENDER` no está en `true` o gRPC-Web no activo | Verificar que Render inyecte `RENDER=true`; o setear `GRPC_USE_WEB=true` manualmente |
| `Connection refused` en gRPC | URL del servicio incorrecta o servicio dormido | Verificar URL en env vars; esperar que el servicio despierte |
| `Jwt:Key no configurada` | Falta variable de entorno | Agregar `Jwt__Key` en Render |
| Build falla: `protos not found` | Dockerfile con context incorrecto | El build context debe ser la raíz del repo |
| `no existe la columna creado_desde_ip` | Falta ejecutar el fix SQL de Reservation | Correr el ALTER TABLE del Paso 2.4 en Supabase |
