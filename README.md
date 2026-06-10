# Hotel Luxemburgo — Microservicios

Migración del monolito `ServicioHotel` (.NET 8 + SQL Server) a una arquitectura
de **6 microservicios** + **API Gateway** (.NET 8 + PostgreSQL 18).

Materia: **Integración de Sistemas** — PUCE 2026
Docente: Damián Nicolalde

---

## Arquitectura

| Microservicio              | BD                       | Puerto REST | Puerto gRPC | Responsabilidad |
|----------------------------|--------------------------|-------------|-------------|-----------------|
| `HotelLux.Auth`            | `HotelLux_Auth`          | 5001        | 5101        | Usuarios, roles, JWT, validación de tokens |
| `HotelLux.Accommodation`   | `HotelLux_Accommodation` | 5002        | 5102        | Sucursales, tipos de habitación, habitaciones, tarifas, catálogo |
| `HotelLux.Reservation`     | `HotelLux_Reservation`   | 5003        | 5103        | Clientes, reservas, saga de confirmación |
| `HotelLux.Stay`            | `HotelLux_Stay`          | 5004        | 5104        | Check-in/out, cargos de estadía, valoraciones (fusión stay+rating) |
| `HotelLux.Finance`         | `HotelLux_Finance`       | 5005        | 5105        | Facturas, detalle, pagos (fusión billing+payment) |
| `HotelLux.Audit`           | `HotelLux_Audit`         | 5008        | 5108        | Consumer RabbitMQ de eventos de auditoría; gRPC legacy temporal |
| `HotelLux.Gateway`         | —                        | 5000        | —           | YARP + Swagger + GraphQL (`/graphql`) |

---

## Estructura del repositorio

```
HotelLuxemburgo/
├── db-scripts/                    # 6 scripts SQL para PostgreSQL 18
│   ├── 01_HotelLux_Auth.sql
│   ├── 02_HotelLux_Accommodation.sql
│   ├── 03_HotelLux_Reservation.sql
│   ├── 04_HotelLux_Stay.sql
│   ├── 05_HotelLux_Finance.sql
│   └── 06_HotelLux_Audit.sql
│
├── protos/                        # Contratos gRPC compartidos
│   ├── common.proto
│   ├── auth.proto
│   ├── accommodation.proto
│   ├── reservation.proto
│   ├── stay.proto
│   ├── finance.proto
│   └── audit.proto
│
├── HotelLux.Auth/                 # Microservicio Auth (5 capas + .sln)
├── HotelLux.Accommodation/
├── HotelLux.Reservation/
├── HotelLux.Stay/
├── HotelLux.Finance/
├── HotelLux.Audit/
├── HotelLux.Gateway/              # YARP
│
├── .gitignore
└── README.md
```

Cada microservicio sigue el mismo patrón de capas heredado del monolito:

```
HotelLux.<Servicio>/
├── HotelLux.<Servicio>.API/           # Web API REST + gRPC server
├── HotelLux.<Servicio>.Business/      # Services, DTOs, validators, mappers, interfaces
├── HotelLux.<Servicio>.DataManagement/ # UoW, data services, data models
├── HotelLux.<Servicio>.DataAccess/    # EF Core, DbContext, repositorios
└── HotelLux.<Servicio>.sln
```

---

## Comunicación entre servicios

- **REST público** (hacia frontend y booking externo): vía Gateway YARP.
- **GraphQL** (Gateway): endpoint `/graphql` sobre los mismos servicios internos.
- **gRPC interno** (entre microservicios): comunicación directa de negocio.
- **RabbitMQ** (bus de eventos): auditoría vía `AuditEventMessage` → `HotelLux.Audit.API` → PostgreSQL.

### RabbitMQ: local vs producción

| Ambiente | Broker |
|---|---|
| Local / pruebas | Docker Desktop + `docker-compose.rabbitmq.yml` (`localhost`) |
| Render / producción | Broker RabbitMQ externo gestionado (ej. CloudAMQP). Configurar `RabbitMq__Uri`, `CLOUDAMQP_URL` o variables `Host`/`Port`/`UseSsl`. **No usar `localhost`.** |

