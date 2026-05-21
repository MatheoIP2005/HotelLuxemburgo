# Hotel Luxemburgo — Sistema de Gestión Hotelera
## Plan de Migración: REST Monolito → Microservicios Híbrido
### gRPC (procesos internos) + REST (endpoints públicos)

**Integración de Sistemas — PUCE 2026 | Docente: Damián Nicolalde**  
Sprint 2: 01 May – 29 May 2026 | Sprint 3: 30 May – 26 Jun 2026

---

## 1. Principio de la Arquitectura Híbrida

La arquitectura adopta un modelo donde REST gobierna la frontera pública del sistema y gRPC gobierna la comunicación interna entre microservicios. Esta separación no es opcional: responde a dos restricciones irreconciliables con un único protocolo.

**Regla de oro:**
- Si el endpoint es llamado por el cliente, browser o frontend React → REST (público, sin autenticación o con JWT visible)
- Si el endpoint es llamado exclusivamente por otro microservicio → gRPC (interno, tipado, HTTP/2)
- Si el endpoint es llamado por ambos (ej. admin REST + servicio interno) → mantener REST interno + implementar equivalente gRPC

| Canal | Protocolo | Autenticación | Propósito |
|---|---|---|---|
| Cliente / Browser → Gateway | REST / HTTPS | JWT Bearer | Contrato público estable |
| Servicio → Servicio (interno) | gRPC / HTTP2 | mTLS (scope académico: cleartext) | Rendimiento, tipado estricto |
| Eventos de auditoría | gRPC fire-and-forget | Sin respuesta bloqueante | Trazabilidad sin latencia |

---

## 2. Mapa de Microservicios y Separación de Bases de Datos

Cada microservicio es dueño exclusivo de su base de datos (Database-per-Service). Las FK cruzadas del monolito se reemplazan por columnas GUID sin FK formal.

| Servicio | Puerto REST | Puerto gRPC | Base de Datos | Tablas Principales |
|---|---|---|---|---|
| auth-service | :5001 | :5101 | HotelJJ_Auth | USUARIO_APP, ROL, USUARIOS_ROLES |
| accommodation-service | :5002 | :5102 | HotelJJ_Accommodation | SUCURSAL, TIPO_HABITACION, HABITACION, TARIFA, CATALOGO_SERVICIOS, TIPO_HAB_IMAGEN |
| reservation-service | :5003 | :5103 | HotelJJ_Reservation | CLIENTES, RESERVAS, RESERVAS_HABITACIONES |
| stay-service | :5004 | :5104 | HotelJJ_Stay | ESTADIAS, CARGO_ESTADIA |
| billing-service | :5005 | :5105 | HotelJJ_Billing | FACTURAS, FACTURA_DETALLE |
| payment-service | :5006 | :5106 | HotelJJ_Payment | PAGOS |
| rating-service | :5007 | — | HotelJJ_Rating | VALORACIONES |
| audit-service | :5008 (interno) | :5108 (consumer) | HotelJJ_Audit | AUDITORIA |

---

## 3. Criterio de Decisión: qué cambia a gRPC y qué se mantiene REST

No todos los endpoints internos migran a gRPC. La migración aplica cuando el endpoint es invocado exclusivamente por otro servicio en un flujo transaccional o de validación. Los endpoints administrativos de lectura que el gateway también sirve permanecen como REST interno.

