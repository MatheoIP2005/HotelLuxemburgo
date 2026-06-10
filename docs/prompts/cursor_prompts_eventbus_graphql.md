# Prompts para Cursor - RabbitMQ, bus de eventos y GraphQL

Este documento contiene prompts secuenciales para implementar RabbitMQ, bus de eventos y GraphQL en el backend de HotelLuxemburgo. La regla principal es avanzar un cambio a la vez, compilar y probar despues de cada paso.

## Contexto base para todos los prompts

Estas son las condiciones actuales del proyecto:

- Backend .NET 8 con microservicios por dominio.
- Solucion principal: `HotelLux.Stack.slnx`.
- Microservicios:
  - `HotelLux.Auth`
  - `HotelLux.Accommodation`
  - `HotelLux.Reservation`
  - `HotelLux.Stay`
  - `HotelLux.Finance`
  - `HotelLux.Audit`
  - `HotelLux.Gateway`
- Cada microservicio usa capas `API`, `Business`, `DataManagement`, `DataAccess`.
- La comunicacion interna actual entre servicios es gRPC.
- La auditoria actual se emite por gRPC fire-and-forget hacia `HotelLux.Audit`.
- No existe RabbitMQ, MassTransit, Kafka ni bus de eventos.
- El Gateway actual usa YARP y Swagger agregado; todavia no tiene GraphQL.
- No reestructures el proyecto completo. Mantener los patrones existentes.
- No cambiar logica de negocio salvo que sea estrictamente necesario.
- No revertir cambios locales existentes.
- Despues de cada prompt, ejecutar build y tests.

Comandos minimos de verificacion despues de cada cambio:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Si algun comando falla, detenerse, corregir el fallo y volver a ejecutar los comandos antes de seguir.

---

## Prompt 1 - Crear contrato compartido de evento de auditoria

Quiero que implementes el primer paso para introducir un bus de eventos con RabbitMQ, pero sin configurar RabbitMQ todavia.

Objetivo:
Crear un contrato compartido en `HotelLux.Shared` para representar eventos de auditoria de manera independiente de gRPC.

Contexto:
Actualmente los servicios publican auditoria usando `audit.proto` y las implementaciones `AuditGrpcEmitter` o `AuditGrpcClient`. No quiero eliminar nada todavia. Este paso solo crea el contrato compartido que despues usara RabbitMQ.

Cambios a realizar:

1. Crear carpeta:
   `HotelLux.Shared/Events`

2. Crear archivo:
   `HotelLux.Shared/Events/AuditEventMessage.cs`

3. Definir un record o clase publica `AuditEventMessage` con estos campos:
   - `Guid EventId`
   - `string ServicioOrigen`
   - `string TablaAfectada`
   - `string Operacion`
   - `string EntidadGuid`
   - `string? IdRegistro`
   - `string UsuarioGuid`
   - `string UsuarioEjecutor`
   - `string? IpOrigen`
   - `string? DatosAnterioresJson`
   - `string? DatosNuevosJson`
   - `DateTimeOffset FechaEventoUtc`

4. Agregar valores por defecto seguros:
   - `EventId = Guid.NewGuid()`
   - strings requeridos con `string.Empty`
   - `FechaEventoUtc = DateTimeOffset.UtcNow`

5. No agregar MassTransit todavia.
6. No modificar los emisores gRPC todavia.
7. No modificar `AuditGrpcService` todavia.

Criterios de aceptacion:

- `HotelLux.Shared` compila.
- `HotelLux.Stack.slnx` compila.
- No se rompio ningun microservicio.

Verificacion obligatoria:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Al finalizar, reporta:

- Archivos modificados.
- Resultado de build.
- Resultado de tests.
- Si detectaste algun riesgo para el siguiente paso.

---

## Prompt 2 - Agregar MassTransit y configuracion RabbitMQ reusable

Quiero que implementes el segundo paso del bus de eventos: agregar MassTransit con RabbitMQ de forma reusable, pero sin migrar emisores todavia.

Objetivo:
Crear infraestructura compartida para configurar RabbitMQ en los microservicios API.

Contexto:
Ya debe existir `HotelLux.Shared/Events/AuditEventMessage.cs`.
El proyecto sigue usando gRPC para auditoria en este punto. No reemplaces todavia los `AuditGrpcEmitter`.

Cambios a realizar:

1. Agregar paquetes NuGet necesarios a los proyectos API que publicaran o consumiran eventos:
   - `HotelLux.Auth.API`
   - `HotelLux.Accommodation.API`
   - `HotelLux.Reservation.API`
   - `HotelLux.Stay.API`
   - `HotelLux.Finance.API`
   - `HotelLux.Audit.API`

   Paquetes:
   - `MassTransit`
   - `MassTransit.RabbitMQ`

2. Crear en `HotelLux.Shared` una configuracion reusable:
   - Carpeta: `HotelLux.Shared/Messaging`
   - Archivo: `RabbitMqSettings.cs`
   - Archivo: `RabbitMqConfigurationExtensions.cs`

3. `RabbitMqSettings` debe mapear:
   - `Host`
   - `VirtualHost`
   - `Username`
   - `Password`
   - `AuditQueue`

   Valores por defecto sugeridos:
   - `Host = "localhost"`
   - `VirtualHost = "/"`
   - `Username = "guest"`
   - `Password = "guest"`
   - `AuditQueue = "hotellux.audit.events"`

4. Crear extension reusable para configurar el host de RabbitMQ dentro de MassTransit. Debe evitar duplicacion entre servicios.

5. Agregar seccion `RabbitMq` en los `appsettings.json` de los proyectos API anteriores.

6. No registrar consumidores todavia.
7. No cambiar `IAuditEmitter` todavia.
8. No eliminar referencias gRPC todavia.

Criterios de aceptacion:

- Todos los `.csproj` restauran y compilan.
- La configuracion se puede leer desde `appsettings.json` y variables de entorno.
- No hay consumidores ni publishers nuevos todavia.

Verificacion obligatoria:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Al finalizar, reporta:

- Paquetes agregados por proyecto.
- Archivos modificados.
- Resultado de build y tests.
- Si NuGet o restore fallo, explica el error exacto.

---

## Prompt 3 - Implementar consumidor RabbitMQ en Audit

Quiero que implementes el consumidor de eventos de auditoria en `HotelLux.Audit.API`.

Objetivo:
`HotelLux.Audit.API` debe consumir eventos `AuditEventMessage` desde RabbitMQ y persistirlos en la base de auditoria, usando la misma logica que hoy existe en `AuditGrpcService`.

Contexto:
Ya existe el contrato `AuditEventMessage`.
Ya existen paquetes MassTransit y configuracion RabbitMQ.
Actualmente `AuditGrpcService` persiste eventos recibidos por gRPC. No lo elimines todavia; debe quedar como compatibilidad temporal.

Cambios a realizar:

1. Crear carpeta:
   `HotelLux.Audit/HotelLux.Audit.API/Consumers`

2. Crear consumidor:
   `AuditEventConsumer.cs`

3. El consumidor debe implementar `IConsumer<AuditEventMessage>`.

4. Inyectar:
   - `AuditDbContext`
   - `ILogger<AuditEventConsumer>`

5. Mapear `AuditEventMessage` a `EventoAuditoriaEntity` igual que `AuditGrpcService`.

6. Normalizar `Operacion` antes de guardar, porque la tabla solo acepta:
   - `INSERT`
   - `UPDATE`
   - `DELETE`

   Regla sugerida:
   - `INSERT`, `CREATE`, `LOGIN`, `ASSIGN_ROLE` => `INSERT`
   - `DELETE`, `LOGOUT`, `REMOVE_ROLE`, `REVOKE` => `DELETE`
   - cualquier otro valor, incluyendo `LOCK`, `DISABLE`, `ENABLE`, `CAMBIO_PASSWORD` => `UPDATE`

7. Registrar MassTransit en `HotelLux.Audit.API/Program.cs`:
   - Agregar consumer `AuditEventConsumer`
   - Configurar RabbitMQ
   - Configurar receive endpoint con cola `RabbitMq:AuditQueue`
   - Usar `ConfigureConsumer<AuditEventConsumer>(context)`

8. Mantener:
   - `app.MapGrpcService<AuditGrpcService>().EnableGrpcWeb();`
   - Swagger
   - Controllers
   - Health checks

9. Agregar health check de RabbitMQ si es simple y no rompe dependencias. Si no es claro, omitirlo y explicar.

Criterios de aceptacion:

- `HotelLux.Audit.API` compila.
- `AuditGrpcService` sigue existiendo y compilando.
- El consumer usa `AuditDbContext` con lifetime correcto.
- No se duplican registros salvo que llegue el mismo evento por ambos caminos.

Verificacion obligatoria:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Al finalizar, reporta:

- Archivos modificados.
- Como queda registrada la cola.
- Resultado de build y tests.
- Como se probaria manualmente con RabbitMQ levantado.

---

## Prompt 4 - Migrar publishers de auditoria a RabbitMQ, servicio por servicio

Quiero migrar los emisores de auditoria desde gRPC a RabbitMQ, pero servicio por servicio y sin tocar la logica Business.

Objetivo:
Reemplazar las implementaciones concretas de `IAuditEmitter` para que publiquen `AuditEventMessage` en RabbitMQ.

Contexto:
Los servicios Business ya dependen de `IAuditEmitter`.
No quiero modificar los services Business salvo que sea inevitable.
La frontera correcta esta en las implementaciones dentro de la capa API.

Servicios a migrar:

1. Accommodation
2. Reservation
3. Stay
4. Finance
5. Auth

Importante:
Auth tiene una interfaz diferente:
`Task EmitAsync(...)`

Los demas usan:
`void EmitFireAndForget(...)`

Cambios a realizar:

### 4.1 Accommodation

1. Crear:
   `HotelLux.Accommodation/HotelLux.Accommodation.API/Services/AuditRabbitMqEmitter.cs`

2. Implementar `HotelLux.Accommodation.Business.Interfaces.IAuditEmitter`.

3. Inyectar:
   - `IPublishEndpoint`
   - `ILogger<AuditRabbitMqEmitter>`

4. En `EmitFireAndForget`, publicar `AuditEventMessage`.

5. Mantener semantica fire-and-forget:
   - No bloquear flujo de negocio.
   - Capturar excepciones y loguear warning.

6. Cambiar registro DI en `ServiceCollectionExtensions.cs`:
   - Antes: `IAuditEmitter, AuditGrpcEmitter`
   - Despues: `IAuditEmitter, AuditRabbitMqEmitter`

7. No borrar `AuditGrpcEmitter` todavia.

8. Compilar y probar antes de seguir con otro servicio.

### 4.2 Reservation

Repetir el mismo patron en:

- `HotelLux.Reservation/HotelLux.Reservation.API/Services/AuditRabbitMqEmitter.cs`
- Registro DI en `HotelLux.Reservation.API/Extensions/ServiceCollectionExtensions.cs`

No tocar `ReservaService`.

Compilar y probar antes de seguir.

### 4.3 Stay

Repetir el mismo patron en:

- `HotelLux.Stay/HotelLux.Stay.API/Services/AuditRabbitMqEmitter.cs`
- Registro DI en `HotelLux.Stay.API/Extensions/ServiceCollectionExtensions.cs`

No tocar `EstadiaService`, `ValoracionService` ni `CargoEstadiaService` salvo que sea obligatorio.

Compilar y probar antes de seguir.

### 4.4 Finance

Repetir el mismo patron en:

- `HotelLux.Finance/HotelLux.Finance.API/Clients/AuditRabbitMqClient.cs` o `Services/AuditRabbitMqEmitter.cs`
- Registro DI en `HotelLux.Finance.API/Program.cs`

Mantener el nombre que mejor respete la estructura actual. Actualmente Finance usa `Clients/AuditGrpcClient.cs`.

Compilar y probar antes de seguir.

### 4.5 Auth

Auth usa:

```csharp
Task EmitAsync(string tablaAfectada, string operacion, string entidadGuid, string usuarioGuid, string detalleJson, CancellationToken cancellationToken = default);
```

1. Crear:
   `HotelLux.Auth/HotelLux.Auth.API/Services/AuditRabbitMqEmitter.cs`

2. Implementar `HotelLux.Auth.Business.Interfaces.IAuditEmitter`.

3. Mantener la normalizacion de operaciones que ya tiene `AuditGrpcEmitter`.

4. Construir `AuditEventMessage` con:
   - `ServicioOrigen = "auth-service"`
   - `DatosNuevosJson` incluyendo verbo y detalle igual que hoy.

5. Cambiar DI en:
   `HotelLux.Auth/HotelLux.Auth.API/Extensions/ServiceCollectionExtensions.cs`

6. No borrar `AuditGrpcEmitter` todavia.

Compilar y probar.

Criterios de aceptacion globales:

- Los cinco servicios publican por RabbitMQ.
- `AuditGrpcEmitter` puede quedar en codigo como fallback temporal, pero ya no debe estar registrado en DI.
- No se modifica Business salvo necesidad justificada.
- Build y tests pasan despues de cada servicio y al final.

Verificacion obligatoria final:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Prueba manual sugerida con RabbitMQ:

1. Levantar RabbitMQ local.
2. Levantar `HotelLux.Audit.API`.
3. Levantar un servicio publisher, por ejemplo Accommodation.
4. Ejecutar una operacion que emita auditoria.
5. Confirmar en RabbitMQ Management que la cola recibe/consume mensajes.
6. Confirmar en BD Audit que se inserta el evento.

Al finalizar, reporta:

- Servicio migrado.
- Archivos modificados.
- Resultado de build y tests.
- Si RabbitMQ no estaba disponible, indicar que solo se valido compilacion/tests.

---

## Prompt 5 - Agregar soporte local para RabbitMQ y documentacion

Quiero agregar soporte local/documentado para RabbitMQ sin afectar la forma actual de levantar PostgreSQL ni los microservicios.

Objetivo:
Facilitar que el equipo pueda levantar RabbitMQ localmente y verificar el bus de eventos.

Cambios a realizar:

1. Agregar `docker-compose.rabbitmq.yml` en la raiz del repo.

2. Debe incluir solo RabbitMQ con management:
   - Imagen sugerida: `rabbitmq:3-management`
   - Puerto AMQP: `5672`
   - Puerto management UI: `15672`
   - Usuario: `guest`
   - Password: `guest`

3. Agregar script PowerShell opcional:
   `scripts/Start-RabbitMq.ps1`

   Debe ejecutar:
   `docker compose -f docker-compose.rabbitmq.yml up -d`

4. Agregar script PowerShell opcional:
   `scripts/Test-RabbitMq.ps1`

   Debe verificar que `http://localhost:15672` responde.

5. Actualizar README o crear doc nuevo:
   `docs/rabbitmq_event_bus.md`

   Debe explicar:
   - Como levantar RabbitMQ.
   - Variables `RabbitMq__...`.
   - Cola usada por auditoria.
   - Flujo publisher -> RabbitMQ -> Audit consumer -> BD.
   - Como diagnosticar si no se insertan eventos.

6. No cambiar scripts existentes de stack salvo que sea necesario.

Criterios de aceptacion:

- No se rompe el build.
- La documentacion es clara para Windows/PowerShell.
- Los scripts no deben hacer acciones destructivas.

