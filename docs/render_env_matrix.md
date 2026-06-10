# Matriz de variables — Render (HotelLuxemburgo)

Referencia operativa para desplegar los 7 servicios en Render con RabbitMQ externo (CloudAMQP u otro broker gestionado).

**No usar `localhost` para RabbitMQ en Render.** Docker local es solo para desarrollo/pruebas.

## RabbitMQ (todos los publishers + Audit)

Elegir **una** opción:

### Opción A — URL completa (recomendada)

```text
CLOUDAMQP_URL=amqps://<usuario>:<password>@<host>:5671/<vhost>
RabbitMq__AuditQueue=hotellux.audit.events
```

o:

```text
RabbitMq__Uri=amqps://<usuario>:<password>@<host>:5671/<vhost>
RabbitMq__AuditQueue=hotellux.audit.events
```

`CLOUDAMQP_URL` se usa solo si `RabbitMq__Uri` está vacío.

### Opción B — variables separadas

```text
RabbitMq__Host=<host>
RabbitMq__Port=5671
RabbitMq__VirtualHost=<vhost>
RabbitMq__Username=<usuario>
RabbitMq__Password=<password>
RabbitMq__UseSsl=true
RabbitMq__AuditQueue=hotellux.audit.events
```

- `amqp` → puerto 5672, sin TLS.
- `amqps` → puerto 5671, con TLS.

---

## Por servicio

| Servicio Render | Dockerfile | RabbitMQ | Health post-deploy |
|---|---|---|---|
| `hotellux-audit` | `HotelLux.Audit/Dockerfile` | Sí (consumer) | `/health`, `/health/live`, `/health/ready` |
| `hotellux-auth` | `HotelLux.Auth/Dockerfile` | Sí (publisher) | idem |
| `hotellux-accommodation` | `HotelLux.Accommodation/Dockerfile` | Sí | idem |
| `hotellux-reservation` | `HotelLux.Reservation/Dockerfile` | Sí | idem |
| `hotellux-stay` | `HotelLux.Stay/Dockerfile` | Sí | idem |
| `hotellux-finance` | `HotelLux.Finance/Dockerfile` | Sí | idem |
| `hotellux-gateway` | `HotelLux.Gateway/Dockerfile` | **No** | `/health`, `/health/live`, `/health/ready`, `/graphql`, `/swagger/v1/swagger.json` |

---

### `hotellux-audit`

| Tipo | Variable |
|---|---|
| **Obligatorias** | `ConnectionStrings__AuditDb`, `Jwt__Key`, RabbitMQ (Opción A o B), `RabbitMq__AuditQueue` |
| **Opcionales** | — |
| **gRPC saliente** | No (consumer RabbitMQ; `AuditGrpcService` legacy en servidor) |
| **Readiness** | `/health/ready` debe ser **200** si RabbitMQ y BD están OK |

---

### `hotellux-auth`

| Tipo | Variable |
|---|---|
| **Obligatorias** | `ConnectionStrings__AuthDb`, `JwtSettings__JwtSecret`, `JwtSettings__JwtRefreshSecret`, `JwtSettings__Issuer`, `JwtSettings__Audience`, RabbitMQ, `RabbitMq__AuditQueue` |
| **Opcionales** | `JwtSettings__JwtExpiresIn`, `JwtSettings__JwtRefreshExpiresIn` |
| **gRPC saliente** | No requerido para deploy mínimo |
| **Readiness** | `/health/ready` incluye MassTransit/RabbitMQ |

---

### `hotellux-accommodation`

| Tipo | Variable |
|---|---|
| **Obligatorias** | `ConnectionStrings__AccommodationDb`, `Jwt__Key`, RabbitMQ, `RabbitMq__AuditQueue` |
| **Opcionales** | `StayService__GrpcAddress` (URL pública Render del Stay, gRPC-Web) |
| **gRPC saliente** | Stay (si se usa estadías desde alojamiento) |
| **Readiness** | `/health/ready` incluye RabbitMQ |

---

### `hotellux-reservation`