| Endpoint / Caso de Uso | REST actual | Migra a gRPC | Razón |
|---|---|---|---|
| ValidateToken (auth) | No existía como REST interno | ✓ NUEVO gRPC | Todos los servicios validan JWT en cada request; latencia crítica |
| CheckAvailability (accommodation) | GET /habitaciones/disponibilidad | ✓ NUEVO gRPC | Llamado por reservation-service en cada confirmación; real-time |
| ConfirmRoomLock (accommodation) | PATCH /habitaciones/{id}/estado | ✓ MIGRA gRPC | Solo reservation-service lo llama; bloqueo atómico |
| UpdateRoomStatus / check-in-out (accommodation) | PATCH /habitaciones/{id}/estado | ✓ MIGRA gRPC | Solo stay-service lo llama al hacer check-in / check-out |
| GenerateReservationInvoice (billing) | POST /facturas (REST privado) | ✓ MIGRA gRPC | Solo reservation-service genera facturas; eliminar acceso REST |
| GenerateFinalInvoice (billing) | POST /facturas/final (REST privado) | ✓ MIGRA gRPC | Solo stay-service al hacer checkout |
| UpdateInvoiceBalance / MarkInvoicePaid (billing) | No existía explícito | ✓ NUEVO gRPC | payment-service actualiza saldo; nunca expuesto al cliente |
| ValidateStayCompleted (stay) | No existía | ✓ NUEVO gRPC | rating-service verifica estadía antes de aceptar valoración |
| EmitAuditEvent (audit) | No existía | ✓ NUEVO gRPC | Fire-and-forget desde todos los servicios; no bloquea flujo |
| GET /api/v1/accommodations/** (búsqueda pública) | REST público | ✗ MANTIENE REST | Consumido por browser / frontend; no tiene equivalente gRPC |
| POST /api/v1/internal/auth/login-refresh-logout | REST interno | ✗ MANTIENE REST | Frontend llama login; gRPC solo para ValidateToken inter-servicio |
| GET/POST /api/v1/public/reservas | REST público | ✗ MANTIENE REST | El cliente crea reservas vía Gateway; interfaz de usuario |
| PATCH reservas/{id}/confirmar - cancelar | REST interno | ✗ MANTIENE REST | Operación admin via Gateway; internamente reservation orquesta gRPC |
| GET /internal/facturas, /estadias, /pagos (lectura admin) | REST interno | ✗ MANTIENE REST | Lectura administrativa via Gateway; no hay ganancia con gRPC |
| GET /accommodations/{guid}/reviews | REST público | ✗ MANTIENE REST | Consumido por browser; rating-service usa gRPC solo para validar estadía |

✓ Verde = migra a gRPC | ✗ Naranja = se mantiene REST

---

## 4. Definición de .proto Files por Microservicio

Los archivos .proto son el contrato de comunicación interno. Deben vivir en un repositorio compartido (ej. /protos/) y ser compilados como paquete NuGet referenciado por todos los servicios. Jose Daniel es el responsable de mantener este repositorio.

### 4.1 auth.proto

| Método gRPC | Request Message | Response / Propósito |
|---|---|---|
| ValidateToken | ValidateTokenRequest { string token } | ValidateTokenResponse { bool valid; string user_guid; repeated string roles; } |
| GetUserRoles | GetUserRolesRequest { string user_guid } | GetUserRolesResponse { repeated string roles; } |

### 4.2 accommodation.proto

| Método gRPC | Request Message | Response / Propósito |
|---|---|---|
| CheckAvailability | CheckAvailRequest { string sucursal_guid; string fecha_entrada; string fecha_salida; int32 cantidad_personas; } | CheckAvailResponse { bool disponible; repeated HabitacionDisponible habitaciones; } |
| ConfirmRoomLock | ConfirmLockRequest { string habitacion_guid; string reserva_guid; string fecha_entrada; string fecha_salida; } | ConfirmLockResponse { bool success; string mensaje; } |
| UpdateRoomStatus | UpdateRoomStatusRequest { string habitacion_guid; string nuevo_estado; string operacion_guid; } | UpdateRoomStatusResponse { bool success; } |
| ReleaseRoomLock | ReleaseRoomRequest { string habitacion_guid; string reserva_guid; } | ReleaseRoomResponse { bool success; } |

### 4.3 reservation.proto

| Método gRPC | Request Message | Response / Propósito |
|---|---|---|
| GetReservation | GetReservationRequest { string reserva_guid } | GetReservationResponse { string reserva_guid; string estado; string cliente_guid; } |
| ValidateReservationForCheckin | ValidateCheckinRequest { string reserva_guid } | ValidateCheckinResponse { bool valid; string cliente_guid; repeated string habitacion_guids; } |

### 4.4 stay.proto

| Método gRPC | Request Message | Response / Propósito |
|---|---|---|
| ValidateStayCompleted | ValidateStayRequest { string estadia_guid } | ValidateStayResponse { bool completed; string reserva_guid; } |
| GetStayStatus | GetStayStatusRequest { string estadia_guid } | GetStayStatusResponse { string estado; string cliente_guid; } |

### 4.5 billing.proto

| Método gRPC | Request Message | Response / Propósito |
|---|---|---|
| GenerateReservationInvoice | GenReservInvoiceRequest { string reserva_guid; repeated InvoiceLineItem items; } | GenInvoiceResponse { string factura_guid; decimal total; } |
| GenerateFinalInvoice | GenFinalInvoiceRequest { string reserva_guid; string estadia_guid; } | GenInvoiceResponse { string factura_guid; decimal total; } |
| UpdateInvoiceBalance | UpdateBalanceRequest { string factura_guid; decimal monto_pagado; } | UpdateBalanceResponse { decimal saldo_pendiente; } |
| MarkInvoicePaid | MarkPaidRequest { string factura_guid } | MarkPaidResponse { bool success; } |

### 4.6 audit.proto (fire-and-forget)

| Método gRPC | Request Message | Response |
|---|---|---|
| EmitAuditEvent | AuditEventRequest { string servicio_origen; string tabla_afectada; string operacion; string entidad_guid; string usuario_guid; string detalle_json; } | google.protobuf.Empty |

---

## 5. Planes de Implementación por Integrante

Cada integrante tiene asignado un microservicio específico. Los planes a continuación detallan exactamente qué endpoints mantener como REST, cuáles migrar a gRPC, y las tareas por semana del Sprint 2 y Sprint 3.

---

### 5.1 Jorge Jara (JJ) — Infrastructure Lead
**Microservicio:** Separación de Bases de Datos + DevOps (Docker / docker-compose)

**▶ Brechas a corregir del Sprint 1**
- Eliminar GET /WeatherForecast (scaffolding de .NET sin remover)
- Corregir parámetros de ruta: usar {entidadGuid} en lugar de mezcla de {id} y {*Guid}
- Agregar GET /api/v1/internal/habitaciones/disponibilidad (actualmente ausente)
- Agregar POST /api/v1/public/reservas (actualmente ausente en este servicio — verificar asignación)

**▶ Sprint 2 — Semanas 1–2: Scripts de separación de DBs**
- Crear los 8 scripts SQL de separación de bases de datos:
  - HotelJJ_Auth (seguridad.USUARIO_APP, ROL, USUARIOS_ROLES)
  - HotelJJ_Accommodation (SUCURSAL, TIPO_HABITACION, HABITACION, TARIFA, CATALOGO_SERVICIOS, TIPO_HAB_CATALOGO, TIPO_HAB_IMAGEN)
  - HotelJJ_Reservation (CLIENTES, RESERVAS, RESERVAS_HABITACIONES)
  - HotelJJ_Stay (ESTADIAS, CARGO_ESTADIA)
  - HotelJJ_Billing (FACTURAS, FACTURA_DETALLE)
  - HotelJJ_Payment (PAGOS)
  - HotelJJ_Rating (VALORACIONES)
  - HotelJJ_Audit (seguridad.AUDITORIA)
- Incluir scripts de semilla con GUIDs fijos para referencias cruzadas entre servicios
- Entregar scripts validados — son BLOQUEANTES para todo el equipo (dependencia crítica)

**▶ Sprint 2 — Semanas 3–4: Docker + Infraestructura**
- Crear Dockerfiles para cada microservicio (.NET 10 / ASP.NET Core)
- Crear docker-compose.yml base con todos los servicios y sus puertos
- Configurar SQL Server en Docker: imagen mcr.microsoft.com/mssql/server:2022-latest con MSSQL_MEMORY_LIMIT_MB=512 por instancia
- Configurar red interna Docker para comunicación gRPC entre contenedores (cleartext HTTP/2, sin TLS interno)
- Documentar variables de entorno requeridas por cada servicio

**▶ Sprint 3: Integración y estabilidad**
- Mantener docker-compose actualizado conforme cada integrante integra su servicio
- Resolver problemas de red HTTP/2 entre contenedores (principal riesgo técnico)
- Documentar instrucciones de levantamiento local en README.md del repositorio raíz

**Endpoints REST — acciones requeridas:**

| Endpoint | Acción requerida |
|---|---|
| GET /WeatherForecast | ELIMINAR — scaffolding, no debe existir en producción |
| GET /api/v1/internal/habitaciones/disponibilidad | AGREGAR — faltante crítico del Sprint 1 |
| Todos los {id} en rutas internas | RENOMBRAR a {entidadGuid} ej: {habitacionGuid} |

---

### 5.2 Jose Daniel (JD) — API Gateway Lead
**Microservicio:** API Gateway (YARP .NET) + Repositorio de Protos

**▶ Brechas a corregir del Sprint 1**
- Eliminar POST /api/v1/alojamientos y POST /api/v1/alojamientos/buscar (no están en el contrato)
- Eliminar GET /api/v1/alojamientos/busqueda (duplica /accommodations/search)
- Agregar POST /api/v1/public/reservas — JD y Matheo no lo exponen, el flujo de integración queda cortado sin este endpoint
- Agregar GET /api/v1/internal/habitaciones/disponibilidad (actualmente ausente)

**▶ Sprint 2 — Semanas 1–2: API Gateway scaffold + Proto repository**
- Crear proyecto HotelJJ.Gateway usando YARP (.NET 10)
- Configurar rutas de proxy en yarp.json:
  - /api/v1/auth/** → auth-service:5001
  - /api/v1/accommodations/** → accommodation-service:5002
  - /api/v1/public/reservas/** → reservation-service:5003
  - /api/v1/internal/reservas/** → reservation-service:5003
  - /api/v1/internal/clientes/** → reservation-service:5003
  - /api/v1/internal/estadias/** → stay-service:5004
  - /api/v1/internal/facturas/** → billing-service:5005
  - /api/v1/internal/pagos/** → payment-service:5006
  - /api/v1/internal/valoraciones/** → rating-service:5007
- Crear repositorio compartido /protos/ con los archivos: auth.proto, accommodation.proto, reservation.proto, stay.proto, billing.proto, audit.proto
- Compilar protos y publicar como paquete NuGet interno (HotelJJ.Protos)
- RESPONSABILIDAD: ser el gatekeeper — cualquier cambio de .proto requiere aprobación de JD

**▶ Sprint 2 — Semanas 3–4: Middleware JWT en Gateway**
- Implementar middleware de validación JWT en el Gateway para rutas /internal/**
- El Gateway extrae el token del header Authorization → llama auth-service.ValidateToken via gRPC → rechaza con 401 si inválido
- Las rutas /public/** y /accommodations/** no requieren JWT
- Configurar rate limiting básico en Gateway para endpoints públicos

**▶ Sprint 3: Estabilización del Gateway**
- Verificar que todas las rutas del contrato del Sprint 1 están correctamente proxy-eadas
- Documentar el mapa completo de rutas Gateway en README.md
- Actualizar paquete NuGet de protos según cambios de los demás servicios

**Rutas a eliminar (no están en el contrato):**

| Ruta a eliminar | Motivo |
|---|---|
| POST /api/v1/alojamientos | Duplica semánticamente /accommodations; no está en contrato |
| POST /api/v1/alojamientos/buscar | Idem — crear ambigüedad con /accommodations/search |
| GET /api/v1/alojamientos/busqueda | Idem — mover lógica a /accommodations/search si falta |

---

### 5.3 Matheo — accommodation-service
**Microservicio:** accommodation-service — Puerto REST :5002 | Puerto gRPC :5102

**▶ Brechas a corregir del Sprint 1**
- Agregar POST /api/v1/public/reservas — Matheo no lo expone; el Booking central no puede crear reservas
- Verificar soft-delete: todos los módulos deben usar PATCH /inhabilitar en lugar de DELETE físico

**▶ Sprint 2 — Semanas 1–2: Separar microservicio**
- Crear proyecto HotelJJ.AccommodationService (.NET 10 / ASP.NET Core)
- Configurar EF Core apuntando a HotelJJ_Accommodation (esperar script de JJ)
- Migrar controllers REST existentes:
  - AccommodationController: GET /search, GET /{guid}, GET /categories — mantener como REST público (sin JWT)
  - HabitacionController: CRUD + GET /disponibilidad — mantener como REST interno (JWT Bearer)
  - GET /api/v1/accommodations/{guid}/reviews → proxy al rating-service (o delegar via Gateway)

**▶ Sprint 2 — Semanas 3–4: Implementar gRPC server**
- Referenciar paquete HotelJJ.Protos (publicado por JD)
- Implementar AccommodationGrpcService con los siguientes métodos:
  - CheckAvailability: consulta HABITACION + TARIFA para fechas solicitadas; retorna lista con disponible=true/false
  - ConfirmRoomLock: cambia estado de HABITACION a 'reservada' para el rango de fechas; transacción atómica
  - UpdateRoomStatus: cambia estado de HABITACION (libre/ocupada/mantenimiento); llamado por stay-service
  - ReleaseRoomLock: libera habitación si se cancela reserva
- Emitir EmitAuditEvent (gRPC fire-and-forget) en cada cambio de estado de habitación
- Validar JWT via auth-service.ValidateToken en todos los endpoints internos (usar cliente gRPC)

**Mapa REST vs gRPC para accommodation-service:**

| Endpoint | Protocolo final | Notas |
|---|---|---|
| GET /api/v1/accommodations/search | REST público | Sin JWT — consumido por frontend |
| GET /api/v1/accommodations/{guid} | REST público | Sin JWT |
| GET /api/v1/accommodations/categories | REST público | Sin JWT |
| GET /api/v1/internal/habitaciones/** | REST interno | JWT Bearer via Gateway |
| GET /api/v1/internal/habitaciones/disponibilidad | REST interno | También existe como gRPC CheckAvailability |
| CheckAvailability | gRPC :5102 | Llamado por reservation-service |
| ConfirmRoomLock | gRPC :5102 | Llamado por reservation-service al confirmar |
| UpdateRoomStatus | gRPC :5102 | Llamado por stay-service en check-in/out |

---

### 5.4 Juanito — reservation-service
**Microservicio:** reservation-service — Puerto REST :5003 | Puerto gRPC :5103

**▶ Brechas a corregir del Sprint 1 — CRÍTICAS**
- CRÍTICO: Mover toda la auth de /api/v1/auth/ a /api/v1/internal/auth/ — este es el problema más grave detectado; rompe el contrato interno REST
- Agregar POST /api/v1/internal/auth/refresh y POST /api/v1/internal/auth/logout (actualmente ausentes)
- Nota: si Juanito no gestiona auth-service, coordinar con el integrante asignado a auth-service (Ignacio) para la corrección de rutas

**▶ Sprint 2 — Semanas 1–2: Separar microservicio**
- Crear proyecto HotelJJ.ReservationService (.NET 10 / ASP.NET Core)
- Configurar EF Core apuntando a HotelJJ_Reservation (CLIENTES, RESERVAS, RESERVAS_HABITACIONES)
- Migrar controllers REST:
  - ReservaController: POST /public/reservas, GET /internal/reservas, GET /internal/reservas/{guid}, PATCH confirmar, PATCH cancelar
  - ClienteController: CRUD /internal/clientes/{guid}
  - GET /internal/habitaciones/disponibilidad — implementar endpoint REST que llama internamente via gRPC a accommodation-service

**▶ Sprint 2 — Semanas 3–4: gRPC cliente outbound + server**
- Implementar clientes gRPC outbound:
  - accommodation-service.CheckAvailability → llamar ANTES de crear reserva
  - accommodation-service.ConfirmRoomLock → llamar al confirmar reserva (PATCH /confirmar)
  - billing-service.GenerateReservationInvoice → llamar tras confirmar reserva exitosamente
  - auth-service.ValidateToken → validar JWT en todos los endpoints /internal/**
  - audit-service.EmitAuditEvent → en cada operación CRUD
- Implementar ReservationGrpcService (servidor):
  - GetReservation: retorna estado y datos de reserva para stay-service
  - ValidateReservationForCheckin: verifica que reserva esté confirmada antes del check-in
- IMPORTANTE: PATCH /confirmar orquesta la siguiente saga gRPC:
  1. CheckAvailability → verificar que la habitación sigue disponible
  2. ConfirmRoomLock → bloquear habitación
  3. Actualizar estado RESERVAS = 'CON' en BD local
  4. GenerateReservationInvoice → crear factura en billing-service
  5. EmitAuditEvent → registrar la confirmación

**Mapa REST vs gRPC para reservation-service:**

| Endpoint | Protocolo final | Notas |
|---|---|---|
| POST /api/v1/public/reservas | REST público | Sin JWT — cliente crea reserva |
| GET /api/v1/internal/reservas/** | REST interno | JWT Bearer — admin consulta reservas |
| PATCH /reservas/{guid}/confirmar | REST interno | JWT Bearer — orquesta saga gRPC internamente |
| PATCH /reservas/{guid}/cancelar | REST interno | JWT Bearer — libera lock de habitación via gRPC |
| GET/POST /api/v1/internal/clientes/** | REST interno | JWT Bearer |
| GetReservation | gRPC :5103 | Servidor — llamado por stay-service |
| ValidateReservationForCheckin | gRPC :5103 | Servidor — llamado por stay-service |
| CheckAvailability (cliente) | gRPC → :5102 | Cliente outbound → accommodation-service |
| GenerateReservationInvoice (cliente) | gRPC → :5105 | Cliente outbound → billing-service |

---

### 5.5 Kelvin — auth-service
**Microservicio:** auth-service — Puerto REST :5001 | Puerto gRPC :5101

**▶ Brechas a corregir del Sprint 1**
- Eliminar GET / (scaffolding — no debe existir en producción)
- Eliminar GET /alojamientos/busqueda (no está en el contrato de auth; pertenece a accommodation si acaso)
- Unificar auth bajo /api/v1/internal/auth/ únicamente — Kelvin actualmente duplica ambos prefijos; el alias sin /internal/ debe eliminarse
- Estandarizar parámetros de ruta a {entidadGuid} en lugar de {id} genérico

**▶ Sprint 2 — Semanas 1–2: Separar microservicio**
- Crear proyecto HotelJJ.AuthService (.NET 10 / ASP.NET Core)
- Configurar EF Core apuntando a HotelJJ_Auth (USUARIO_APP, ROL, USUARIOS_ROLES)
- Migrar controllers REST:
  - POST /api/v1/internal/auth/login — genera JWT (access + refresh token)
  - POST /api/v1/internal/auth/refresh — renueva access token con refresh token válido
  - POST /api/v1/internal/auth/logout — invalida refresh token
  - CRUD /api/v1/internal/usuarios — gestión de usuarios (JWT Bearer)
  - CRUD /api/v1/internal/roles — gestión de roles (JWT Bearer)
  - POST/GET /api/v1/internal/usuarios/{guid}/roles — asignación de roles

**▶ Sprint 2 — Semanas 3–4: Implementar gRPC server (prioridad máxima)**
- auth-service es el primer gRPC que debe estar operativo — todos los demás dependen de él
- Implementar AuthGrpcService:
  - ValidateToken: verifica firma JWT, extrae claims, retorna { valid, user_guid, roles[] }
  - GetUserRoles: dado un user_guid, retorna la lista de roles activos
- Cache Redis para ValidateToken: el 95% de las llamadas deben resolverse desde caché (TTL = vida restante del token)
- Emitir EmitAuditEvent en login, logout, creación de usuarios y cambios de rol

**▶ Sprint 3: Hardening**
- Validar que todos los servicios que llaman ValidateToken reciben respuesta en < 10ms (P99)
- Documentar la generación de JWT: algoritmo (HS256 o RS256), claims incluidos, tiempo de vida
- Implementar PATCH /inhabilitar en usuarios y roles (soft-delete correcto)

**Mapa REST vs gRPC para auth-service:**

| Endpoint | Protocolo final | Notas |
|---|---|---|
| POST /api/v1/internal/auth/login | REST interno | Frontend llama login via Gateway |
| POST /api/v1/internal/auth/refresh | REST interno | Frontend renueva token |
| POST /api/v1/internal/auth/logout | REST interno | Frontend invalida sesión |
| CRUD /api/v1/internal/usuarios/** | REST interno | JWT Bearer — solo admin |
| CRUD /api/v1/internal/roles/** | REST interno | JWT Bearer — solo admin |
| GET / | ELIMINAR | Scaffolding de .NET — no exponer |
| ValidateToken | gRPC :5101 | Servidor — llamado por TODOS los servicios en cada request |
| GetUserRoles | gRPC :5101 | Servidor — llamado para RBAC granular |

---

## 5.6–5.11 Integrantes Adicionales — Resumen de Microservicios

Los siguientes integrantes implementan microservicios que no tienen brechas del Sprint 1 documentadas, pero sí tienen tareas de migración híbrida REST+gRPC completas.

### 5.6 Ignacio — auth-service (coordinación con Kelvin)
- Implementar la capa gRPC de auth-service en coordinación con Kelvin
- Sprint 2 Sem 1-2: Levantar el servidor gRPC en :5101 con AuthGrpcService
- Sprint 2 Sem 3-4: Implementar caché Redis para ValidateToken
- Sprint 3: auth-service es el primer servicio que debe pasar el checklist de Definition of Done — todos dependen de él

### 5.7 María Paulina — accommodation-service (gRPC crítico)
- Implementar AccommodationGrpcService en :5102 — segundo servicio gRPC más crítico
- Sprint 2 Sem 1-2: Levantar proyecto y configurar EF Core sobre HotelJJ_Accommodation
- Sprint 2 Sem 3-4: CheckAvailability, ConfirmRoomLock, UpdateRoomStatus, ReleaseRoomLock
- Sprint 3: Validar con reservation-service que el flujo de confirmación de reserva funciona end-to-end
- Dependencia crítica: recibir script de HotelJJ_Accommodation de JJ antes de iniciar

### 5.8 Dana — stay-service
- Microservicio: stay-service — Puerto REST :5004 | Puerto gRPC :5104
- Sprint 2 Sem 1-2: Crear HotelJJ.StayService, EF Core sobre HotelJJ_Stay
- Migrar EstadiaController REST interno: POST /checkin/{reservaGuid}, POST /checkout/{estadiaGuid}, GET/POST /cargos
- Sprint 2 Sem 3-4: Implementar clientes gRPC outbound:
  - reservation-service.ValidateReservationForCheckin → antes de hacer check-in
  - accommodation-service.UpdateRoomStatus → cambiar habitación a 'ocupada' en check-in, 'libre' en checkout
  - billing-service.GenerateFinalInvoice → al hacer checkout
  - audit-service.EmitAuditEvent → en check-in, checkout y cada cargo
- Implementar StayGrpcService servidor: ValidateStayCompleted (para rating-service)
- Sprint 3: Coordinación con Doménica para el E2E flow completo

### 5.9 Doménica — reservation-service (integración E2E)
- Microservicio: reservation-service — liderar la integración final del flujo completo
- Sprint 2: Implementar la saga de confirmación de reserva orquestando todos los gRPC calls (ver plan de Juanito sección 5.4)
- Sprint 3: Escribir suite de integración E2E cubriendo el flujo de 10 pasos:
  - Crear cliente → Crear reserva → Confirmar reserva → Verificar factura
  - Registrar pago → Check-in → Agregar cargo → Check-out → Verificar factura final → Valoración
- Dependencia: todos los demás servicios gRPC deben estar operativos antes de Sprint 3

### 5.10 Martín Herrera — billing-service + Scrum Master
- Microservicio: billing-service — Puerto REST :5005 | Puerto gRPC :5105
- Sprint 2 Sem 1-2: Crear HotelJJ.BillingService, EF Core sobre HotelJJ_Billing
- Migrar FacturaController REST interno: listar, obtener, anular (GET/PATCH — solo lectura y anulación administrativa)
- IMPORTANTE: Los endpoints de generación de factura se eliminan de REST — solo existirán como gRPC:
  - GenerateReservationInvoice → equivalente a SP_GENERAR_FACTURA_RESERVA
  - GenerateFinalInvoice → equivalente a SP_GENERAR_FACTURA_FINAL
  - UpdateInvoiceBalance → actualiza saldo_pendiente
  - MarkInvoicePaid → cambia estado a PAG cuando saldo = 0
- Emitir EmitAuditEvent en cada generación/anulación de factura
- Como Scrum Master: mantener tablero ZOHO Sprints actualizado, coordinar dailies y sprint review

### 5.11 Anahí Berru — payment-service + Gestión de Board
- Microservicio: payment-service — Puerto REST :5006 | Puerto gRPC :5106
- Sprint 2 Sem 1-2: Crear HotelJJ.PaymentService, EF Core sobre HotelJJ_Payment (PAGOS)
- Migrar PagoController REST interno: listar pagos, obtener por guid, actualizar estado
- PATCH /api/v1/internal/pagos/{guid}/estado → sigue siendo REST externo (webhook de pasarela de pagos)
- Cliente HTTP hacia pasarela externa (Stripe sandbox o mock)
- Sprint 2 Sem 3-4: Implementar gRPC outbound:
  - billing-service.UpdateInvoiceBalance → al registrar pago parcial
  - billing-service.MarkInvoicePaid → si saldo resultante = 0
  - audit-service.EmitAuditEvent → en cada pago registrado
- Idempotencia: índice único filtrado sobre transaccion_externa para evitar pagos duplicados
- Como gestora de board: actualizar criterios de aceptación de User Stories en ZOHO según nueva arquitectura

### 5.12 Dylan Medina — rating-service + Product Owner
- Microservicio: rating-service — Puerto REST :5007 | Sin servidor gRPC (solo cliente outbound)
- Sprint 2 Sem 1-2: Crear HotelJJ.RatingService, EF Core sobre HotelJJ_Rating (VALORACIONES)
- Migrar ValoracionController: CRUD + moderar + responder valoraciones
- Endpoints REST que mantiene:
  - GET /api/v1/accommodations/{guid}/reviews → público, sin JWT — el contrato exige este endpoint
  - POST /api/v1/internal/valoraciones — crea valoración (JWT Bearer)
  - PATCH /api/v1/internal/valoraciones/{guid}/moderar — modera valoración
- gRPC outbound: al crear valoración → stay-service.ValidateStayCompleted → solo estadías en estado FIN pueden valorarse
- Emitir EmitAuditEvent en publicación y ocultamiento de valoraciones
- Validación: puntuación 0–10, retornar 422 si fuera de rango
- Como Product Owner: refinar User Stories con criterios de aceptación que incluyan validaciones gRPC, priorizar con MoSCoW

### 5.13 Stephano Zapata — audit-service + Product Owner
- Microservicio: audit-service — Puerto gRPC :5108 (solo consumer) | REST interno directo, sin Gateway público
- Sprint 2 Sem 1-2: Crear HotelJJ.AuditService, EF Core sobre HotelJJ_Audit (seguridad.AUDITORIA)
- NO expone REST externo — solo consume gRPC
- Implementar AuditGrpcConsumer: recibe EmitAuditEvent de cualquier servicio y persiste en AUDITORIA con servicio_origen
- Sprint 2 Sem 3-4: Modo fire-and-forget:
  - El servidor gRPC retorna google.protobuf.Empty inmediatamente
  - La inserción en BD se hace de forma asíncrona (Task.Run o channel en memoria)
  - Si audit-service cae, los demás servicios continúan operando — el evento se pierde (aceptable en scope académico)
- REST interno directo (sin pasar por Gateway público):
  - GET /api/v1/internal/auditoria — listado con filtros por tabla_afectada y rango de fechas
  - GET /api/v1/internal/auditoria/{guid} — detalle de evento
- Sprint 3: Validar que 100% de operaciones CRUD en todos los servicios generan al menos un evento de auditoría
- Como Product Owner: documentar acceptance criteria con campo 'genera evento de auditoría: Sí/No'

---

## 6. Orden de Implementación y Dependencias Críticas

| Semana | Responsables | Tareas (BLOQUEANTES primero) |
|---|---|---|
| Sem 1–2 | JJ (BLOQUEANTE) | 8 scripts SQL de separación de DBs con GUIDs semilla — todos dependen de esto |
| Sem 1–2 | JD (BLOQUEANTE) | API Gateway scaffold + repositorio de .proto files + paquete NuGet |
| Sem 1–2 | Juanito/JJ | Dockerfiles + docker-compose base |
| Sem 2–3 | Kelvin + Ignacio | auth-service gRPC :5101 operativo — primer servicio gRPC (todos dependen) |
| Sem 2–3 | Matheo + María Paulina | accommodation-service gRPC :5102 — segundo servicio crítico |
| Sem 3–4 | Martín | billing-service gRPC :5105 (depende de auth + accommodation) |
| Sem 3–4 | Dana | stay-service gRPC :5104 (depende de auth + accommodation + billing) |
| Sem 3–4 | Anahí | payment-service (depende de billing gRPC) |
| Sem 3–4 | Dylan | rating-service (depende de stay gRPC) |
| Sem 3–4 | Stephano | audit-service gRPC consumer (depende de protos compilados) |
| Sem 3–4 | Doménica | reservation-service — integra todos los anteriores (más complejo) |
| Sprint 3 | Doménica + Dana | Suite de integración E2E — 10 pasos del flujo completo |
| Sprint 3 | Todos | Correcciones, tests unitarios, README, PR review |

---

## 7. Definition of Done — Checklist por Microservicio

Un microservicio se considera Done cuando cumple TODOS los siguientes criterios:

- Levanta en Docker sin errores (docker-compose up)
- Todos sus endpoints REST retornan el código HTTP correcto según el contrato
- Sus métodos gRPC responden correctamente (validar con grpcurl o test unitario)
- Emite eventos a audit-service en cada operación CRUD
- Valida JWT via auth-service.ValidateToken en endpoints protegidos
- Al menos 5 tests unitarios en capa Application/Business
- README.md con instrucciones de ejecución local
- PR aprobado por al menos 1 compañero de equipo

---

## 8. Riesgos Principales y Mitigaciones

| Riesgo | Probabilidad | Impacto | Mitigación |
|---|---|---|---|
| Protos inconsistentes entre equipos | Alta | Alto | JD como gatekeeper; todo cambio requiere aprobación antes de merge |
| gRPC no funciona en Docker por HTTP/2 | Media | Alto | Usar cleartext entre contenedores; TLS solo en Gateway externo |
| Scripts de DB llegan tarde (JJ bloqueante) | Media | Alto | JJ entrega scripts en Semana 1 — prioridad máxima del Sprint 2 |
| Tiempo insuficiente para integración completa | Alta | Alto | Priorizar reservation + accommodation + billing; el resto puede ser stub |
| SQL Server en Docker consume mucha RAM | Media | Medio | MSSQL_MEMORY_LIMIT_MB=512 por instancia en docker-compose |
| Saga de confirmación de reserva con error parcial | Media | Alto | Implementar compensación: si GenerateInvoice falla, llamar ReleaseRoomLock |

---

*— Fin del Plan de Migración —*