Verificacion obligatoria:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Si Docker esta disponible:

```powershell
docker compose -f docker-compose.rabbitmq.yml up -d
powershell -ExecutionPolicy Bypass -File scripts\Test-RabbitMq.ps1
```

Al finalizar, reporta:

- Archivos agregados.
- Comandos ejecutados.
- Resultado de build/tests.
- Resultado de prueba RabbitMQ si se pudo ejecutar.

---

## Prompt 6 - Agregar GraphQL al Gateway como BFF

Quiero agregar GraphQL en `HotelLux.Gateway` sin reemplazar YARP ni Swagger.

Objetivo:
El Gateway debe exponer `/graphql` como una capa BFF que consulta los endpoints REST existentes de los microservicios a traves de HTTP.

Contexto:
El Gateway actual usa:

- YARP ReverseProxy
- Swagger agregado propio
- Rutas REST existentes

No quiero eliminar ni romper:

- `app.MapReverseProxy()`
- `/swagger`
- `/swagger/v1/swagger.json`
- `/health`
- rutas YARP existentes

Cambios a realizar:

1. Agregar paquete NuGet a `HotelLux.Gateway`:
   - `HotChocolate.AspNetCore`

2. Crear estructura:
   - `HotelLux.Gateway/GraphQL`
   - `HotelLux.Gateway/GraphQL/Types`
   - `HotelLux.Gateway/GraphQL/Clients`

3. Crear clientes HTTP tipados para llamar a servicios existentes:
   - `AccommodationGatewayClient`
   - `ReservationGatewayClient`

   Usar direcciones desde configuracion `ReverseProxy:Clusters`.

4. Los clientes deben reenviar header `Authorization` recibido en Gateway cuando aplique.

5. Crear `Query` GraphQL con campos publicos iniciales:
   - `accommodationsSearch(...)`
   - `accommodation(sucursalGuid: UUID!)`
   - `accommodationReviews(sucursalGuid: UUID!, pagina: Int, limite: Int)`
   - `reservation(reservaGuid: UUID!)`

6. Crear `Mutation` GraphQL inicial:
   - `createReservation(input: CreateReservationInput!)`

7. Mapear DTOs de manera conservadora:
   - Puedes crear DTOs especificos en Gateway para no depender de proyectos Business de otros servicios.
   - No referenciar proyectos Business desde Gateway.
   - Usar `System.Text.Json` con camelCase.

8. Registrar GraphQL en `Program.cs`:

   - `builder.Services.AddGraphQLServer()...`
   - `app.MapGraphQL("/graphql")`

9. Mantener:

   - `app.MapGet("/", ...)`
   - `app.MapGet("/health", ...)`
   - `app.UseGatewaySwaggerUi()`
   - `app.MapReverseProxy()`

10. Agregar al health de Gateway un campo informativo:
   - `graphql = "/graphql"`

11. No implementar todavia autenticacion por campo avanzada. Solo reenviar `Authorization`.

Criterios de aceptacion:

- Gateway compila.
- Swagger sigue funcionando.
- YARP sigue funcionando.
- `/graphql` responde.
- No se agregan dependencias desde Gateway hacia Business de microservicios.

Verificacion obligatoria:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Prueba manual sugerida:

1. Levantar al menos Accommodation, Reservation y Gateway.
2. Ejecutar:

```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:5000/health" -UseBasicParsing
```

3. Probar introspeccion GraphQL:

```powershell
$body = @{ query = "{ __typename }" } | ConvertTo-Json
Invoke-WebRequest -Uri "http://127.0.0.1:5000/graphql" -Method POST -ContentType "application/json" -Body $body -UseBasicParsing
```

4. Probar query publica real cuando los servicios esten levantados.

Al finalizar, reporta:

- Schema inicial expuesto.
- Archivos modificados.
- Resultado de build/tests.
- Resultado de prueba `/graphql` si se pudo ejecutar.

---

## Prompt 7 - Prueba integrada end-to-end

Quiero que hagas una verificacion integrada de RabbitMQ + Audit + Gateway GraphQL.

Objetivo:
Validar que el backend completo funciona despues de introducir bus de eventos y GraphQL.

Pasos:

1. Revisar `git status --short` y listar cambios.

2. Ejecutar:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

3. Si Docker esta disponible, levantar RabbitMQ:

```powershell
docker compose -f docker-compose.rabbitmq.yml up -d
```

4. Levantar servicios necesarios:
   - Audit
   - Auth
   - Accommodation
   - Reservation
   - Gateway

5. Probar health checks:
   - `http://127.0.0.1:5008/health`
   - `http://127.0.0.1:5001/health`
   - `http://127.0.0.1:5002/health`
   - `http://127.0.0.1:5003/health`
   - `http://127.0.0.1:5000/health`

6. Probar Swagger Gateway:
   - `http://127.0.0.1:5000/swagger/v1/swagger.json`

7. Probar GraphQL:

```graphql
query {
  __typename
}
```

8. Ejecutar una operacion que emita auditoria.

9. Confirmar que:
   - El mensaje llega a RabbitMQ.
   - `HotelLux.Audit.API` lo consume.
   - Se inserta en la tabla `auditoria.evento_auditoria`.

10. Si no hay BD local disponible, dejar documentado que la prueba de persistencia queda pendiente y explicar exactamente que si se valido.

Criterios de aceptacion:

- Build pasa.
- Tests pasan.
- Gateway sigue sirviendo REST y Swagger.
- `/graphql` responde.
- RabbitMQ recibe eventos.
- Audit consume eventos cuando BD esta disponible.

Reporte final requerido:

- Cambios finales por area:
  - Shared
  - Audit
  - Publishers
  - Gateway GraphQL
  - Scripts/docs
- Comandos ejecutados y resultados.
- Riesgos pendientes.
- Siguientes pasos recomendados.

---

## Prompt 8 - Corregir lifetime DI de publishers RabbitMQ antes del E2E

Necesito que corrijas un problema real encontrado durante la validacion runtime.

Contexto:
El build y los tests pasan, pero al arrancar un servicio publisher, por ejemplo `HotelLux.Accommodation.API`, falla la construccion del ServiceProvider en Development con este error:

```text
Cannot consume scoped service 'MassTransit.IPublishEndpoint'
from singleton 'HotelLux.Accommodation.Business.Interfaces.IAuditEmitter'.
```

Causa:
Los nuevos emitters RabbitMQ fueron registrados como singleton, pero inyectan `IPublishEndpoint`, que MassTransit registra con lifetime scoped/contextual. Eso compila, pero rompe el arranque con validacion de scopes. Ademas, aunque se cambiara `IAuditEmitter` a scoped, los emitters actuales usan `Task.Run`, lo que podria capturar un servicio scoped despues de que el request scope termine.

Objetivo:
Corregir los publishers RabbitMQ para que puedan usarse de forma segura como fire-and-forget y no rompan el arranque de los microservicios.

Solucion recomendada:
Mantener `IAuditEmitter` como singleton si se desea conservar el patron anterior, pero cambiar los emitters para inyectar `MassTransit.IBus` en vez de `IPublishEndpoint`.

Motivo:
`IBus` es seguro para un singleton y permite llamar `Publish(...)` sin capturar un servicio scoped dentro de `Task.Run`.

Archivos a corregir:

1. `HotelLux.Accommodation/HotelLux.Accommodation.API/Services/AuditRabbitMqEmitter.cs`
2. `HotelLux.Reservation/HotelLux.Reservation.API/Services/AuditRabbitMqEmitter.cs`
3. `HotelLux.Stay/HotelLux.Stay.API/Services/AuditRabbitMqEmitter.cs`
4. `HotelLux.Finance/HotelLux.Finance.API/Clients/AuditRabbitMqClient.cs`
5. `HotelLux.Auth/HotelLux.Auth.API/Services/AuditRabbitMqEmitter.cs`

Cambios concretos:

1. Reemplazar:

```csharp
private readonly IPublishEndpoint _publishEndpoint;
```

por:

```csharp
private readonly IBus _bus;
```

2. Reemplazar constructores que reciben `IPublishEndpoint publishEndpoint` por `IBus bus`.

3. Reemplazar:

```csharp
await _publishEndpoint.Publish(...)
```

por:

```csharp
await _bus.Publish(...)
```

4. Mantener la semantica fire-and-forget y los logs de warning.

5. En Auth, mantener el metodo `Task EmitAsync(...)`, pero tambien usar `IBus`.

6. No cambiar las interfaces Business.

7. No tocar la logica Business.

8. No eliminar todavia los emitters gRPC antiguos.

9. Mantener los registros DI actuales si quedan validos con `IBus`.

Verificacion obligatoria:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Verificacion runtime obligatoria despues del build:

Arrancar al menos Accommodation en Development sin RabbitMQ corriendo para confirmar que ya no falla por DI:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:PORT='5052'
$env:GRPC_PORT='5052'
$env:Jwt__Key='HotelLuxemburgo_AccessSecret_MinimoTrentaYDosCaracteres_2026'
dotnet run --no-build --no-launch-profile --project HotelLux.Accommodation\HotelLux.Accommodation.API\HotelLux.Accommodation.API.csproj
```

En otra terminal:

```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:5052/health" -UseBasicParsing
```

Resultado esperado:

- El servicio arranca.
- `/health` responde 200.
- Si RabbitMQ no esta disponible, puede haber logs de reconexion de MassTransit, pero NO debe fallar por lifetime DI.

Si el servicio no arranca porque RabbitMQ no esta disponible, reportalo claramente. En ese caso, el siguiente ajuste sera hacer que RabbitMQ sea opcional o que el bus no bloquee startup local.

Reporte final requerido:

- Archivos modificados.
- Confirmacion de que ya no se inyecta `IPublishEndpoint` en singletons.
- Resultado de build.
- Resultado de tests.
- Resultado de runtime `/health`.

---

## Prompt 9 - Separar health checks de liveness y readiness

Necesito que ajustes los health checks despues de integrar MassTransit/RabbitMQ.

Contexto:
El fix del Prompt 8 corrigio el problema de DI: los servicios ya no fallan por `Cannot consume scoped service IPublishEndpoint from singleton IAuditEmitter`.

Nuevo comportamiento detectado:
MassTransit registra automaticamente un health check del bus. Cuando RabbitMQ no esta levantado, endpoints como `/health` devuelven 503 aunque la API ya haya arrancado correctamente.

Esto puede ser correcto para readiness, pero es confuso para liveness/local/dev y para pruebas basicas de que la API esta viva.

Objetivo:
Separar claramente:

- Liveness: la API arranco y puede responder HTTP.
- Readiness: la API y sus dependencias criticas, incluyendo RabbitMQ, estan listas.

Cambios requeridos:

1. En todos los proyectos API que usan `app.MapHealthChecks("/health")`, reemplazar el mapeo simple por tres endpoints:

   - `/health`: liveness compatible hacia atras, debe responder 200 si la app esta viva aunque RabbitMQ este caido.
   - `/health/live`: igual que liveness, debe responder 200 si la app esta viva.
   - `/health/ready`: readiness completo, debe incluir MassTransit/RabbitMQ y devolver 503 si RabbitMQ esta caido.

2. Aplicar en:

   - `HotelLux.Auth/HotelLux.Auth.API/Program.cs`
   - `HotelLux.Accommodation/HotelLux.Accommodation.API/Program.cs`
   - `HotelLux.Reservation/HotelLux.Reservation.API/Program.cs`
   - `HotelLux.Stay/HotelLux.Stay.API/Program.cs`
   - `HotelLux.Finance/HotelLux.Finance.API/Program.cs`
   - `HotelLux.Audit/HotelLux.Audit.API/Program.cs`

3. Gateway no usa RabbitMQ. Puede mantener `/health` actual, pero agrega `/health/live` y `/health/ready` si es sencillo y consistente.

4. Para liveness usar `HealthCheckOptions` con predicate que excluya MassTransit/RabbitMQ. Dos opciones aceptables:

   Opcion A:

   ```csharp
   app.MapHealthChecks("/health", new HealthCheckOptions
   {
       Predicate = registration => !registration.Name.Contains("masstransit", StringComparison.OrdinalIgnoreCase)
   });
   ```

   Opcion B:

   ```csharp
   app.MapHealthChecks("/health", new HealthCheckOptions
   {
       Predicate = _ => false
   });
   ```

   Preferir opcion A si funciona, porque conserva otros health checks no relacionados al broker.

5. Para readiness:

   ```csharp
   app.MapHealthChecks("/health/ready");
   ```

   Debe incluir todos los health checks registrados, incluido MassTransit.

6. Agregar el using necesario:

   ```csharp
   using Microsoft.AspNetCore.Diagnostics.HealthChecks;
   ```

7. No cambiar la configuracion de MassTransit.
8. No hacer RabbitMQ opcional todavia.
9. No tocar logica Business.

Criterios de aceptacion:

- Con RabbitMQ apagado:
  - `/health` responde 200.
  - `/health/live` responde 200.
  - `/health/ready` responde 503.
- Con RabbitMQ encendido:
  - `/health` responde 200.
  - `/health/live` responde 200.
  - `/health/ready` responde 200.
- Build y tests pasan.

Verificacion obligatoria:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Verificacion runtime minima sin RabbitMQ:

Arrancar Accommodation:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:PORT='5052'
$env:GRPC_PORT='5052'
$env:Jwt__Key='HotelLuxemburgo_AccessSecret_MinimoTrentaYDosCaracteres_2026'
dotnet run --no-build --no-launch-profile --project HotelLux.Accommodation\HotelLux.Accommodation.API\HotelLux.Accommodation.API.csproj
```

En otra terminal:

```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:5052/health" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5052/health/live" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5052/health/ready" -UseBasicParsing
```

Resultado esperado sin RabbitMQ:

- `/health`: 200
- `/health/live`: 200
- `/health/ready`: 503

Actualizar documentacion:

1. Actualizar `docs/rabbitmq_event_bus.md` explicando la diferencia:
   - `/health` y `/health/live`: app viva.
   - `/health/ready`: app lista con RabbitMQ.

2. Si existe script de health local, actualizarlo para usar `/health` o `/health/live` segun corresponda.

Reporte final requerido:

- Archivos modificados.
- Resultado de build.
- Resultado de tests.
- Resultado runtime sin RabbitMQ para `/health`, `/health/live`, `/health/ready`.
- Confirmar que `/health/ready` queda como verificacion real del bus RabbitMQ.

---

## Prompt 10 - Validacion integrada RabbitMQ + Audit + Gateway GraphQL

Necesito que hagas una validacion integrada de los cambios ya implementados. Este prompt es principalmente de pruebas. No hagas cambios de codigo salvo que encuentres un fallo real y puntual durante la validacion.

Objetivo:
Comprobar que:

- RabbitMQ levanta localmente.
- Los servicios conectan con RabbitMQ.
- `HotelLux.Audit.API` consume `AuditEventMessage`.
- Al ejecutar una operacion que publica auditoria, el evento termina persistido en `auditoria.evento_auditoria`.
- El Gateway sigue respondiendo REST, Swagger y GraphQL.

Contexto:
Ya se implementaron:

