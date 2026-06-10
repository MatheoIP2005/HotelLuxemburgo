# RabbitMQ y bus de eventos de auditoría

Este documento describe el bus de auditoría en HotelLuxemburgo: desarrollo local con Docker y producción en Render con un broker externo.

## Estrategia de ambientes

| Ambiente | Broker RabbitMQ |
|---|---|
| **Local / pruebas** | Docker Desktop + `docker-compose.rabbitmq.yml` (`localhost:5672`) |
| **Producción (Render)** | Broker RabbitMQ externo o gestionado (por ejemplo CloudAMQP). **No usar `localhost`.** |

Los microservicios en Render no ejecutan Docker local. Solo necesitan variables `RabbitMq__...` apuntando al host real del broker.

---

## Local / testing con Docker

**Requisito:** Docker Desktop debe estar corriendo antes de levantar RabbitMQ.

Desde la raíz del repositorio:

```powershell
docker compose -f docker-compose.rabbitmq.yml up -d
```

O usando el script opcional:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Start-RabbitMq.ps1
```

Verificar que la consola de administración responde:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Test-RabbitMq.ps1
```

- AMQP: `localhost:5672`
- Management UI: http://localhost:15672
- Usuario/contraseña por defecto: `guest` / `guest`

### Variables locales esperadas

En `appsettings.json` de cada publisher/consumer (sobrescribibles por entorno):

```text
RabbitMq__Host=localhost
RabbitMq__Port=5672
RabbitMq__VirtualHost=/
RabbitMq__Username=guest
RabbitMq__Password=guest
RabbitMq__UseSsl=false
RabbitMq__AuditQueue=hotellux.audit.events
```

### Levantar el stack completo

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Start-HotelLuxStack.ps1
```

Parámetros útiles:

- `-SkipRabbitMq`: no intenta Docker; `/health/ready` puede devolver 503.
- `-RequireRabbitMq`: falla si Docker/RabbitMQ no están disponibles.

---

## Producción en Render

1. Crear un broker RabbitMQ gestionado (recomendado: **CloudAMQP** u otro proveedor AMQP compatible).
2. Configurar en **cada** servicio publisher y en Audit (elegir una opción):

**Opción A — URL completa (recomendada):**

```text
RabbitMq__Uri=amqps://<usuario>:<password>@<host>:5671/<vhost>
RabbitMq__AuditQueue=hotellux.audit.events
```

O variable del proveedor:

```text
CLOUDAMQP_URL=amqps://<usuario>:<password>@<host>:5671/<vhost>
RabbitMq__AuditQueue=hotellux.audit.events
```

**Opción B — variables separadas:**

```text
RabbitMq__Host=<host>
RabbitMq__Port=5671
RabbitMq__VirtualHost=<vhost>
RabbitMq__Username=<usuario>
RabbitMq__Password=<password>
RabbitMq__UseSsl=true
RabbitMq__AuditQueue=hotellux.audit.events
```

- `amqp://` → puerto 5672, sin TLS.
- `amqps://` → puerto 5671, con TLS.

3. **No** usar `localhost` ni Docker Desktop en Render.
4. Validar con `/health/ready` en Audit, Accommodation, Reservation, Auth, Stay y Finance.

`HotelLux.Gateway` **no** requiere `RabbitMq__...`.

---

## Configuración en microservicios

Cada API publisher/consumer incluye la sección `RabbitMq` en `appsettings.json`:

```json
"RabbitMq": {
  "Host": "localhost",
  "Port": 5672,
  "VirtualHost": "/",
  "Username": "guest",
  "Password": "guest",
  "UseSsl": false,
  "AuditQueue": "hotellux.audit.events"
}
```

### Variables de entorno (.NET)

| Variable | Equivalente en appsettings |
|---|---|
| `RabbitMq__Uri` | `RabbitMq:Uri` (URL `amqp://` o `amqps://`) |
| `CLOUDAMQP_URL` | Fallback si `RabbitMq:Uri` está vacío |
| `RabbitMq__Host` | `RabbitMq:Host` |
| `RabbitMq__Port` | `RabbitMq:Port` |
| `RabbitMq__VirtualHost` | `RabbitMq:VirtualHost` |
| `RabbitMq__Username` | `RabbitMq:Username` |
| `RabbitMq__Password` | `RabbitMq:Password` |
| `RabbitMq__UseSsl` | `RabbitMq:UseSsl` |
| `RabbitMq__AuditQueue` | `RabbitMq:AuditQueue` |

---

## Flujo de auditoría

```
Publisher (Auth, Accommodation, Reservation, Stay, Finance)
    -> publica AuditEventMessage (MassTransit)
    -> RabbitMQ (cola hotellux.audit.events)
    -> HotelLux.Audit.API (AuditEventConsumer)
    -> PostgreSQL (auditoria.evento_auditoria)
```

Los servicios publishers usan `AuditRabbitMqEmitter` / `AuditRabbitMqClient`.
`AuditGrpcService` permanece en el código como **compatibilidad temporal**; el camino principal es RabbitMQ.

---

## Verificación rápida del backend

Antes o después de cambios en RabbitMQ/MassTransit, ejecutar el flujo estándar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Test-Backend.ps1
```

Incluye build del stack y `HotelLux.Shared.Tests` (parsing de `RabbitMq__Uri`, `CLOUDAMQP_URL`, puerto y TLS), más tests de Reservation y Finance.

Con Gateway levantado:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/Test-Backend.ps1 -GatewaySmoke
```

---

## Cómo probar manualmente (local)

1. Levantar RabbitMQ (Docker Desktop activo).
2. Levantar `HotelLux.Audit.API`.
3. Levantar un publisher, por ejemplo `HotelLux.Accommodation.API`.
4. Ejecutar una operación que emita auditoría (login, crear/actualizar recurso).
5. En RabbitMQ Management, confirmar actividad en la cola `hotellux.audit.events`.
6. En PostgreSQL, consultar `auditoria.evento_auditoria`.

---

## Health checks (liveness vs readiness)

MassTransit registra un health check del bus RabbitMQ:

| Endpoint | Propósito |
|---|---|
| `/health` | Liveness: responde 200 si la app está viva, aunque RabbitMQ esté caído. |
| `/health/live` | Igual que liveness. |
| `/health/ready` | Readiness: incluye RabbitMQ; responde 503 si el broker no está disponible. |

Comportamiento esperado:

- **RabbitMQ apagado**: `/health` y `/health/live` → 200; `/health/ready` → 503.
- **RabbitMQ encendido**: los tres endpoints → 200.

Para smoke tests usar `/health` o `/health/live`. Para validar el bus usar `/health/ready`.

---

## Diagnóstico si no se insertan eventos

1. **RabbitMQ no está levantado** (local): publisher registra warning; Audit no consume; `/health/ready` → 503.
2. **Docker Desktop apagado** (local): no se puede levantar el broker; usar `Start-HotelLuxStack.ps1 -RequireRabbitMq` para detectarlo.
3. **Credenciales/host incorrectos** (producción): revisar `RabbitMq__...` en Render; no usar `localhost`.
4. **Audit API no está corriendo**: los mensajes pueden acumularse en la cola.
5. **Base de datos Audit no disponible**: el consumer falla al persistir; revisar logs de `AuditEventConsumer`.

---

## Compatibilidad gRPC (legacy temporal)

Las implementaciones `AuditGrpcEmitter` / `AuditGrpcClient` y `AuditGrpcService` permanecen en el código como fallback temporal, pero ya no están registradas en DI de los publishers. Para volver a gRPC habría que revertir el registro en `ServiceCollectionExtensions` o `Program.cs`.