Ver `docs/rabbitmq_event_bus.md` y `DEPLOY.md` para variables `RabbitMq__...`.

### Flujo de validación de tokens

Todo endpoint con prefijo `/api/v1/internal/**` requiere JWT. Antes de ejecutar la lógica,
el microservicio llama a `auth.ValidateToken` por gRPC. Caché en memoria opcional.

### Saga de confirmación de reserva

`POST /accomodations/reservas` desde el booking dispara:

1. `accommodation.CheckAvailability`
2. `accommodation.ConfirmRoomLock` (por cada habitación)
3. `UPDATE reserva SET estado='CON'` (local)
4. `finance.GenerateReservationInvoice`
5. Publicación `AuditEventMessage` hacia RabbitMQ (fire-and-forget)

Compensación si algún paso falla: `accommodation.ReleaseRoomLock` + `UPDATE reserva = 'CAN'`.

---

## Cómo levantar el entorno local

### 1. Bases de datos (PostgreSQL 18)

Ejecutar los 6 scripts de `db-scripts/` en orden desde pgAdmin. Cada script crea su BD,
schema, tablas, índices y datos semilla coherentes (mismos GUIDs entre BDs).

### 2. RabbitMQ (Docker Desktop)

```powershell
docker compose -f docker-compose.rabbitmq.yml up -d
# o: powershell -ExecutionPolicy Bypass -File scripts/Start-RabbitMq.ps1
```

Requiere **Docker Desktop corriendo**. Sin RabbitMQ, los servicios arrancan pero `/health/ready` devuelve 503.

### 3. Microservicios (stack completo)

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Start-HotelLuxStack.ps1
```

Levanta Audit, Auth, Finance, Accommodation, Reservation, Stay y Gateway (puertos 5000–5008).
Opciones: `-SkipRabbitMq`, `-RequireRabbitMq`.

### 4. Verificación local del backend

Flujo estándar (build + tests Shared, Reservation y Finance):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Test-Backend.ps1
```

Con Gateway ya levantado en `:5000`, incluir smoke de health/GraphQL:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Test-Backend.ps1 -GatewaySmoke
```

`HotelLux.Shared.Tests` cubre el parsing de configuración RabbitMQ (`RabbitMq__Uri`, `CLOUDAMQP_URL`, puerto y TLS).

---

## GraphQL

El Gateway expone GraphQL en `/graphql` además de REST/YARP y Swagger.

```powershell
$body = @{ query = "{ __typename }" } | ConvertTo-Json
Invoke-WebRequest -Uri "http://127.0.0.1:5000/graphql" -Method POST -ContentType "application/json" -Body $body
```

GraphQL reenvía `Authorization` hacia servicios internos según la implementación actual del Gateway.

---

## Health checks

| Endpoint | Propósito |
|---|---|
| `/health` | Liveness (compatible hacia atrás) |
| `/health/live` | Liveness |
| `/health/ready` | Readiness; incluye RabbitMQ en servicios que usan MassTransit |

Con RabbitMQ apagado: `/health` y `/health/live` → 200; `/health/ready` → 503.

---

## Decisiones de diseño clave

- **PostgreSQL 18** local, despliegue final en Supabase.
- **Docker** se usa localmente para RabbitMQ de pruebas; en Render los microservicios usan un broker externo.
- **JWT HS256** con llave simétrica compartida entre los 6 servicios.
- **Database-per-Service**: FKs cross-BD se reemplazan por GUIDs lógicos sin FK.
- **Stored procedures del monolito** (`SP_GENERAR_FACTURA_*`, `SP_HACER_CHECKIN`, etc.)
  se migran a la capa Business del microservicio dueño. Lo que cruza dominios se
  reemplaza por orquestación gRPC.
- **Triggers de auditoría**: eliminados. Cada servicio publica `AuditEventMessage` a RabbitMQ.
  `HotelLux.Audit.API` consume la cola y persiste en `auditoria.evento_auditoria`.