- `AuditEventMessage` en `HotelLux.Shared`.
- MassTransit + RabbitMQ.
- `AuditEventConsumer` en Audit.
- Publishers RabbitMQ en Auth, Accommodation, Reservation, Stay y Finance.
- `IBus` en emitters para evitar lifetime DI incorrecto.
- Health checks separados:
  - `/health` y `/health/live`: liveness.
  - `/health/ready`: readiness con RabbitMQ.
- GraphQL en `HotelLux.Gateway` en `/graphql`.

Reglas:

1. No refactorizar.
2. No eliminar gRPC todavia.
3. No cambiar Business salvo bug confirmado.
4. Si Docker Desktop no esta corriendo, reportar y detener la parte RabbitMQ sin inventar resultados.
5. Si la BD local no esta disponible, reportar exactamente que quedo pendiente.

Paso 1 - Estado inicial

Ejecutar:

```powershell
git status --short
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Resultado esperado:

- Build OK.
- Tests OK.

Paso 2 - Levantar RabbitMQ

Ejecutar:

```powershell
docker compose -f docker-compose.rabbitmq.yml up -d
powershell -ExecutionPolicy Bypass -File scripts\Test-RabbitMq.ps1
```

Resultado esperado:

- RabbitMQ Management responde en `http://localhost:15672`.
- Usuario/password local: `guest` / `guest`.

Si Docker falla:

- Reportar error exacto.
- No continuar con la prueba E2E RabbitMQ.
- Si es posible, continuar solo con Gateway GraphQL.

Paso 3 - Levantar servicios minimos

Levantar en terminales separadas, con `dotnet run --no-build --no-launch-profile`:

1. `HotelLux.Audit/HotelLux.Audit.API/HotelLux.Audit.API.csproj`
2. `HotelLux.Auth/HotelLux.Auth.API/HotelLux.Auth.API.csproj`
3. `HotelLux.Accommodation/HotelLux.Accommodation.API/HotelLux.Accommodation.API.csproj`
4. `HotelLux.Reservation/HotelLux.Reservation.API/HotelLux.Reservation.API.csproj`
5. `HotelLux.Gateway/HotelLux.Gateway.csproj`

Usar puertos por defecto si estan libres:

- Gateway: 5000
- Auth: 5001
- Accommodation: 5002
- Reservation: 5003
- Audit: 5008

Si algun puerto esta ocupado, usar `PORT` y `GRPC_PORT` temporales y documentarlo.

Paso 4 - Verificar health checks

Probar:

```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:5008/health" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5008/health/ready" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5002/health" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5002/health/ready" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5003/health" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5003/health/ready" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5000/health" -UseBasicParsing
```

Resultado esperado con RabbitMQ activo:

- `/health`: 200.
- `/health/ready`: 200 en servicios con RabbitMQ.
- Gateway `/health`: 200.

Paso 5 - Probar Gateway Swagger y GraphQL

Swagger:

```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:5000/swagger/v1/swagger.json" -UseBasicParsing
```

GraphQL introspection minima:

```powershell
$body = @{ query = "{ __typename }" } | ConvertTo-Json
Invoke-WebRequest -Uri "http://127.0.0.1:5000/graphql" -Method POST -ContentType "application/json" -Body $body -UseBasicParsing
```

Resultado esperado:

- Swagger JSON responde 200.
- GraphQL responde 200 y contiene `Query`.

Paso 6 - Probar que Audit consume desde RabbitMQ

La forma preferida es ejecutar una operacion real que ya publique auditoria. Usa la operacion mas simple disponible con datos seed locales.

Opciones sugeridas:

1. Si tienes JWT admin/vendedor:
   - Ejecutar una operacion interna sencilla en Accommodation, por ejemplo crear/actualizar un catalogo, tarifa o habitacion.

2. Si no tienes JWT:
   - Usar un endpoint publico que dispare auditoria, por ejemplo crear reserva publica si la BD y datos seed lo permiten.

3. Si ninguna operacion HTTP real es viable por falta de datos o JWT:
   - Crear temporalmente un pequeño proyecto/endpoint/manual test solo si es necesario, pero preferir no agregar codigo permanente.
   - Alternativa aceptable: usar una prueba manual de MassTransit desde un script o snippet que publique `AuditEventMessage` al broker, siempre que no quede codigo permanente innecesario.

Despues de emitir el evento:

1. Confirmar en RabbitMQ Management que la cola `hotellux.audit.events` existe.
2. Confirmar que no hay mensajes acumulados sin consumir.
3. Confirmar en logs de `HotelLux.Audit.API` que el consumidor recibio/proceso el evento.
4. Confirmar en BD Audit que existe un registro nuevo en `auditoria.evento_auditoria`.

Consulta SQL sugerida:

```sql
SELECT id_auditoria, auditoria_guid, servicio_origen, tabla_afectada, operacion, fecha_evento_utc
FROM auditoria.evento_auditoria
ORDER BY fecha_evento_utc DESC
LIMIT 10;
```

Paso 7 - Reporte final

Reportar:

- Build y tests: OK/fallo.
- RabbitMQ: OK/fallo.
- Servicios levantados: lista con puertos.
- Health checks:
  - `/health`
  - `/health/ready`
- Gateway:
  - Swagger OK/fallo.
  - GraphQL OK/fallo.
- Evento de auditoria:
  - Publisher usado.
  - Cola RabbitMQ.
  - Consumer Audit.
  - Registro en BD.
- Problemas encontrados.
- Cambios de codigo realizados, si hubo alguno.

Criterios de aceptacion:

- Build OK.
- Tests OK.
- RabbitMQ OK.
- `/health/ready` OK con RabbitMQ activo.
- Gateway GraphQL OK.
- Al menos un evento `AuditEventMessage` fue consumido por Audit.
- Si la BD esta disponible, el evento queda persistido.

---

## Prompt 11 - Corregir logging Windows EventLog para readiness fallido

Necesito corregir un problema runtime detectado al probar los health checks en Windows.

Contexto:
El build y los tests pasan. Gateway responde correctamente:

- `/health`: 200
- `/health/live`: 200
- `/health/ready`: 200
- `/graphql`: 200

Accommodation con RabbitMQ apagado responde:

- `/health`: 200
- `/health/live`: 200

Pero al consultar `/health/ready`, en vez de devolver un 503 limpio por RabbitMQ no disponible, la conexion termina de forma inesperada porque el logging intenta escribir al Windows EventLog y falla por permisos:

```text
System.AggregateException: An error occurred while writing to logger(s).
Cannot open log for source '.NET Runtime'. You may not have write access.
System.ComponentModel.Win32Exception (5): Acceso denegado.
```

Causa:
En Windows, el provider de EventLog puede estar registrado y lanzar excepciones al intentar escribir logs de errores/unhealthy checks si el usuario no tiene permisos sobre el EventLog. Esto rompe respuestas de error que deberian ser normales, como readiness 503.

Objetivo:
Evitar que ningun microservicio falle por intentar escribir al Windows EventLog. En Windows local, usar solo Console y Debug como providers de logging.

Solucion recomendada:

Crear una extension compartida en `HotelLux.Shared.Hosting` para configurar logging de forma consistente.

Archivo nuevo sugerido:

`HotelLux.Shared/Hosting/HotelLuxLoggingConfiguration.cs`

Contenido esperado:

```csharp
using Microsoft.Extensions.Logging;

namespace HotelLux.Shared.Hosting;

public static class HotelLuxLoggingConfiguration
{
    public static WebApplicationBuilder ConfigureHotelLuxLogging(this WebApplicationBuilder builder)
    {
        if (OperatingSystem.IsWindows())
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
        }

        return builder;
    }
}
```

Si `WebApplicationBuilder` necesita namespace adicional, agregar:

```csharp
using Microsoft.AspNetCore.Builder;
```

Aplicar en todos los `Program.cs` principales, justo despues de crear el builder:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.ConfigureHotelLuxLogging();
```

Servicios donde aplicar:

- `HotelLux.Auth/HotelLux.Auth.API/Program.cs`
- `HotelLux.Accommodation/HotelLux.Accommodation.API/Program.cs`
- `HotelLux.Reservation/HotelLux.Reservation.API/Program.cs`
- `HotelLux.Stay/HotelLux.Stay.API/Program.cs`
- `HotelLux.Finance/HotelLux.Finance.API/Program.cs`
- `HotelLux.Audit/HotelLux.Audit.API/Program.cs`
- `HotelLux.Gateway/Program.cs`

Importante:

1. Auth ya tiene un bloque parecido solo para Windows + Production. Reemplazarlo por la extension compartida para evitar duplicacion.
2. No cambiar niveles de logging en `appsettings.json` salvo que sea necesario.
3. No ocultar errores reales: Console/Debug deben seguir mostrando errores.
4. No cambiar health checks.
5. No cambiar MassTransit.
6. No tocar Business.

Verificacion obligatoria:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Verificacion runtime sin RabbitMQ:

Arrancar Accommodation en un puerto temporal:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:PORT='5062'
$env:GRPC_PORT='5062'
$env:Jwt__Key='HotelLuxemburgo_AccessSecret_MinimoTrentaYDosCaracteres_2026'
dotnet run --no-build --no-launch-profile --project HotelLux.Accommodation\HotelLux.Accommodation.API\HotelLux.Accommodation.API.csproj
```

En otra terminal:

```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:5062/health" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5062/health/live" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5062/health/ready" -UseBasicParsing
```

Resultado esperado sin RabbitMQ:

- `/health`: 200
- `/health/live`: 200
- `/health/ready`: 503
- No debe aparecer `Cannot open log for source '.NET Runtime'`.
- No debe terminar la conexion de forma inesperada.

Verificacion Gateway:

```powershell
$body = @{ query = "{ __typename }" } | ConvertTo-Json
Invoke-WebRequest -Uri "http://127.0.0.1:5000/graphql" -Method POST -ContentType "application/json" -Body $body -UseBasicParsing
```

Resultado esperado:

- GraphQL responde 200.

Reporte final requerido:

- Archivos modificados.
- Confirmar que EventLog ya no queda como provider activo en Windows local.
- Build/tests.
- Runtime Accommodation:
  - `/health`
  - `/health/live`
  - `/health/ready`
- Confirmar que readiness falla limpio con 503 cuando RabbitMQ esta apagado.

---

## Prompt 12 - Actualizar README, DEPLOY y scripts operativos para RabbitMQ/GraphQL

Necesito cerrar la parte operativa/documental despues de implementar RabbitMQ, bus de eventos, GraphQL, logging Windows y health checks.

Contexto validado:

- Build pasa.
- Tests pasan.
- Gateway responde:
  - `/health`: 200
  - `/health/live`: 200
  - `/health/ready`: 200
  - `/swagger/v1/swagger.json`: 200
  - `/graphql`: 200
- Accommodation sin RabbitMQ responde:
  - `/health`: 200
  - `/health/live`: 200
  - `/health/ready`: 503 limpio
- Cobertura estatica Gateway/YARP:
  - 117 endpoints publicos/locales en `docs/endpoints_publicas.txt` + `docs/endpoints_locales.txt`
  - 0 rutas sin cobertura en `HotelLux.Gateway/appsettings.json`
- Docker Desktop no esta corriendo en este entorno, por lo que el E2E RabbitMQ real queda pendiente hasta tener Docker activo.

Problema:
Algunos docs/scripts siguen describiendo el estado anterior:

- `README.md` todavia dice "Sin RabbitMQ/Kafka" y "Audit consumer gRPC".
- `README.md` menciona `audit.EmitAuditEvent` como flujo principal.
- `DEPLOY.md` todavia configura `AuditService__GrpcAddress` para publishers y no documenta `RabbitMq__...`.
- `scripts/Start-HotelLuxStack.ps1` solo levanta Auth, Accommodation, Reservation, Stay y Gateway; falta RabbitMQ, Audit y Finance.
- `docs/endpoints_locales.txt` describe auditoria como triggers de base de datos, pero ahora el camino principal es evento RabbitMQ -> Audit consumer -> BD.

Objetivo:
Actualizar documentacion y scripts para que reflejen la arquitectura actual:

- RabbitMQ como bus de eventos de auditoria.
- `HotelLux.Audit.API` como consumer RabbitMQ principal.
- gRPC de Audit queda solo como compatibilidad temporal.
- Gateway expone REST/YARP, Swagger y GraphQL.
- Health checks separados en liveness/readiness.
- Scripts locales deben levantar todos los servicios necesarios para probar endpoints publicos/locales y el bus.

Cambios requeridos:

### 1. README.md

Actualizar:

1. Tabla de arquitectura:
   - `HotelLux.Audit`: cambiar responsabilidad de "Consumer gRPC" a "Consumer RabbitMQ de eventos de auditoria; gRPC legacy temporal".
   - `HotelLux.Gateway`: mencionar YARP + Swagger + GraphQL.

2. Comunicacion entre servicios:
   - Mantener gRPC para comunicacion interna de negocio.
   - Agregar RabbitMQ para eventos de auditoria.
   - Eliminar/actualizar la frase "Sin RabbitMQ/Kafka".

3. Saga de reserva:
   - Cambiar `audit.EmitAuditEvent` por publicacion `AuditEventMessage` hacia RabbitMQ.

4. Decisiones de diseño:
   - Triggers eliminados.
   - Cada servicio publica `AuditEventMessage`.
   - `HotelLux.Audit.API` consume de RabbitMQ y persiste en `auditoria.evento_auditoria`.

5. Agregar seccion corta:
   - `GraphQL`
   - endpoint `/graphql`
   - se mantiene REST/YARP y Swagger.

6. Agregar seccion health:
   - `/health`: liveness
   - `/health/live`: liveness
   - `/health/ready`: readiness con RabbitMQ

### 2. DEPLOY.md

Actualizar variables de entorno.

1. Agregar una seccion "RabbitMQ / broker".

2. Documentar variables comunes para todos los servicios que publican/consumen eventos:

```text
RabbitMq__Host        =
RabbitMq__VirtualHost =
RabbitMq__Username    =
RabbitMq__Password    =
RabbitMq__AuditQueue  = hotellux.audit.events
```

3. En Render:
   - Indicar que se necesita un broker RabbitMQ gestionado externo, porque Render web services no provee RabbitMQ persistente por defecto.
   - Sugerir CloudAMQP u otro RabbitMQ administrado.
   - No inventar credenciales reales.

4. Para servicios:
   - Audit requiere `RabbitMq__...` y `ConnectionStrings__AuditDb`.
   - Auth/Accommodation/Reservation/Stay/Finance requieren `RabbitMq__...` como publishers.

5. Remover o marcar como legacy temporal los `AuditService__GrpcAddress` de los publishers.
   - No borrarlos si se quiere documentar fallback, pero dejar claro que no son el camino principal.

6. Gateway:
   - Agregar que expone `/graphql`.
   - Mantener variables `ReverseProxy__Clusters__...`.

7. Verificacion deploy:
   - `/health` para liveness.
   - `/health/ready` para readiness con RabbitMQ.
   - `/graphql` query `{ __typename }`.

### 3. docs/endpoints_locales.txt

Actualizar solo el texto descriptivo de Auditoria:

De:

```text
Auditoría — solo lectura. Los registros se insertan automáticamente mediante triggers de base de datos.
```

A algo equivalente a:

```text
Auditoría — solo lectura. Los registros se insertan desde eventos de auditoría consumidos por HotelLux.Audit.API desde RabbitMQ.
```