| Tipo | Variable |
|---|---|
| **Obligatorias** | `ConnectionStrings__ReservationDb`, `Jwt__Key`, RabbitMQ, `RabbitMq__AuditQueue` |
| **Opcionales** | `AccommodationService__GrpcAddress`, `FinanceService__GrpcAddress`, `StayService__GrpcAddress` |
| **gRPC saliente** | Accommodation, Finance, Stay (saga reserva; URLs HTTPS Render) |
| **Readiness** | `/health/ready` incluye RabbitMQ |

---

### `hotellux-stay`

| Tipo | Variable |
|---|---|
| **Obligatorias** | `ConnectionStrings__StayDb`, `Jwt__Key`, RabbitMQ, `RabbitMq__AuditQueue` |
| **Opcionales** | `ReservationService__GrpcAddress`, `AccommodationService__GrpcAddress`, `FinanceService__GrpcAddress` |
| **gRPC saliente** | Reservation, Accommodation, Finance |
| **Readiness** | `/health/ready` incluye RabbitMQ |

---

### `hotellux-finance`

| Tipo | Variable |
|---|---|
| **Obligatorias** | `ConnectionStrings__FinanceDb`, `Jwt__Key`, RabbitMQ, `RabbitMq__AuditQueue` |
| **Opcionales** | — |
| **gRPC saliente** | No obligatorio para smoke inicial |
| **Readiness** | `/health/ready` incluye RabbitMQ |

---

### `hotellux-gateway`

| Tipo | Variable |
|---|---|
| **Obligatorias** | `ReverseProxy__Clusters__*__Destinations__api__Address` (6 clusters: accommodation, reservation, stay, auth, finance, audit) |
| **Opcionales** | — |
| **RabbitMQ** | **No** |
| **Post-deploy** | `/health`, `/health/live`, `/health/ready`, `POST /graphql` `{ __typename }`, `/swagger/v1/swagger.json` |

Ejemplo cluster (reemplazar URLs reales de Render):

```text
ReverseProxy__Clusters__accommodation__Destinations__api__Address=https://hotellux-accommodation.onrender.com/
ReverseProxy__Clusters__reservation__Destinations__api__Address=https://hotellux-reservation.onrender.com/
ReverseProxy__Clusters__stay__Destinations__api__Address=https://hotellux-stay.onrender.com/
ReverseProxy__Clusters__auth__Destinations__api__Address=https://hotellux-auth.onrender.com/
ReverseProxy__Clusters__finance__Destinations__api__Address=https://hotellux-finance.onrender.com/
ReverseProxy__Clusters__audit__Destinations__api__Address=https://hotellux-audit.onrender.com/
```

---

## Smoke remoto post-deploy

Desde la raíz del repo (ajustar URLs a las de Render):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Test-RenderSmoke.ps1 `
  -GatewayBaseUrl "https://hotellux-gateway.onrender.com" `
  -AuditBaseUrl "https://hotellux-audit.onrender.com" `
  -AuthBaseUrl "https://hotellux-auth.onrender.com" `
  -AccommodationBaseUrl "https://hotellux-accommodation.onrender.com" `
  -ReservationBaseUrl "https://hotellux-reservation.onrender.com" `
  -StayBaseUrl "https://hotellux-stay.onrender.com" `
  -FinanceBaseUrl "https://hotellux-finance.onrender.com" `
  -RequireServiceReady
```

En planes **free** de Render puede haber cold start (~30 s). Si el smoke falla por timeout, esperar y repetir.

Solo Gateway (prueba local o mínima):

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Test-RenderSmoke.ps1 -GatewayBaseUrl "http://127.0.0.1:5000"
```

---

## Orden sugerido de Manual Deploy

1. Broker RabbitMQ externo listo (CloudAMQP).
2. Audit → Auth → Finance → Accommodation → Reservation → Stay → Gateway.
3. `Test-RenderSmoke.ps1` con `-RequireServiceReady`.
4. Login u operación que genere auditoría (fuera del smoke script; requiere credenciales).