No cambiar contratos ni schemas salvo que sea necesario.

### 4. scripts/Start-HotelLuxStack.ps1

Actualizar script para el stack actual.

Debe:

1. Intentar levantar RabbitMQ usando `docker-compose.rabbitmq.yml` si Docker esta disponible.
   - Si Docker no esta corriendo, mostrar warning claro y continuar o detener segun parametro.

2. Agregar parametro:

```powershell
[switch]$SkipRabbitMq
```

3. Agregar parametro:

```powershell
[switch]$RequireRabbitMq
```

4. Levantar servicios en este orden:
   - RabbitMQ (si no se omite)
   - Audit
   - Auth
   - Finance
   - Accommodation
   - Reservation
   - Stay
   - Gateway

5. Revisar puertos:
   - 5000, 5001, 5002, 5003, 5004, 5005, 5008
   - 5672, 15672 si RabbitMQ no se omite

6. Mantener `dotnet build HotelLux.Stack.slnx -v minimal` antes de levantar servicios.

7. Al final imprimir URLs utiles:
   - Gateway `/health`
   - Gateway `/swagger`
   - Gateway `/graphql`
   - RabbitMQ Management `http://localhost:15672`
   - Readiness examples:
     - `http://127.0.0.1:5008/health/ready`
     - `http://127.0.0.1:5002/health/ready`

8. No hacer acciones destructivas.

### 5. scripts/Test-GatewayHealth.ps1

Actualizar opcionalmente para mostrar:

- `/health`
- `/health/live`
- `/health/ready`
- `/graphql` con `{ __typename }`

No debe depender de microservicios internos para pasar, salvo que se agregue parametro `-RequireReady`.

### 6. docs/rabbitmq_event_bus.md

Revisar y asegurar que:

- menciona `/health/ready`
- menciona que Docker Desktop debe estar corriendo
- menciona que `AuditGrpcService` queda como compatibilidad temporal
- menciona que deploy requiere broker RabbitMQ externo

Verificacion obligatoria:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Verificacion Gateway minima:

Levantar Gateway solo y probar:

```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:5000/health" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5000/health/live" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5000/health/ready" -UseBasicParsing
$body = @{ query = "{ __typename }" } | ConvertTo-Json
Invoke-WebRequest -Uri "http://127.0.0.1:5000/graphql" -Method POST -ContentType "application/json" -Body $body -UseBasicParsing
```

Reporte final requerido:

- Archivos modificados.
- Build/tests.
- Confirmar que README ya no dice "Sin RabbitMQ/Kafka".
- Confirmar que DEPLOY documenta `RabbitMq__...`.
- Confirmar que `Start-HotelLuxStack.ps1` levanta Audit y Finance ademas de los servicios anteriores.
- Confirmar que GraphQL queda documentado.
- Si Docker no esta activo, indicar que RabbitMQ E2E queda pendiente hasta encender Docker Desktop.

## Prompt 13 - Separar estrategia local Docker y produccion Render para RabbitMQ

Necesito que ajustes el proyecto para que quede clara y correctamente implementada la estrategia final de ambientes:

- En local/testing se usa RabbitMQ con Docker mediante `docker-compose.rabbitmq.yml`.
- En produccion en Render NO se asume Docker local ni `localhost`; los microservicios se conectan a un broker RabbitMQ externo/gestionado por variables de entorno.

No cambies la logica Business. No elimines `AuditGrpcService` todavia; sigue como compatibilidad temporal. El camino principal de auditoria debe seguir siendo RabbitMQ.

### Contexto tecnico actual

Ya existen:

- `AuditEventMessage` en `HotelLux.Shared`.
- Configuracion RabbitMQ/MassTransit en `HotelLux.Shared/Messaging`.
- Publishers RabbitMQ en Auth, Accommodation, Reservation, Stay y Finance.
- Consumer RabbitMQ en `HotelLux.Audit.API`.
- GraphQL Gateway en `/graphql`.
- Health checks:
  - `/health` y `/health/live`: liveness.
  - `/health/ready`: readiness con RabbitMQ cuando aplique.
- Scripts:
  - `docker-compose.rabbitmq.yml`
  - `scripts/Start-RabbitMq.ps1`
  - `scripts/Test-RabbitMq.ps1`

La decision arquitectonica final es:

```text
Local / pruebas:
Docker Desktop + docker-compose.rabbitmq.yml

Produccion en Render:
Microservicios .NET en Render + broker RabbitMQ externo gestionado
Ejemplo recomendado: CloudAMQP u otro proveedor compatible con AMQP/RabbitMQ
```

### Objetivo

Dejar configurado y documentado el proyecto para que nadie confunda Docker local con produccion. Docker debe quedar como herramienta de desarrollo/pruebas, no como requisito de produccion de los microservicios.

### Cambios requeridos

#### 1. README.md

Actualizar la documentacion principal para indicar:

- El backend usa RabbitMQ como bus de eventos de auditoria.
- Docker se usa localmente para levantar RabbitMQ de pruebas.
- En Render/produccion se debe usar un broker RabbitMQ externo o gestionado.
- No usar `localhost` para RabbitMQ en Render.
- `HotelLux.Audit.API` consume `AuditEventMessage` desde RabbitMQ y persiste en auditoria.
- El Gateway expone REST/Swagger y GraphQL en `/graphql`.
- Health checks:
  - `/health`: proceso HTTP vivo.
  - `/health/live`: liveness.
  - `/health/ready`: dependencias listas, incluyendo RabbitMQ en servicios que lo usan.

Eliminar o corregir cualquier frase desactualizada como:

- "Sin RabbitMQ/Kafka"
- "Sin Docker durante desarrollo"
- "Auditoria solo por gRPC"

#### 2. DEPLOY.md

Actualizar la seccion de Render para produccion:

1. Agregar una seccion clara: `RabbitMQ en produccion`.

2. Explicar que hay dos opciones:

   - Recomendada: broker gestionado externo, por ejemplo CloudAMQP.
   - Alternativa: RabbitMQ desplegado como servicio propio en Render con Docker y disco persistente, solo si el equipo quiere operar el broker.

3. Dejar como recomendacion principal:

```text
Render:
- Desplegar Auth, Accommodation, Reservation, Stay, Finance, Audit y Gateway.
- Configurar RabbitMq__... en cada servicio publisher y en Audit.
- Usar un host real de RabbitMQ, no localhost.
```

4. Documentar variables obligatorias:

```text
RabbitMq__Host
RabbitMq__VirtualHost
RabbitMq__Username
RabbitMq__Password
RabbitMq__AuditQueue=hotellux.audit.events
```

5. Indicar donde van:

- `HotelLux.Audit.API`: requiere `RabbitMq__...` y `ConnectionStrings__AuditDb`.
- `HotelLux.Auth.API`: requiere `RabbitMq__...` para publicar auditoria.
- `HotelLux.Accommodation.API`: requiere `RabbitMq__...`.
- `HotelLux.Reservation.API`: requiere `RabbitMq__...`.
- `HotelLux.Stay.API`: requiere `RabbitMq__...`.
- `HotelLux.Finance.API`: requiere `RabbitMq__...`.
- `HotelLux.Gateway`: no requiere RabbitMQ; si requiere URLs de servicios y JWT segun configuracion actual.

6. Marcar `AuditService__GrpcAddress` como legacy temporal si aparece todavia para publishers. No debe presentarse como camino principal.

7. Agregar validacion post-deploy:

```powershell
# Gateway
GET https://<gateway>.onrender.com/health
GET https://<gateway>.onrender.com/health/ready
POST https://<gateway>.onrender.com/graphql

# Servicios con RabbitMQ
GET https://<audit>.onrender.com/health/ready
GET https://<accommodation>.onrender.com/health/ready
GET https://<reservation>.onrender.com/health/ready
```

8. Explicar resultados esperados:

- Si RabbitMQ esta bien configurado: `/health/ready` devuelve 200.
- Si RabbitMQ esta mal configurado o inaccesible: `/health/ready` devuelve 503.
- `/health` puede seguir devolviendo 200 aunque RabbitMQ este caido, porque es liveness.

#### 3. docs/rabbitmq_event_bus.md

Actualizar este documento para dividirlo en dos secciones:

##### Local/testing con Docker

Debe incluir:

```powershell
docker compose -f docker-compose.rabbitmq.yml up -d
powershell -ExecutionPolicy Bypass -File scripts\Test-RabbitMq.ps1
```

Variables locales esperadas:

```text
RabbitMq__Host=localhost
RabbitMq__VirtualHost=/
RabbitMq__Username=guest
RabbitMq__Password=guest
RabbitMq__AuditQueue=hotellux.audit.events
```

##### Produccion en Render

Debe incluir:

- No usar Docker Desktop.
- No usar `localhost`.
- Configurar un broker RabbitMQ externo/gestionado.
- Cargar variables `RabbitMq__...` en Render.
- Validar con `/health/ready`.

#### 4. scripts/Start-HotelLuxStack.ps1

Actualizar el script para que refleje el uso local:

1. Agregar parametros:

```powershell
[switch]$SkipRabbitMq
[switch]$RequireRabbitMq
```

2. Si `SkipRabbitMq` no esta activo, intentar levantar RabbitMQ con:

```powershell
docker compose -f docker-compose.rabbitmq.yml up -d
```

3. Si Docker no esta disponible:

- Con `RequireRabbitMq`: fallar con error claro.
- Sin `RequireRabbitMq`: mostrar warning y continuar levantando servicios, dejando claro que `/health/ready` puede devolver 503.

4. Levantar los servicios en este orden:

```text
RabbitMQ local opcional
Audit
Auth
Finance
Accommodation
Reservation
Stay
Gateway
```

5. Revisar puertos:

```text
5000 Gateway
5001 Auth
5002 Accommodation
5003 Reservation
5004 Stay
5005 Finance
5008 Audit
5672 RabbitMQ AMQP
15672 RabbitMQ Management
```

Si el repo usa otros puertos reales en `appsettings`, respetar los puertos existentes del proyecto, pero documentarlos correctamente en el output del script.

6. Al final imprimir:

```text
Gateway health: http://127.0.0.1:5000/health
Gateway ready:  http://127.0.0.1:5000/health/ready
Gateway GraphQL: http://127.0.0.1:5000/graphql
RabbitMQ UI local: http://localhost:15672
Audit ready: http://127.0.0.1:5008/health/ready
Accommodation ready: http://127.0.0.1:5002/health/ready
Reservation ready: http://127.0.0.1:5003/health/ready
```

#### 5. scripts/Test-GatewayHealth.ps1

Actualizar para probar:

- `/health`
- `/health/live`
- `/health/ready`
- `/graphql` con `{ __typename }`

Agregar parametro opcional:

```powershell
[switch]$RequireReady
```

Comportamiento:

- Sin `RequireReady`: si `/health/ready` falla, mostrar warning y no fallar todo el script.
- Con `RequireReady`: si `/health/ready` no devuelve 200, fallar.
- `/graphql` debe devolver 200 con `{ __typename }`.

#### 6. appsettings.json

Revisar los `appsettings.json` de servicios que usan RabbitMQ:

- Auth
- Accommodation
- Reservation
- Stay
- Finance
- Audit

Validar que tengan valores locales razonables:

```json
"RabbitMq": {
  "Host": "localhost",
  "VirtualHost": "/",
  "Username": "guest",
  "Password": "guest",
  "AuditQueue": "hotellux.audit.events"
}
```

No poner credenciales de produccion en archivos. Produccion debe usar variables de entorno de Render.

#### 7. GraphQL / Gateway

No ampliar schema en este prompt. Solo asegurar que la documentacion de Render indique que:

- Gateway expone `/graphql`.
- GraphQL reenvia Authorization hacia servicios internos segun implementacion actual.
- Gateway no necesita `RabbitMq__...`.

### Verificacion obligatoria

Ejecutar:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Verificacion documental:

```powershell
rg -n "Sin RabbitMQ|Sin Docker|AuditService__GrpcAddress|RabbitMq__Host|CloudAMQP|/graphql|/health/ready" README.md DEPLOY.md docs scripts
```

La verificacion debe confirmar:

- README ya no dice "Sin RabbitMQ/Kafka".
- README ya no dice "Sin Docker durante desarrollo" como regla actual.
- DEPLOY documenta `RabbitMq__...`.
- DEPLOY recomienda broker gestionado externo para Render.
- `AuditService__GrpcAddress` no aparece como camino principal de auditoria.
- `docs/rabbitmq_event_bus.md` diferencia local Docker vs Render produccion.
- `Start-HotelLuxStack.ps1` incluye Audit y Finance.
- `Test-GatewayHealth.ps1` prueba GraphQL.

Si Docker Desktop esta encendido, tambien ejecutar:

```powershell
docker compose -f docker-compose.rabbitmq.yml up -d
powershell -ExecutionPolicy Bypass -File scripts\Test-RabbitMq.ps1
```

Si Docker Desktop no esta encendido, reportar claramente:

```text
Docker Desktop no esta corriendo; no se ejecuto E2E RabbitMQ local.
```

### Reporte final requerido

Responder con:

- Archivos modificados.
- Que estrategia quedo documentada:
  - local/testing = Docker RabbitMQ
  - produccion Render = broker RabbitMQ externo/gestionado
- Resultado de build.
- Resultado de tests.
- Resultado de verificacion documental.
- Resultado de Docker/RabbitMQ local si se pudo probar.
  Si no se pudo, decir que queda pendiente hasta encender Docker Desktop.

## Prompt 14 - Corregir smoke test GraphQL de Gateway y verificarlo en runtime

Necesito que corrijas un bug puntual en `scripts/Test-GatewayHealth.ps1` y lo verifiques en runtime.

### Contexto

Despues de los prompts 12 y 13, el script `scripts/Test-GatewayHealth.ps1` prueba:

- `/health`
- `/health/live`
- `/health/ready`
- `/graphql` con `{ __typename }`

Pero actualmente contiene esta logica:

```powershell
$content = [System.Text.Encoding]::UTF8.GetString($gql.Content)
```

En Windows PowerShell, `Invoke-WebRequest` normalmente devuelve `.Content` como `string`, no como `byte[]`. Entonces esa linea puede fallar aunque GraphQL responda 200 correctamente.

### Objetivo

Hacer que `scripts/Test-GatewayHealth.ps1` sea robusto en Windows PowerShell y PowerShell 7, sin cambiar el backend.

### Cambios requeridos

1. Actualizar `scripts/Test-GatewayHealth.ps1`.

2. Reemplazar la conversion directa con una funcion o bloque robusto:

```powershell
function Get-ResponseText {
    param([Parameter(Mandatory)]$Response)

    if ($Response.Content -is [byte[]]) {
        return [System.Text.Encoding]::UTF8.GetString($Response.Content)
    }

    return [string]$Response.Content
}
```

3. Para GraphQL:

   - Obtener contenido con `Get-ResponseText`.
   - Validar que el JSON contenga `data.__typename = "Query"`.
   - Preferir `ConvertFrom-Json` en vez de solo regex cuando sea posible.
   - Si el JSON no es valido o no contiene `Query`, lanzar error claro con el contenido recibido.

Ejemplo aceptable:

```powershell
$content = Get-ResponseText -Response $gql
try {
    $json = $content | ConvertFrom-Json
} catch {
    throw "GraphQL devolvio contenido no JSON: $content"
}

if ($json.data.__typename -ne "Query") {
    throw "GraphQL no devolvio __typename Query: $content"
}
```

4. No cambiar rutas ni comportamiento de `-RequireReady`.

5. No tocar GraphQL schema ni Gateway code si no es necesario.

### Verificacion obligatoria

Ejecutar:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Luego levantar solo Gateway en Development en puerto 5000 o en un puerto libre si 5000 esta ocupado:

```powershell
dotnet run --no-build --project HotelLux.Gateway\HotelLux.Gateway.csproj
```

Probar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-GatewayHealth.ps1
```

Resultado esperado:

- `/health`: 200
- `/health/live`: 200
- `/health/ready`: 200 en Gateway
- `/graphql`: 200
- El script imprime algo equivalente a `OK: GraphQL -> 200` sin error de `Encoding.GetString`.

Si el puerto 5000 esta ocupado:

- Levantar Gateway con `PORT=<puerto_libre>` o variable equivalente en PowerShell.
- Ejecutar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-GatewayHealth.ps1 -GatewayBaseUrl "http://127.0.0.1:<puerto_libre>"
```

### Verificacion adicional

Confirmar que este comando ya no encuentra la conversion incorrecta:

```powershell
rg -n "Encoding\\]::UTF8\\.GetString\\(\\$gql\\.Content\\)" scripts\Test-GatewayHealth.ps1
```

Debe devolver 0 resultados.

### Reporte final requerido

Responder con:

- Archivo modificado.
- Build/tests.
- Resultado de `scripts/Test-GatewayHealth.ps1`.
- Confirmar que GraphQL se valido parseando JSON y `data.__typename == "Query"`.
- Confirmar que no se tocaron backend/schema ni RabbitMQ.

## Prompt 15 - Validacion E2E local RabbitMQ -> Audit -> BD y endpoints Gateway

Necesito que hagas una validacion E2E local completa de la implementacion actual. Este prompt es principalmente de verificacion; no hagas cambios de codigo salvo que encuentres un error real y pequeño que bloquee la prueba.

### Objetivo

Validar que:

- RabbitMQ levanta con Docker local.
- Los servicios que usan MassTransit conectan al broker.
- Un publisher real publica `AuditEventMessage`.
- `HotelLux.Audit.API` consume desde la cola `hotellux.audit.events`.
- El evento queda persistido en `auditoria.evento_auditoria`.
- Gateway sigue respondiendo REST/Swagger/GraphQL.
- Endpoints publicos y locales siguen cubiertos por Gateway.

### Requisitos antes de empezar

1. Docker Desktop debe estar corriendo.
2. PostgreSQL local debe estar disponible con las bases/schemas del proyecto.
3. No usar credenciales de produccion.
4. No usar Render para esta prueba; esto es local/testing.
5. No tocar la estrategia documentada:

```text
local/testing = Docker RabbitMQ
produccion Render = broker RabbitMQ externo/gestionado
```

Si Docker Desktop no esta corriendo:

- Reportar claramente que no se puede ejecutar el E2E RabbitMQ local.
- No inventar resultados.
- Aun asi ejecutar build/tests y smoke de Gateway si es posible.

### Paso 1 - Verificacion base

Ejecutar:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Resultado esperado:

- Build: 0 errores.
- Reservation tests: 5/5.
- Finance tests: 4/4.

Si aparece bloqueo de `VBCSCompiler` o archivo en `obj`, ejecutar:

```powershell
dotnet build-server shutdown
```

y repetir el build. Reportar que era bloqueo de compilador compartido, no error de codigo.

### Paso 2 - Levantar RabbitMQ local

Ejecutar:

```powershell
docker info
docker compose -f docker-compose.rabbitmq.yml up -d
powershell -ExecutionPolicy Bypass -File scripts\Test-RabbitMq.ps1
```

Resultado esperado:

- RabbitMQ Management responde en `http://localhost:15672`.
- AMQP disponible en `localhost:5672`.

### Paso 3 - Levantar servicios minimos para E2E

Levantar como minimo:

1. `HotelLux.Audit.API`
2. Un publisher real. Preferido: `HotelLux.Auth.API` si existe un login/operacion sencilla con seed local. Alternativa: `HotelLux.Accommodation.API` con una operacion de catalogo/administracion que publique auditoria.
3. `HotelLux.Gateway`

Si la prueba elegida requiere mas servicios, levantarlos explicitamente.

Usar puertos locales del proyecto:

```text
Gateway: 5000
Auth: 5001
Accommodation: 5002
Reservation: 5003
Stay: 5004
Finance: 5005
Audit: 5008
RabbitMQ: 5672 / 15672
```

Validar readiness:

```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:5008/health/ready" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5001/health/ready" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5000/health/ready" -UseBasicParsing
```

Si se usa otro publisher, validar su `/health/ready`.

Resultado esperado con RabbitMQ activo:

- Servicios con MassTransit: `/health/ready` -> 200.
- Gateway: `/health/ready` -> 200.

### Paso 4 - Ejecutar operacion que publique auditoria

Primero inspeccionar rapidamente el controller/seed del publisher elegido para usar una operacion real y segura.

Opcion recomendada si Auth tiene seed usable:

1. Hacer login exitoso por Gateway o directo a Auth.
2. Confirmar que el publisher registra publicacion de auditoria.
3. Confirmar que Audit consume el evento.

Si Auth no tiene seed claro, usar una operacion en Accommodation/Reservation/Finance/Stay que ya publique auditoria y sea reversible o no destructiva para datos de prueba.

No inventes payloads. Lee los DTOs/controllers y usa un request valido.

### Paso 5 - Verificar RabbitMQ

Usar RabbitMQ Management o API HTTP si es viable para comprobar la cola:

- La cola `hotellux.audit.events` existe.
- Hay actividad de publish/consume.
- Idealmente no quedan mensajes acumulados despues de que Audit consuma.

Si no usas la API HTTP, reportar la verificacion manual esperada en la UI.

### Paso 6 - Verificar persistencia en Audit DB

Consultar PostgreSQL local y confirmar que existe al menos un registro nuevo en:

```text
auditoria.evento_auditoria
```

La consulta debe revisar campos relevantes:

- `servicio_origen`
- `tabla_afectada`
- `operacion`
- `fecha_evento_utc`
- `usuario_ejecutor` si aplica

Si no hay acceso CLI a PostgreSQL, reportar el query exacto para pgAdmin y no inventar resultados.

Query sugerido:

```sql
SELECT auditoria_guid,
       servicio_origen,
       tabla_afectada,
       operacion,
       usuario_ejecutor,
       fecha_evento_utc
FROM auditoria.evento_auditoria
ORDER BY fecha_evento_utc DESC
LIMIT 10;
```

### Paso 7 - Validar Gateway

Ejecutar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-GatewayHealth.ps1 -RequireReady
```

Validar manualmente:

```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:5000/swagger/v1/swagger.json" -UseBasicParsing
```

Y confirmar GraphQL:

```powershell
$body = @{ query = "{ __typename }" } | ConvertTo-Json
Invoke-WebRequest -Uri "http://127.0.0.1:5000/graphql" -Method POST -ContentType "application/json" -Body $body -UseBasicParsing
```

### Paso 8 - Cobertura endpoints publicos/locales

Repetir la verificacion estatica de rutas Gateway contra:

- `docs/endpoints_publicas.txt`
- `docs/endpoints_locales.txt`
- `HotelLux.Gateway/appsettings.json`

Confirmar:

```text
MISSING=0
```

Si ya existe script para esto, usarlo. Si no existe, hacer una verificacion razonable con parsing JSON/PowerShell, sin editar codigo salvo que sea necesario.

### Reporte final requerido

Responder con:

- Build/tests.
- Estado Docker/RabbitMQ.
- Servicios levantados.
- Readiness por servicio.
- Operacion usada para generar auditoria.
- Evidencia de RabbitMQ:
  - cola existe
  - publish/consume observado
- Evidencia de BD:
  - registro nuevo en `auditoria.evento_auditoria` o query exacto si no se pudo consultar.
- Gateway:
  - `/health`
  - `/health/live`
  - `/health/ready`
  - `/graphql`
  - Swagger
- Cobertura endpoints:
  - `MISSING=0` o lista concreta de faltantes.
- Si algo no se pudo probar, explicar el bloqueo exacto y el siguiente paso.

### Criterio de aceptacion

Se considera completado solo si:

- RabbitMQ local esta activo.
- Al menos un evento real llega al bus.
- Audit lo consume.
- La BD de auditoria muestra el evento.
- Gateway sigue sano.

Si Docker o PostgreSQL local no estan disponibles, el prompt queda parcialmente completado y debe decirlo claramente.

## Prompt 16 - Hardening RabbitMQ para Render: URI, puerto y TLS/AMQPS

Necesito que prepares la configuracion RabbitMQ para produccion en Render con brokers gestionados como CloudAMQP, sin romper el flujo local ya validado con Docker.

### Contexto

La validacion E2E local ya fue exitosa:

- RabbitMQ local con Docker activo.
- Login real en `HotelLux.Auth.API`.
- Publish hacia `hotellux.audit.events`.
- `HotelLux.Audit.API` consumio.
- Registro persistido en `auditoria.evento_auditoria`.
- Gateway `/health`, `/health/live`, `/health/ready`, `/graphql` y Swagger OK.
- Cobertura Gateway/YARP: `MISSING=0`.

El problema pendiente es de produccion:

- `HotelLux.Shared/Messaging/RabbitMqSettings.cs` hoy solo soporta:
  - `Host`
  - `VirtualHost`
  - `Username`
  - `Password`
  - `AuditQueue`
- `RabbitMqConfigurationExtensions.ConfigureRabbitMqHost` usa `cfg.Host(settings.Host, settings.VirtualHost, ...)`.
- Para Render + CloudAMQP u otros brokers gestionados, normalmente se entrega una URL completa `amqp://...` o `amqps://...`, y puede requerirse puerto/TLS.

No cambies la arquitectura. No cambies publishers/consumer salvo que sea estrictamente necesario para usar la configuracion compartida.

### Objetivo

Soportar estos escenarios:

1. Local Docker actual, sin cambios obligatorios:

```text
RabbitMq__Host=localhost
RabbitMq__VirtualHost=/
RabbitMq__Username=guest
RabbitMq__Password=guest
RabbitMq__AuditQueue=hotellux.audit.events
```

2. Produccion con variables separadas:

```text
RabbitMq__Host=<host>
RabbitMq__Port=5672
RabbitMq__VirtualHost=<vhost>
RabbitMq__Username=<usuario>
RabbitMq__Password=<password>
RabbitMq__UseSsl=false
RabbitMq__AuditQueue=hotellux.audit.events
```

3. Produccion con TLS/AMQPS:

```text
RabbitMq__Host=<host>
RabbitMq__Port=5671
RabbitMq__VirtualHost=<vhost>
RabbitMq__Username=<usuario>
RabbitMq__Password=<password>
RabbitMq__UseSsl=true
RabbitMq__AuditQueue=hotellux.audit.events
```

4. Produccion con URL completa del proveedor:

```text
RabbitMq__Uri=amqp://user:pass@host:5672/vhost
```

o:

```text
RabbitMq__Uri=amqps://user:pass@host:5671/vhost
```

Tambien aceptar `CLOUDAMQP_URL` como fallback si `RabbitMq__Uri` no esta configurado.

### Cambios requeridos

#### 1. RabbitMqSettings

Actualizar `HotelLux.Shared/Messaging/RabbitMqSettings.cs` para agregar:

```csharp
public string? Uri { get; set; }
public int? Port { get; set; }
public bool UseSsl { get; set; }
```

Mantener defaults locales:

```csharp
Host = "localhost"
VirtualHost = "/"
Username = "guest"
Password = "guest"
AuditQueue = "hotellux.audit.events"
```

No guardar credenciales productivas.

#### 2. RabbitMqConfigurationExtensions

Actualizar `HotelLux.Shared/Messaging/RabbitMqConfigurationExtensions.cs`.

Requisitos:

1. `GetRabbitMqSettings` debe:
   - Bindear la seccion `RabbitMq`.
   - Si `settings.Uri` esta vacio, leer `CLOUDAMQP_URL` desde environment.
   - No pisar valores locales si no existe `CLOUDAMQP_URL`.

2. `ConfigureRabbitMqHost` debe soportar:

   - Si `settings.Uri` existe:
     - Parsear con `System.Uri`.
     - Aceptar esquemas `amqp`, `amqps` y, si MassTransit lo requiere, convertir de forma compatible a `rabbitmq`/`rabbitmqs` o usar el overload soportado por MassTransit 8.3.4.
     - Extraer usuario/password si vienen en la URL.
     - Extraer virtual host desde `AbsolutePath`, cuidando `/`.
     - Inferir `UseSsl=true` para `amqps`.
     - Usar puerto si viene en la URL; si no, inferir 5671 para `amqps` y 5672 para `amqp`.

   - Si no hay `Uri`:
     - Usar `Host`, `Port`, `VirtualHost`, `Username`, `Password`, `UseSsl`.
     - Si `Port` es null, usar 5671 si `UseSsl=true`, caso contrario 5672.

3. Revisar la API real de MassTransit 8.3.4 disponible en el proyecto antes de elegir overloads.

4. No loguear passwords ni URLs completas con credenciales.

5. Mantener compatibilidad local:

```text
docker-compose.rabbitmq.yml + appsettings actuales deben seguir funcionando sin agregar Port/UseSsl/Uri.
```

#### 3. appsettings.json locales

Revisar los `appsettings.json` de:

- Auth
- Accommodation
- Reservation
- Stay
- Finance
- Audit

Opcionalmente agregar solo valores no sensibles:

```json
"Port": 5672,
"UseSsl": false
```

No agregar `Uri` en archivos locales salvo que quede vacio y sea claramente documental. Preferible no agregarlo.

#### 4. Documentacion

Actualizar:

- `README.md`
- `DEPLOY.md`
- `docs/rabbitmq_event_bus.md`

Documentar dos maneras de configurar Render:

##### Opcion A recomendada: URL completa

```text
RabbitMq__Uri=amqps://user:password@host:5671/vhost
RabbitMq__AuditQueue=hotellux.audit.events
```

o usando variable del proveedor:

```text
CLOUDAMQP_URL=amqps://user:password@host:5671/vhost
RabbitMq__AuditQueue=hotellux.audit.events
```

##### Opcion B: variables separadas

```text
RabbitMq__Host=<host>
RabbitMq__Port=5671
RabbitMq__VirtualHost=<vhost>
RabbitMq__Username=<usuario>
RabbitMq__Password=<password>
RabbitMq__UseSsl=true
RabbitMq__AuditQueue=hotellux.audit.events
```

Indicar:

- `amqp` normalmente usa 5672.
- `amqps` normalmente usa 5671.
- En Render no usar `localhost`.
- Gateway no necesita `RabbitMq__...`.

#### 5. Tests unitarios o de bajo costo

Si existe proyecto de tests adecuado para `HotelLux.Shared`, agregar tests para parsing/configuracion.

Si no existe y crear uno es demasiado grande, al menos agregar una prueba pequeña o validacion interna de bajo riesgo donde corresponda.

Casos a cubrir de alguna forma:

- Sin `Uri`: `Host=localhost`, `Port=null`, `UseSsl=false` => puerto efectivo 5672.
- `UseSsl=true`, `Port=null` => puerto efectivo 5671.
- `RabbitMq__Uri=amqp://user:pass@example.com:5672/myvhost`.
- `RabbitMq__Uri=amqps://user:pass@example.com:5671/myvhost`.
- `CLOUDAMQP_URL` funciona si `RabbitMq__Uri` no existe.

No sobrecomplicar con secretos reales.

### Verificacion obligatoria

Ejecutar:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Si agregaste tests nuevos, ejecutarlos tambien.

Verificacion local RabbitMQ, si Docker Desktop esta corriendo:

```powershell
docker compose -f docker-compose.rabbitmq.yml up -d
powershell -ExecutionPolicy Bypass -File scripts\Test-RabbitMq.ps1
```

Levantar al menos `HotelLux.Audit.API` y un publisher con configuracion local default y confirmar:

```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:5008/health/ready" -UseBasicParsing
```

debe devolver 200 si RabbitMQ local esta activo.

Verificacion de smoke Gateway:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-GatewayHealth.ps1
```

### Reporte final requerido

Responder con:

- Archivos modificados.
- Forma final de configurar local Docker.
- Forma final de configurar Render con `RabbitMq__Uri` o `CLOUDAMQP_URL`.
- Forma alternativa con variables separadas.
- Build/tests.
- Resultado de RabbitMQ local si Docker estaba disponible.
- Confirmar que el E2E local previo no se rompe.
- Confirmar que no se imprimen secretos en logs/docs.

### Criterio de aceptacion

Se considera listo si:

- Local Docker sigue funcionando con configuracion actual.
- Render puede configurarse con URL `amqp://` o `amqps://`.
- Render puede configurarse con variables separadas incluyendo `Port` y `UseSsl`.
- `CLOUDAMQP_URL` es aceptado como fallback.
- Build/tests pasan.

## Prompt 17 - Integrar tests nuevos al flujo estándar de verificación

Necesito cerrar un detalle operativo despues del hardening de RabbitMQ para Render.

### Contexto

Prompt 16 agrego:

- `HotelLux.Shared/Messaging/RabbitMqConnectionResolver.cs`
- Nuevos tests en `tests/HotelLux.Shared.Tests`
- 6 tests para parsing/configuracion RabbitMQ

La verificacion manual ya paso:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Shared.Tests\HotelLux.Shared.Tests.csproj -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

Pero `HotelLux.Stack.slnx` actualmente solo incluye proyectos API, no incluye los proyectos de tests. Esto puede hacer que el equipo olvide ejecutar `HotelLux.Shared.Tests` cuando toque RabbitMQ.

### Objetivo

Crear un flujo de verificacion unico, facil de ejecutar, que incluya:

- Build del stack.
- Tests Shared.
- Tests Reservation.
- Tests Finance.
- Smoke opcional de Gateway.

No cambiar logica de negocio, RabbitMQ, GraphQL ni schema.

### Cambios requeridos

#### 1. Revisar solución/proyectos

Revisar `HotelLux.Stack.slnx`.

Si el formato `.slnx` permite incluir proyectos de tests sin romper el uso actual, agregar:

```xml
<Project Path="tests/HotelLux.Shared.Tests/HotelLux.Shared.Tests.csproj" />
<Project Path="tests/HotelLux.Reservation.Business.Tests/HotelLux.Reservation.Business.Tests.csproj" />
<Project Path="tests/HotelLux.Finance.Business.Tests/HotelLux.Finance.Business.Tests.csproj" />
```

Si por criterio del proyecto prefieres mantener `.slnx` solo para APIs, no lo modifiques, pero explica el motivo en el reporte final.

#### 2. Crear script central de verificación

Crear:

```text
scripts/Test-Backend.ps1
```

Debe:

1. Ubicarse en la raiz del repo.
2. Ejecutar:

```powershell
dotnet build HotelLux.Stack.slnx -v minimal
dotnet test tests\HotelLux.Shared.Tests\HotelLux.Shared.Tests.csproj -v minimal
dotnet test tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj -v minimal
dotnet test tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj -v minimal
```

3. Aceptar parametros:

```powershell
[switch]$NoRestore
[switch]$GatewaySmoke
[string]$GatewayBaseUrl = "http://127.0.0.1:5000"
```

4. Si `NoRestore` esta activo, pasar `--no-restore` a los `dotnet test`.

5. Si `GatewaySmoke` esta activo, ejecutar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-GatewayHealth.ps1 -GatewayBaseUrl $GatewayBaseUrl
```

6. Fallar con exit code distinto de 0 si build o cualquier test falla.

7. Imprimir un resumen claro al final:

```text
Build: OK
Shared tests: OK
Reservation tests: OK
Finance tests: OK
Gateway smoke: OK / skipped
```

#### 3. Documentación

Actualizar:

- `README.md`
- `docs/rabbitmq_event_bus.md` si aplica

Agregar una seccion breve de verificacion local:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1
```

Con Gateway ya levantado:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1 -GatewaySmoke
```

Mencionar que `HotelLux.Shared.Tests` cubre parsing RabbitMQ para `RabbitMq__Uri`, `CLOUDAMQP_URL`, puerto y TLS.

#### 4. No incluir outputs generados

Confirmar que `bin/` y `obj/` de `tests/HotelLux.Shared.Tests` no quedan trackeados por git.

No editar `.gitignore` salvo que realmente falte algo. Actualmente ya contiene `bin/` y `obj/`.

### Verificación obligatoria

Ejecutar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1
```

Si hay Gateway vivo en `http://127.0.0.1:5000`, ejecutar tambien:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1 -NoRestore -GatewaySmoke
```

Tambien ejecutar:

```powershell
git status --short
```

Confirmar que no aparecen `bin/` ni `obj/` como archivos nuevos trackeables.

### Reporte final requerido

Responder con:

- Archivos modificados.
- Si agregaste tests al `.slnx` o si dejaste el `.slnx` solo para APIs.
- Resultado de `scripts/Test-Backend.ps1`.
- Resultado de `scripts/Test-Backend.ps1 -NoRestore -GatewaySmoke` si se pudo.
- Confirmar que `HotelLux.Shared.Tests` queda dentro del flujo estándar.
- Confirmar que no se tocaron RabbitMQ runtime, GraphQL schema ni lógica Business.

## Prompt 18 - Preflight de despliegue Render: Dockerfiles, variables y smoke remoto

Necesito preparar el cierre operativo para desplegar en Render con RabbitMQ externo/CloudAMQP.

### Contexto

Ya esta implementado y validado:

- RabbitMQ local con Docker.
- Bus de eventos de auditoria con MassTransit.
- `AuditEventConsumer` consume y persiste en BD.
- GraphQL en Gateway `/graphql`.
- Health checks `/health`, `/health/live`, `/health/ready`.
- Render/CloudAMQP soporta `RabbitMq__Uri`, `CLOUDAMQP_URL` y variables separadas `Host`, `Port`, `VirtualHost`, `Username`, `Password`, `UseSsl`.
- `scripts/Test-Backend.ps1` ejecuta build + tests + smoke opcional.
- `HotelLux.Stack.slnx` incluye APIs y tests.

Verificaciones recientes:

```text
Test-Backend.ps1: OK
Test-Backend.ps1 -NoRestore -GatewaySmoke: OK
E2E local RabbitMQ -> Auth login -> Audit -> BD: OK
Gateway/YARP coverage MISSING=0
```

### Objetivo

Dejar listo el proyecto para desplegar en Render sin confusiones:

- Verificar que Dockerfiles no se rompieron al agregar tests al `.slnx`.
- Documentar exactamente las variables por servicio.
- Crear un script de smoke remoto para Render.
- Validar que Gateway, GraphQL, RabbitMQ readiness y auditoria se puedan comprobar despues del deploy.

No modificar logica Business. No cambiar RabbitMQ runtime salvo error real. No cambiar GraphQL schema.

### Cambios requeridos

#### 1. Revisar Dockerfiles

Revisar estos archivos:

- `HotelLux.Auth/Dockerfile`
- `HotelLux.Accommodation/Dockerfile`
- `HotelLux.Reservation/Dockerfile`
- `HotelLux.Stay/Dockerfile`
- `HotelLux.Finance/Dockerfile`
- `HotelLux.Audit/Dockerfile`
- `HotelLux.Gateway/Dockerfile`

Confirmar:

- Cada Dockerfile hace restore/publish del `.csproj` de la API, no de `HotelLux.Stack.slnx`.
- Copia `HotelLux.Shared`.
- Copia `protos` cuando el servicio usa gRPC/protos.
- Gateway copia `docs` si los necesita para Swagger/catalogo.
- Usa `PORT` y no puerto fijo hardcodeado en runtime.

Si encuentras un Dockerfile que dependa del `.slnx` o no copie algo necesario, corregirlo.

#### 2. Crear matriz de variables Render

Crear:

```text
docs/render_env_matrix.md
```

Debe tener una tabla por servicio:

- `hotellux-audit`
- `hotellux-auth`
- `hotellux-accommodation`
- `hotellux-reservation`
- `hotellux-stay`
- `hotellux-finance`
- `hotellux-gateway`

Para cada servicio indicar:

- Dockerfile path.
- Variables obligatorias.
- Variables opcionales.
- Health endpoints post-deploy.
- Si requiere RabbitMQ.
- Si requiere URLs gRPC-Web a otros servicios.

Reglas importantes:

- Gateway no requiere RabbitMQ.
- Audit requiere RabbitMQ y `ConnectionStrings__AuditDb`.
- Publishers Auth/Accommodation/Reservation/Stay/Finance requieren RabbitMQ.
- En Render no usar `localhost` para RabbitMQ.
- Para RabbitMQ recomendar:

```text
CLOUDAMQP_URL=amqps://...
RabbitMq__AuditQueue=hotellux.audit.events
```

o:

```text
RabbitMq__Uri=amqps://...
RabbitMq__AuditQueue=hotellux.audit.events
```

- Incluir alternativa con variables separadas.
- No poner secretos reales.

#### 3. Actualizar DEPLOY.md

Agregar referencia a `docs/render_env_matrix.md`.

Agregar una seccion breve:

```text
Checklist final antes del deploy
```

Debe incluir:

- Ejecutar `scripts/Test-Backend.ps1`.
- Confirmar Docker Desktop/RabbitMQ local solo para pruebas.
- Tener CloudAMQP/RabbitMQ externo listo.
- Cargar variables en Render.
- Hacer Manual Deploy en orden: Audit, Auth, Finance, Accommodation, Reservation, Stay, Gateway.
- Probar health/readiness.
- Probar GraphQL.
- Probar login o una operacion que genere auditoria.

#### 4. Crear script de smoke remoto Render

Crear:

```text
scripts/Test-RenderSmoke.ps1
```

Parametros:

```powershell
[Parameter(Mandatory)][string]$GatewayBaseUrl
[string]$AuditBaseUrl
[string]$AuthBaseUrl
[string]$AccommodationBaseUrl
[string]$ReservationBaseUrl
[string]$StayBaseUrl
[string]$FinanceBaseUrl
[switch]$RequireServiceReady
```

Comportamiento:

1. Normalizar URLs quitando `/` final.
2. Probar Gateway:
   - `GET /health`
   - `GET /health/live`
   - `GET /health/ready`
   - `POST /graphql` con `{ __typename }`
   - `GET /swagger/v1/swagger.json`
3. Para cada servicio cuyo BaseUrl fue enviado:
   - `GET /health`
   - `GET /health/live`
   - `GET /health/ready`
4. Si `RequireServiceReady` esta activo, fallar si cualquier `/health/ready` de servicio devuelve distinto de 200.
5. Si `RequireServiceReady` no esta activo, mostrar warning si un servicio devuelve 503 en `/health/ready`, sin fallar todo el script por readiness. Gateway si debe fallar si no responde bien.
6. GraphQL debe parsear JSON y validar `data.__typename == "Query"`, igual que `Test-GatewayHealth.ps1`.
7. No enviar credenciales ni probar login en este script todavia, para evitar manejar secretos.
8. Imprimir resumen claro:

```text
Gateway health: OK
Gateway GraphQL: OK
Gateway Swagger: OK
Audit ready: OK/WARN/skipped
Auth ready: OK/WARN/skipped
...
```

#### 5. Documentar uso del smoke remoto

En `DEPLOY.md` y/o `docs/render_env_matrix.md`, agregar ejemplo:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-RenderSmoke.ps1 `
  -GatewayBaseUrl "https://hotellux-gateway.onrender.com" `
  -AuditBaseUrl "https://hotellux-audit.onrender.com" `
  -AuthBaseUrl "https://hotellux-auth.onrender.com" `
  -AccommodationBaseUrl "https://hotellux-accommodation.onrender.com" `
  -ReservationBaseUrl "https://hotellux-reservation.onrender.com" `
  -StayBaseUrl "https://hotellux-stay.onrender.com" `
  -FinanceBaseUrl "https://hotellux-finance.onrender.com" `
  -RequireServiceReady
```

Indicar que en planes free de Render puede haber cold start; si falla por timeout, esperar y repetir.

### Verificacion obligatoria

Ejecutar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1
```

Si Gateway local esta levantado:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1 -NoRestore -GatewaySmoke
```

Validar sintaxis del nuevo script sin depender de Render:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command "$null = [scriptblock]::Create((Get-Content scripts\Test-RenderSmoke.ps1 -Raw)); 'OK syntax'"
```

Si Gateway local esta levantado, probar `Test-RenderSmoke.ps1` contra local usando solo Gateway:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-RenderSmoke.ps1 -GatewayBaseUrl "http://127.0.0.1:5000"
```

Resultado esperado:

- Gateway health/live/ready OK.
- GraphQL OK.
- Swagger OK.
- Servicios omitidos si no se pasaron URLs.

Opcional si Docker esta activo:

```powershell
docker compose -f docker-compose.rabbitmq.yml up -d
powershell -ExecutionPolicy Bypass -File scripts\Test-RabbitMq.ps1
```

### Reporte final requerido

Responder con:

- Archivos modificados.
- Confirmacion Dockerfiles.
- Matriz Render creada.
- Script `Test-RenderSmoke.ps1` creado.
- Resultado de `Test-Backend.ps1`.
- Resultado de smoke local con `Test-RenderSmoke.ps1` si se pudo.
- Confirmar que no se tocaron Business, GraphQL schema ni runtime RabbitMQ salvo correccion necesaria.

### Criterio de aceptacion

Listo si:

- Dockerfiles siguen aptos para Render.
- Variables Render quedan claras por servicio.
- Existe smoke remoto reutilizable.
- Tests locales siguen pasando.
- Gateway local puede pasar el smoke remoto apuntando a `http://127.0.0.1:5000`.

---

## Prompt 19 - Validacion final integral EventBus RabbitMQ + Gateway GraphQL + REST/gRPC antes de deploy

### Contexto

Ya se implemento la base principal:

- Bus de eventos con RabbitMQ/MassTransit.
- Publishers de auditoria en Auth, Accommodation, Reservation, Stay y Finance.
- Consumer `AuditEventConsumer` en Audit.
- Configuracion RabbitMQ local y Render/CloudAMQP.
- Gateway con GraphQL en `/graphql`.
- YARP/REST y Swagger global en Gateway.
- Health checks separados:
  - `/health`
  - `/health/live`
  - `/health/ready`
- Scripts:
  - `scripts/Test-Backend.ps1`
  - `scripts/Test-GatewayHealth.ps1`
  - `scripts/Test-RabbitMq.ps1`
  - `scripts/Test-RenderSmoke.ps1`

No quiero que hagas cambios grandes de arquitectura en este prompt. La mision es **validar integralmente que todo funcione correctamente** y corregir solo si encuentras un fallo real y reproducible.

### Objetivo

Verificar, de forma precisa, que los cambios cumplen:

1. RabbitMQ funciona correctamente con el bus de eventos.
2. Los microservicios publican eventos de auditoria por RabbitMQ.
3. Audit consume desde RabbitMQ y persiste en BD.
4. Gateway sirve correctamente GraphQL.
5. Gateway sigue sirviendo REST publico/local por YARP.
6. gRPC sigue funcionando donde corresponde.
7. Middleware, auth, CORS, Swagger y health checks siguen en orden correcto.
8. La configuracion local y Render no se contradicen.

### Reglas importantes

- No reescribas la arquitectura.
- No elimines `AuditGrpcService` todavia; queda como compatibilidad temporal.
- No vuelvas a registrar `AuditGrpcEmitter` / `AuditGrpcClient` como camino principal de auditoria.
- No uses `IPublishEndpoint` dentro de emitters singleton. Deben seguir usando `IBus`.
- No cambies Business salvo que exista un bug demostrado.
- No cambies contratos REST/gRPC/GraphQL salvo que una prueba falle por contrato incorrecto.
- Si Docker Desktop no esta activo, no inventes resultados: reporta que el E2E RabbitMQ local queda pendiente hasta encender Docker.

### Paso 1 - Auditoria estatica obligatoria

Revisar y reportar:

#### EventBus/RabbitMQ

Confirmar:

- `HotelLux.Shared/Events/AuditEventMessage.cs` existe y compila.
- `RabbitMqSettings` soporta:
  - `Uri`
  - `Host`
  - `Port`
  - `VirtualHost`
  - `Username`
  - `Password`
  - `UseSsl`
  - `AuditQueue`
- `RabbitMqConnectionResolver` soporta:
  - local `localhost:5672`
  - `amqp://`
  - `amqps://`
  - puerto por defecto `5672` para amqp
  - puerto por defecto `5671` para amqps
  - vhost desde URI
  - credenciales desde URI
- `RabbitMqConfigurationExtensions` usa:
  - `RabbitMq__Uri`
  - fallback `CLOUDAMQP_URL`
  - `cfg.Host(host, port, vhost, ...)`
  - `UseSsl` cuando corresponde.

Buscar:

```powershell
rg -n "IPublishEndpoint|IBus|AddMassTransit|UsingRabbitMq|RabbitMq|CLOUDAMQP_URL|UseSsl|AuditQueue" HotelLux.Shared HotelLux.Auth HotelLux.Accommodation HotelLux.Reservation HotelLux.Stay HotelLux.Finance HotelLux.Audit
```

Debe quedar:

- 0 referencias a `IPublishEndpoint` en codigo productivo.
- Emitters RabbitMQ usando `IBus`.
- Publishers con `AddHotelLuxRabbitMqPublisher`.
- Audit con `AddConsumer<AuditEventConsumer>()` y `ReceiveEndpoint(rabbitMqSettings.AuditQueue, ...)`.

#### gRPC

Confirmar:

- `MapGrpcService(...).EnableGrpcWeb()` sigue en Auth, Accommodation, Reservation, Stay, Finance y Audit.
- `GrpcChannelFactory` sigue resolviendo URLs para clientes gRPC.
- `GRPC_USE_WEB` / `RENDER=true` esta documentado para Render.
- Los clientes gRPC internos siguen existiendo donde la saga los necesita:
  - Reservation -> Accommodation / Finance / Stay.
  - Stay -> Reservation / Accommodation / Finance.
  - Accommodation -> Stay si corresponde.

Buscar:

```powershell
rg -n "MapGrpcService|EnableGrpcWeb|GrpcChannelFactory|GrpcAddress|GRPC_USE_WEB|RENDER" HotelLux.Shared HotelLux.Auth HotelLux.Accommodation HotelLux.Reservation HotelLux.Stay HotelLux.Finance HotelLux.Audit DEPLOY.md docs
```

#### Gateway GraphQL/REST

Confirmar:

- Gateway mantiene `MapReverseProxy()`.
- Gateway expone `MapGraphQL("/graphql")`.
- GraphQL reenvia `Authorization` a los clientes HTTP tipados.
- GraphQL consulta endpoints REST existentes:
  - `GET api/v1/accommodations/search`
  - `GET api/v1/accommodations/{sucursalGuid}`
  - `GET api/v1/accommodations/{sucursalGuid}/reviews`
  - `GET api/v1/public/reservas/{reservaGuid}`
  - `POST api/v1/accommodations/reservas`
- Swagger global sigue disponible en `/swagger/v1/swagger.json`.

Buscar:

```powershell
rg -n "AddGraphQL|MapGraphQL|MapReverseProxy|Authorization|AccommodationGatewayClient|ReservationGatewayClient" HotelLux.Gateway
```

#### Middleware/health

Confirmar en todos los servicios:

- Exception middleware antes de controllers.
- `UseRouting()`.
- `UseGrpcWeb()`.
- `UseCors()`.
- `UseAuthentication()`.
- `UseAuthorization()`.
- `MapControllers()`.
- `MapHealthChecks("/health")`.
- `MapHealthChecks("/health/live")`.
- `MapHealthChecks("/health/ready")`.

`/health` y `/health/live` deben excluir MassTransit; `/health/ready` debe incluirlo.

### Paso 2 - Build y tests obligatorios

Ejecutar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1
```

Luego, si Gateway local esta levantado:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1 -NoRestore -GatewaySmoke
```

Resultado esperado:

```text
Build: OK
Shared tests: OK
Reservation tests: OK
Finance tests: OK
Gateway smoke: OK o skipped si Gateway no esta levantado
```

Si falla por permisos de `NuGet.Config`, reportalo claramente como problema de entorno/sandbox y repite en una terminal normal de Windows.

### Paso 3 - Smoke Gateway local

Si Gateway esta levantado en `http://127.0.0.1:5000`, ejecutar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-RenderSmoke.ps1 -GatewayBaseUrl "http://127.0.0.1:5000"
```

Debe validar:

- `GET /health` -> 200
- `GET /health/live` -> 200
- `GET /health/ready` -> 200
- `POST /graphql` con `{ __typename }` -> 200 y `data.__typename == "Query"`
- `GET /swagger/v1/swagger.json` -> 200

### Paso 4 - Cobertura REST publica/local por Gateway

Con Gateway levantado, validar que las rutas del catalogo no devuelvan `404` por falta de ruta YARP.

Usar `docs/endpoints_publicas.txt`, `docs/endpoints_locales.txt` y `HotelLux.Gateway/appsettings.json`.

Criterio:

- `404` en Gateway indica ruta no cubierta por YARP.
- `401` indica ruta cubierta pero requiere token.
- `502`/`503` indica ruta cubierta pero microservicio destino no esta levantado.
- Para esta validacion de routing, `401`, `502` y `503` son aceptables; `404` no.

Reportar:

```text
Endpoints revisados: N
Rutas sin cobertura Gateway/YARP: 0
```

Si aparece alguna ruta sin cobertura, corregir `HotelLux.Gateway/appsettings.json` agregando la ruta YARP minima y repetir la prueba.

### Paso 5 - RabbitMQ local E2E

Si Docker Desktop esta encendido, ejecutar:

```powershell
docker compose -f docker-compose.rabbitmq.yml up -d
powershell -ExecutionPolicy Bypass -File scripts\Test-RabbitMq.ps1
```

Luego levantar como minimo:

- `HotelLux.Audit.API`
- un publisher, preferiblemente `HotelLux.Auth.API`
- opcionalmente Gateway si se quiere probar via Gateway

Ejecutar una operacion real que publique auditoria. Preferida:

```http
POST http://127.0.0.1:5001/api/v1/auth/login
```

Payload seed local:

```json
{
  "username": "admin",
  "password": "admin1234"
}
```

Confirmar:

- RabbitMQ management UI responde en `http://localhost:15672`.
- La cola `hotellux.audit.events` existe.
- Audit aparece como consumer.
- El mensaje publicado se consume.
- No quedan mensajes pendientes despues de consumir.
- `HotelLux_Audit.auditoria.evento_auditoria` recibe un registro nuevo.

Si Docker no esta activo:

- No modificar codigo.
- Reportar: "E2E RabbitMQ local pendiente porque Docker Desktop no esta corriendo".

### Paso 6 - Preparacion Render

Revisar `docs/render_env_matrix.md` y `DEPLOY.md`.

Confirmar que Render queda asi:

#### Publishers y Audit

Usar una de estas dos formas:

```text
RabbitMq__Uri=amqps://<usuario>:<password>@<host>:5671/<vhost>
RabbitMq__AuditQueue=hotellux.audit.events
```

o:

```text
CLOUDAMQP_URL=amqps://<usuario>:<password>@<host>:5671/<vhost>
RabbitMq__AuditQueue=hotellux.audit.events
```

Alternativa por partes:

```text
RabbitMq__Host=<host>
RabbitMq__Port=5671
RabbitMq__VirtualHost=<vhost>
RabbitMq__Username=<usuario>
RabbitMq__Password=<password>
RabbitMq__UseSsl=true
RabbitMq__AuditQueue=hotellux.audit.events
```

#### Gateway

Gateway no debe tener RabbitMQ.

Debe tener clusters YARP por variables:

```text
ReverseProxy__Clusters__accommodation__Destinations__api__Address=https://...
ReverseProxy__Clusters__reservation__Destinations__api__Address=https://...
ReverseProxy__Clusters__stay__Destinations__api__Address=https://...
ReverseProxy__Clusters__auth__Destinations__api__Address=https://...
ReverseProxy__Clusters__finance__Destinations__api__Address=https://...
ReverseProxy__Clusters__audit__Destinations__api__Address=https://...
```

#### gRPC Render

Confirmar documentacion:

```text
GRPC_USE_WEB=true
```

solo si la plataforma no inyecta `RENDER=true`.

En Render deberia bastar `RENDER=true`, pero si hay error de protocolo, documentar que se debe forzar `GRPC_USE_WEB=true`.

### Paso 7 - Smoke Render remoto

No ejecutes contra Render si no tienes las URLs reales. Pero deja listo el comando final para el usuario:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-RenderSmoke.ps1 `
  -GatewayBaseUrl "https://hotellux-gateway.onrender.com" `
  -AuditBaseUrl "https://hotellux-audit.onrender.com" `
  -AuthBaseUrl "https://hotellux-auth.onrender.com" `
  -AccommodationBaseUrl "https://hotellux-accommodation.onrender.com" `
  -ReservationBaseUrl "https://hotellux-reservation.onrender.com" `
  -StayBaseUrl "https://hotellux-stay.onrender.com" `
  -FinanceBaseUrl "https://hotellux-finance.onrender.com" `
  -RequireServiceReady
```

### Correcciones permitidas

Solo corregir si se demuestra un fallo:

- Ruta Gateway/YARP faltante.
- Smoke GraphQL que falla por serializacion/parsing.
- Variable Render documentada con nombre incorrecto.
- Registro DI incorrecto.
- Health check mal separado.
- RabbitMQ URI/SSL mal resuelto.
- Middleware fuera de orden y causando error real.

Si corriges algo, ejecutar otra vez:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1
```

Y si Gateway esta levantado:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1 -NoRestore -GatewaySmoke
powershell -ExecutionPolicy Bypass -File scripts\Test-RenderSmoke.ps1 -GatewayBaseUrl "http://127.0.0.1:5000"
```

### Reporte final requerido

Responder con:

```text
Archivos modificados:
- ...

Auditoria EventBus/RabbitMQ:
- OK / hallazgos

Gateway GraphQL:
- OK / hallazgos

REST publico/local por Gateway:
- OK / hallazgos

gRPC:
- OK / hallazgos

Middleware/health:
- OK / hallazgos

Build/tests:
- Build: OK/FAIL
- Shared tests: OK/FAIL
- Reservation tests: OK/FAIL
- Finance tests: OK/FAIL

Smokes:
- Gateway smoke: OK/skipped/FAIL
- RenderSmoke local Gateway: OK/skipped/FAIL
- RabbitMQ E2E local: OK/skipped/FAIL

Pendientes reales:
- ...
```

### Criterio de aceptacion final

Este prompt queda completo si:

- Build pasa con 0 errores.
- Tests pasan.
- Gateway GraphQL responde.
- Swagger global responde.
- Rutas REST publicas/locales no tienen faltantes YARP.
- RabbitMQ local queda validado si Docker esta disponible.
- Render queda listo con matriz de variables clara.
- No queda ningun bug reproducible pendiente en EventBus/RabbitMQ/Gateway GraphQL/gRPC/middleware.

---

## Prompt 20 - E2E real RabbitMQ local y smoke Render final

### Contexto

El Prompt 19 valido la implementacion estatica y los smokes disponibles:

- Build OK.
- Tests OK.
- Gateway GraphQL OK.
- Swagger OK.
- Rutas REST/YARP OK.
- gRPC y middleware OK.
- No se encontraron bugs reproducibles.

El unico pendiente real es operativo:

1. Validar el flujo E2E RabbitMQ local con Docker Desktop activo.
2. Ejecutar smoke remoto Render si ya existen URLs reales desplegadas.

No hagas cambios de codigo salvo que una prueba falle por un bug real y reproducible.

### Objetivo

Probar el flujo completo:

```text
Auth/Publisher -> RabbitMQ -> AuditEventConsumer -> HotelLux_Audit.auditoria.evento_auditoria
```

Y confirmar que el Gateway desplegado o local sigue respondiendo:

```text
/health
/health/live
/health/ready
/graphql
/swagger/v1/swagger.json
```

### Reglas

- No modificar Business.
- No cambiar contratos REST, GraphQL ni gRPC si las pruebas pasan.
- No eliminar `AuditGrpcService`.
- No volver a activar auditoria gRPC como camino principal.
- No tocar RabbitMQ runtime si Docker no esta disponible.
- Si Docker Desktop no esta corriendo, reportar el bloqueo y no inventar resultados.
- Si una prueba falla, primero identificar si es:
  - entorno apagado,
  - puerto ocupado,
  - BD no disponible,
  - credenciales faltantes,
  - bug de codigo.

### Paso 1 - Confirmar baseline

Ejecutar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1 -NoRestore -GatewaySmoke
```

Si Gateway no esta levantado, ejecutar sin `-GatewaySmoke`:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1 -NoRestore
```

Resultado esperado:

```text
Build: OK
Shared tests: OK
Reservation tests: OK
Finance tests: OK
Gateway smoke: OK/skipped
```

### Paso 2 - Levantar RabbitMQ local

Primero verificar Docker:

```powershell
docker ps
```

Si Docker falla con daemon no disponible, detener esta seccion y reportar:

```text
RabbitMQ E2E local: skipped
Motivo: Docker Desktop no esta corriendo.
```

Si Docker esta disponible:

```powershell
docker compose -f docker-compose.rabbitmq.yml up -d
powershell -ExecutionPolicy Bypass -File scripts\Test-RabbitMq.ps1
```

Confirmar:

- AMQP `localhost:5672` responde.
- Management UI `http://localhost:15672` responde.
- Usuario local `guest/guest` funciona.

### Paso 3 - Levantar servicios minimos

Levantar como minimo:

- `HotelLux.Audit.API`
- `HotelLux.Auth.API`

Usar puertos locales esperados:

```text
Auth HTTP: 5001
Audit HTTP: 5008
RabbitMQ: 5672
RabbitMQ UI: 15672
```

Si existe `scripts/Start-HotelLuxStack.ps1` y esta actualizado, se puede usar con RabbitMQ requerido:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Start-HotelLuxStack.ps1 -RequireRabbitMq
```

Si el script levanta mas servicios de los necesarios, esta bien. Pero no mates procesos que no hayas iniciado sin avisar.

### Paso 4 - Verificar readiness

Con RabbitMQ activo:

```powershell
Invoke-WebRequest -Uri "http://127.0.0.1:5008/health/ready" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:5001/health/ready" -UseBasicParsing
```

Resultado esperado:

```text
Audit /health/ready -> 200
Auth /health/ready -> 200
```

Si alguno devuelve 503:

- Revisar logs.
- Confirmar `RabbitMq` en `appsettings.json`.
- Confirmar que RabbitMQ esta corriendo.
- Confirmar cola/vhost/usuario/password.

### Paso 5 - Ejecutar operacion real que publique auditoria

Usar login seed local:

```powershell
$body = @{
  username = "admin"
  password = "admin1234"
} | ConvertTo-Json

Invoke-WebRequest `
  -Uri "http://127.0.0.1:5001/api/v1/auth/login" `
  -Method POST `
  -ContentType "application/json" `
  -Body $body `
  -UseBasicParsing
```

Resultado esperado:

```text
HTTP 200
```

Si falla por credenciales seed:

- No modificar codigo.
- Reportar que el seed local no esta disponible.
- Usar otra operacion real que publique auditoria, si existe y se tienen datos.

### Paso 6 - Verificar RabbitMQ

En `http://localhost:15672` o por API/script, confirmar:

- Cola `hotellux.audit.events` existe.
- Consumers: `1` o mas.
- Mensajes publicados aumenta al ejecutar login.
- Mensajes entregados/consumidos aumenta.
- Mensajes pendientes quedan en `0` despues de consumir.

Si hay mensajes pendientes:

- Confirmar que `HotelLux.Audit.API` esta levantado.
- Confirmar que `AuditEventConsumer` esta registrado.
- Revisar logs de Audit.

### Paso 7 - Verificar persistencia en BD Audit

Consultar `HotelLux_Audit` y confirmar registro nuevo en:

```text
auditoria.evento_auditoria
```

Campos esperados aproximados:

```text
servicio_origen = auth-service
tabla_afectada = seguridad.usuario_app
operacion = INSERT o UPDATE segun normalizacion
usuario_ejecutor = guid/usuario del login
fecha_evento_utc reciente
```

No inventes el resultado. Si no puedes conectarte a PostgreSQL local, reporta:

```text
Persistencia Audit: no verificada por falta de acceso a BD local.
```

### Paso 8 - Smoke Gateway local final

Si Gateway esta levantado:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-RenderSmoke.ps1 -GatewayBaseUrl "http://127.0.0.1:5000"
```

Debe pasar:

- health
- live
- ready
- GraphQL `__typename`
- Swagger

### Paso 9 - Smoke Render remoto si hay URLs reales

Si el usuario ya tiene URLs reales de Render, ejecutar ajustando URLs:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-RenderSmoke.ps1 `
  -GatewayBaseUrl "https://hotellux-gateway.onrender.com" `
  -AuditBaseUrl "https://hotellux-audit.onrender.com" `
  -AuthBaseUrl "https://hotellux-auth.onrender.com" `
  -AccommodationBaseUrl "https://hotellux-accommodation.onrender.com" `
  -ReservationBaseUrl "https://hotellux-reservation.onrender.com" `
  -StayBaseUrl "https://hotellux-stay.onrender.com" `
  -FinanceBaseUrl "https://hotellux-finance.onrender.com" `
  -RequireServiceReady
```

Si no hay URLs reales o no estan desplegadas:

```text
Smoke Render remoto: skipped
Motivo: faltan URLs reales post-deploy.
```

### Correcciones permitidas

Solo corregir si hay evidencia:

- RabbitMQ URI/SSL/vhost mal resuelto.
- Health readiness no incluye MassTransit donde debe.
- Audit no registra consumer.
- Publisher no publica por RabbitMQ.
- Gateway smoke falla por GraphQL/Swagger.
- Ruta Gateway/YARP devuelve 404 por falta de configuracion.

Despues de cualquier correccion, ejecutar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1 -NoRestore
```

Y si Gateway esta levantado:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1 -NoRestore -GatewaySmoke
powershell -ExecutionPolicy Bypass -File scripts\Test-RenderSmoke.ps1 -GatewayBaseUrl "http://127.0.0.1:5000"
```

### Reporte final requerido

Responder con:

```text
Archivos modificados:
- ninguno / lista

Baseline:
- Build:
- Shared tests:
- Reservation tests:
- Finance tests:
- Gateway smoke:

RabbitMQ local:
- Docker:
- RabbitMQ container:
- Test-RabbitMq.ps1:
- Queue:
- Consumers:
- Publish/deliver:

Evento real:
- Operacion ejecutada:
- HTTP status:
- Mensaje consumido:
- Registro BD Audit:

Gateway:
- RenderSmoke local:

Render remoto:
- Smoke:
- URLs usadas:

Pendientes reales:
- ...
```

### Criterio de aceptacion

Listo si:

- Baseline sigue verde.
- RabbitMQ local responde si Docker esta activo.
- Un evento real se publica y se consume.
- Audit persiste el evento en BD o se reporta claramente por que no se pudo verificar BD.
- Gateway local sigue pasando health/GraphQL/Swagger.
- Smoke Render remoto queda ejecutado si hay URLs reales.
- No queda ningun bug reproducible sin corregir.

---

## Prompt 21 - Redeploy Render y verificacion post-deploy de version actual

### Contexto

El codigo local ya esta validado:

- Build OK.
- Tests OK.
- Gateway local OK:
  - `/health`
  - `/health/live`
  - `/health/ready`
  - `/graphql`
  - `/swagger/v1/swagger.json`
- EventBus/RabbitMQ esta implementado y configurado.
- gRPC, REST/YARP y middleware estan correctos localmente.

El smoke remoto contra Render fallo asi:

```text
Gateway /health -> 200
Gateway /health/live -> 404
Gateway /health/ready -> 404
Gateway /graphql -> 404
Gateway /swagger/v1/swagger.json -> 200
Audit /health/ready -> 404
Auth /health/ready -> 404
```

Diagnostico: Render esta ejecutando una version anterior al cambio de health/live/ready, GraphQL y RabbitMQ readiness. No es un bug reproducible del codigo local; hace falta redeployar los servicios con el codigo actual y las variables correctas.

### Objetivo

Preparar y ejecutar una verificacion de redeploy Render para que la version remota quede alineada con el codigo local actual.

### Reglas

- No modificar codigo de negocio.
- No cambiar GraphQL, REST, gRPC ni RabbitMQ salvo que el redeploy demuestre un bug real.
- No inventar resultados de Render.
- Si no tienes acceso al dashboard de Render, dejar instrucciones exactas para que el usuario haga Manual Deploy.
- Si alguna URL real es distinta de las usadas abajo, pedir/usar la URL correcta.

### Paso 1 - Confirmar baseline local antes de redeploy

Ejecutar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1 -NoRestore -GatewaySmoke
powershell -ExecutionPolicy Bypass -File scripts\Test-RenderSmoke.ps1 -GatewayBaseUrl "http://127.0.0.1:5000"
```

Resultado esperado:

```text
Build: OK
Shared tests: OK
Reservation tests: OK
Finance tests: OK
Gateway smoke: OK
RenderSmoke local Gateway: OK
```

### Paso 2 - Confirmar commit/rama que Render debe desplegar

Revisar:

```powershell
git status --short
git branch --show-current
git log -1 --oneline
```

Reportar:

```text
Rama local:
Ultimo commit local:
Cambios sin commit:
```

Si Render despliega desde GitHub y los cambios locales no estan commit/push:

- No hacer deploy esperando que Render tome cambios que no existen en remoto.
- Indicar que primero hay que commit/push de los cambios actuales.

Si el usuario no quiere commit todavia, dejar claro que el redeploy de Render seguira usando la version vieja.

### Paso 3 - Revisar variables Render obligatorias

Usar `docs/render_env_matrix.md` como fuente.

#### Todos los servicios menos Gateway

Confirmar que tienen RabbitMQ:

Opcion A recomendada:

```text
RabbitMq__Uri=amqps://<usuario>:<password>@<host>:5671/<vhost>
RabbitMq__AuditQueue=hotellux.audit.events
```

o:

```text
CLOUDAMQP_URL=amqps://<usuario>:<password>@<host>:5671/<vhost>
RabbitMq__AuditQueue=hotellux.audit.events
```

No usar:

```text
RabbitMq__Host=localhost
```

en Render.

#### Gateway

Gateway no debe tener RabbitMQ. Debe tener clusters YARP:

```text
ReverseProxy__Clusters__accommodation__Destinations__api__Address=https://hotellux-accommodation.onrender.com/
ReverseProxy__Clusters__reservation__Destinations__api__Address=https://hotellux-reservation.onrender.com/
ReverseProxy__Clusters__stay__Destinations__api__Address=https://hotellux-stay.onrender.com/
ReverseProxy__Clusters__auth__Destinations__api__Address=https://hotellux-auth.onrender.com/
ReverseProxy__Clusters__finance__Destinations__api__Address=https://hotellux-finance.onrender.com/
ReverseProxy__Clusters__audit__Destinations__api__Address=https://hotellux-audit.onrender.com/
```

#### gRPC

En Render normalmente existe:

```text
RENDER=true
```

Si gRPC remoto falla por protocolo, agregar:

```text
GRPC_USE_WEB=true
```

en los servicios que tienen clientes gRPC salientes:

- Reservation
- Stay
- Accommodation si usa Stay gRPC

### Paso 4 - Orden de Manual Deploy

Hacer Manual Deploy en este orden:

1. `hotellux-audit`
2. `hotellux-auth`
3. `hotellux-finance`
4. `hotellux-accommodation`
5. `hotellux-reservation`
6. `hotellux-stay`
7. `hotellux-gateway`

Razon:

- Audit debe estar listo para consumir eventos.
- Publishers deben arrancar con RabbitMQ configurado.
- Gateway debe ser el ultimo porque apunta a todos los clusters.

Esperar a que cada servicio termine deploy antes de seguir con el siguiente.

### Paso 5 - Smoke remoto post-deploy

Ejecutar:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Test-RenderSmoke.ps1 `
  -GatewayBaseUrl "https://hotellux-gateway.onrender.com" `
  -AuditBaseUrl "https://hotellux-audit.onrender.com" `
  -AuthBaseUrl "https://hotellux-auth.onrender.com" `
  -AccommodationBaseUrl "https://hotellux-accommodation.onrender.com" `
  -ReservationBaseUrl "https://hotellux-reservation.onrender.com" `
  -StayBaseUrl "https://hotellux-stay.onrender.com" `
  -FinanceBaseUrl "https://hotellux-finance.onrender.com" `
  -RequireServiceReady
```

Resultado esperado despues del redeploy correcto:

```text
Gateway health: OK
Gateway GraphQL: OK
Gateway Swagger: OK
Audit ready: OK
Auth ready: OK
Accommodation ready: OK
Reservation ready: OK
Stay ready: OK
Finance ready: OK
```

Si algun servicio free de Render esta dormido, esperar 30-90 segundos y repetir.

### Paso 6 - Diagnostico si sigue fallando

#### Si Gateway sigue con 404 en `/health/live` o `/graphql`

Significa que Gateway sigue en version vieja.

Revisar:

- Render esta apuntando al repo/rama correcta.
- El ultimo deploy tomo el commit actual.
- El Dockerfile de Gateway fue usado.
- El servicio no esta usando una imagen/cache vieja.

#### Si Gateway OK pero servicios `/health/ready` dan 503

Revisar RabbitMQ/CloudAMQP:

- `RabbitMq__Uri` o `CLOUDAMQP_URL`.
- Credenciales.
- Vhost.
- Puerto 5671.
- TLS/AMQPS.
- `RabbitMq__AuditQueue`.

#### Si servicios dan 404 en `/health/ready`

Ese servicio sigue en version vieja. Redeployar ese servicio especifico.

#### Si GraphQL da 500

Revisar logs de Gateway. Posibles causas:

- URL cluster incorrecta.
- HotChocolate no cargo schema.
- Error de serializacion del BFF.

No cambiar schema sin evidencia.

### Paso 7 - Prueba funcional remota de evento

Cuando smoke remoto este OK, ejecutar una operacion real contra Auth remoto si se tienen credenciales seed/produccion:

```http
POST https://hotellux-auth.onrender.com/api/v1/auth/login
```

o via Gateway si la ruta esta proxyada:

```http
POST https://hotellux-gateway.onrender.com/api/v1/auth/login
```

Confirmar en Audit remoto:

- Evento registrado.
- Servicio origen `auth-service`.
- Fecha reciente.

Si no hay credenciales o acceso a BD remota:

```text
Prueba funcional evento remoto: skipped por falta de credenciales/acceso BD.
```

### Reporte final requerido

Responder:

```text
Archivos modificados:
- ninguno / lista

Baseline local:
- Build:
- Tests:
- Gateway local:

Git/Render source:
- Rama:
- Ultimo commit:
- Cambios sin commit:
- Render puede desplegar version actual: si/no

Variables Render:
- RabbitMQ publishers/Audit:
- Gateway clusters:
- gRPC:

Manual Deploy:
- Audit:
- Auth:
- Finance:
- Accommodation:
- Reservation:
- Stay:
- Gateway:

Smoke Render:
- Gateway /health:
- Gateway /health/live:
- Gateway /health/ready:
- Gateway /graphql:
- Gateway /swagger:
- Audit /health/ready:
- Auth /health/ready:
- Accommodation /health/ready:
- Reservation /health/ready:
- Stay /health/ready:
- Finance /health/ready:

Evento remoto:
- Ejecutado/skipped:
- Resultado:

Pendientes reales:
- ...
```

### Criterio de aceptacion

Listo si:

- Render ya no devuelve 404 en `/health/live`, `/health/ready` ni `/graphql`.
- Gateway remoto responde GraphQL y Swagger.
- Servicios remotos tienen `/health/ready`.
- Si RabbitMQ/CloudAMQP esta configurado, readiness remoto pasa 200.
- Si no pasa, queda diagnosticada una variable/servicio especifico.
