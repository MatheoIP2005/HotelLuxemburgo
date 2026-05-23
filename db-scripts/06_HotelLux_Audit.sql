-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio AUDIT
-- Base de datos: HotelLux_Audit
-- Motor: PostgreSQL 18
-- Version: 1.0
--
-- ALCANCE: este microservicio NO expone REST publico. Es un
-- consumidor gRPC fire-and-forget que recibe eventos de todos
-- los demas microservicios y los persiste para trazabilidad.
--
-- DIFERENCIA CON EL MONOLITO:
--   En el monolito existian 13 triggers en SQL Server que
--   escribian a seguridad.AUDITORIA en la misma BD. Aqui los
--   triggers DESAPARECEN. Cada microservicio (auth, accommodation,
--   reservation, stay, finance) implementa un cliente gRPC que
--   llama a audit.EmitAuditEvent(...) tras cada operacion CRUD.
--
-- DEPENDENCIAS LOGICAS:
--   - NINGUNA. Es la unica BD totalmente autonoma del sistema.
--     Sus referencias_id son GUIDs sueltos que apuntan a
--     entidades de otras BDs (sin necesidad de validacion).
--
-- CONTENIDO:
--   Schema: auditoria
--   Tablas: evento_auditoria
--   Datos semilla: 13 eventos representativos del flujo end-to-end
--                  ya sembrado en las otras 5 BDs.
-- ============================================================

-- ============================================================
-- SCHEMA
-- ============================================================
CREATE SCHEMA IF NOT EXISTS auditoria;


-- ============================================================
-- TABLA: auditoria.evento_auditoria
--
-- Cambios respecto al monolito:
--   - id_auditoria: BIGINT (soporta volumen alto)
--   - datos_anteriores / datos_nuevos: JSONB (mucho mejor que
--     VARCHAR(MAX) para PostgreSQL: indexable, consultable,
--     valida sintaxis JSON automaticamente)
--   - servicio_origen: VARCHAR(80) con CHECK de los servicios
--     conocidos (auth, accommodation, reservation, stay,
--     finance, gateway). Si llega otro nombre, se rechaza.
-- ============================================================
CREATE TABLE auditoria.evento_auditoria (
    id_auditoria             BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    auditoria_guid           UUID         NOT NULL DEFAULT gen_random_uuid(),
    tabla_afectada           VARCHAR(100) NOT NULL,             -- ej: alojamiento.sucursal
    operacion                VARCHAR(10)  NOT NULL,             -- INSERT | UPDATE | DELETE
    entidad_guid             UUID         NULL,                 -- ref logica a la entidad de otra BD
    id_registro_afectado     VARCHAR(100) NULL,                 -- ID INT del registro (opcional)
    datos_anteriores         JSONB        NULL,                 -- estado previo (UPDATE/DELETE)
    datos_nuevos             JSONB        NULL,                 -- estado nuevo (INSERT/UPDATE)
    usuario_ejecutor         VARCHAR(100) NOT NULL,             -- username de quien hizo la accion
    usuario_guid             UUID         NULL,                 -- ref logica al usuario en Auth
    ip_origen                VARCHAR(45)  NULL,
    servicio_origen          VARCHAR(80)  NOT NULL,
    fecha_evento_utc         TIMESTAMPTZ  NOT NULL DEFAULT now(),
    activo                   BOOLEAN      NOT NULL DEFAULT TRUE,
    CONSTRAINT uq_auditoria_guid     UNIQUE (auditoria_guid),
    CONSTRAINT chk_auditoria_operacion CHECK (operacion IN ('INSERT','UPDATE','DELETE')),
    CONSTRAINT chk_auditoria_servicio  CHECK (
        servicio_origen IN (
            'auth-service',
            'accommodation-service',
            'reservation-service',
            'stay-service',
            'finance-service',
            'gateway-service',
            'audit-service'
        )
    )
);


-- ============================================================
-- INDICES DE APOYO
-- Los queries tipicos del endpoint GET /auditoria son:
--   - "eventos de la tabla X en el rango de fechas Y"
--   - "eventos generados por el usuario Z"
--   - "eventos sobre la entidad con GUID W"
--   - "eventos del servicio S"
-- ============================================================
CREATE INDEX ix_auditoria_tabla_fecha
    ON auditoria.evento_auditoria(tabla_afectada, fecha_evento_utc DESC);

CREATE INDEX ix_auditoria_usuario
    ON auditoria.evento_auditoria(usuario_ejecutor, fecha_evento_utc DESC);

CREATE INDEX ix_auditoria_servicio
    ON auditoria.evento_auditoria(servicio_origen, fecha_evento_utc DESC);

CREATE INDEX ix_auditoria_entidad
    ON auditoria.evento_auditoria(entidad_guid)
    WHERE entidad_guid IS NOT NULL;

CREATE INDEX ix_auditoria_fecha
    ON auditoria.evento_auditoria(fecha_evento_utc DESC);

-- Indice GIN sobre los JSONB para queries del tipo "donde datos_nuevos->>'campo' = X"
CREATE INDEX ix_auditoria_datos_nuevos_gin
    ON auditoria.evento_auditoria USING GIN (datos_nuevos);


-- ============================================================
-- DATOS SEMILLA
--
-- 13 eventos que reflejan el flujo end-to-end ya sembrado en las
-- otras 5 BDs. Permiten probar el endpoint GET /auditoria y los
-- filtros (por tabla, fecha, usuario, servicio, entidad).
-- ============================================================

INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion, entidad_guid,
    id_registro_afectado, datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid, ip_origen, servicio_origen,
    fecha_evento_utc
) VALUES

-- 1. Creacion del usuario admin (Seeder inicial)
('eeeeeeee-eeee-eeee-eeee-eeeeeeee0001',
 'seguridad.usuario_app', 'INSERT',
 '21111111-1111-1111-1111-111111111001',
 '1', NULL,
 '{"username":"admin","correo":"admin@hotelluxemburgo.com","estado":"ACT","activo":true}'::jsonb,
 'system', NULL, '127.0.0.1', 'auth-service',
 '2026-04-01 08:00:00+00'),

-- 2. Creacion del usuario juan.perez
('eeeeeeee-eeee-eeee-eeee-eeeeeeee0002',
 'seguridad.usuario_app', 'INSERT',
 '21111111-1111-1111-1111-111111111004',
 '4', NULL,
 '{"username":"juan.perez","correo":"juan.perez@gmail.com","cliente_guid":"33333333-3333-3333-3333-333333333001"}'::jsonb,
 'juan.perez', '21111111-1111-1111-1111-111111111004', '190.123.45.67', 'auth-service',
 '2026-04-15 14:22:00+00'),

-- 3. Creacion del cliente Juan Perez en Reservation (replica)
('eeeeeeee-eeee-eeee-eeee-eeeeeeee0003',
 'reservas.cliente', 'INSERT',
 '33333333-3333-3333-3333-333333333001',
 '1', NULL,
 '{"tipo_identificacion":"CED","numero_identificacion":"1712345678","nombres":"Juan Carlos","apellidos":"Perez Mendoza"}'::jsonb,
 'juan.perez', '21111111-1111-1111-1111-111111111004', '190.123.45.67', 'reservation-service',
 '2026-04-15 14:22:05+00'),

-- 4. Creacion de la sucursal LUX-UIO
('eeeeeeee-eeee-eeee-eeee-eeeeeeee0004',
 'alojamiento.sucursal', 'INSERT',
 '44444444-4444-4444-4444-444444444001',
 '1', NULL,
 '{"codigo":"LUX-UIO","nombre":"Hotel Luxemburgo Quito","ciudad":"Quito","estrellas":5}'::jsonb,
 'admin', '21111111-1111-1111-1111-111111111001', '127.0.0.1', 'accommodation-service',
 '2026-04-01 09:15:00+00'),

-- 5. Creacion de la habitacion Quito 102
('eeeeeeee-eeee-eeee-eeee-eeeeeeee0005',
 'alojamiento.habitacion', 'INSERT',
 '66666666-6666-6666-6666-666666666002',
 '2', NULL,
 '{"numero":"102","sucursal":"LUX-UIO","tipo":"TH-DOBLE","precio_base":120.00,"estado":"DIS"}'::jsonb,
 'admin', '21111111-1111-1111-1111-111111111001', '127.0.0.1', 'accommodation-service',
 '2026-04-01 09:30:00+00'),

-- 6. Creacion de la reserva 001 (Juan Perez)
('eeeeeeee-eeee-eeee-eeee-eeeeeeee0006',
 'reservas.reserva', 'INSERT',
 '99999999-9999-9999-9999-999999999001',
 '1', NULL,
 '{"codigo":"RES-2026-000001","cliente_guid":"33333333-3333-3333-3333-333333333001","sucursal_guid":"44444444-4444-4444-4444-444444444001","total":690.00,"estado":"PEN","origen":"PORTAL"}'::jsonb,
 'juan.perez', '21111111-1111-1111-1111-111111111004', '190.123.45.67', 'reservation-service',
 '2026-05-11 10:00:00+00'),

-- 7. UPDATE de la reserva 001 a estado CON (confirmacion)
('eeeeeeee-eeee-eeee-eeee-eeeeeeee0007',
 'reservas.reserva', 'UPDATE',
 '99999999-9999-9999-9999-999999999001',
 '1',
 '{"estado":"PEN"}'::jsonb,
 '{"estado":"CON","fecha_confirmacion_utc":"2026-05-11T10:00:05Z"}'::jsonb,
 'juan.perez', '21111111-1111-1111-1111-111111111004', '190.123.45.67', 'reservation-service',
 '2026-05-11 10:00:05+00'),

-- 8. Generacion de factura inicial (Juan Perez) via gRPC desde Reservation -> Finance
('eeeeeeee-eeee-eeee-eeee-eeeeeeee0008',
 'finanzas.factura', 'INSERT',
 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0001',
 '1', NULL,
 '{"numero":"FAC-RES-2026-000001","tipo":"RESERVA","total":690.00,"saldo_pendiente":690.00,"estado":"EMI"}'::jsonb,
 'juan.perez', '21111111-1111-1111-1111-111111111004', '190.123.45.67', 'finance-service',
 '2026-05-11 10:00:08+00'),

-- 9. Check-in de Ana Lopez (creacion de estadia)
('eeeeeeee-eeee-eeee-eeee-eeeeeeee0009',
 'hospedaje.estadia', 'INSERT',
 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002',
 '1', NULL,
 '{"reserva_guid":"99999999-9999-9999-9999-999999999002","cliente_guid":"33333333-3333-3333-3333-333333333002","habitacion_guid":"66666666-6666-6666-6666-666666666007","checkin_utc":"2026-05-09T15:30:00Z","estado":"ACT"}'::jsonb,
 'vendedor1', '21111111-1111-1111-1111-111111111002', '10.10.0.15', 'stay-service',
 '2026-05-09 15:30:00+00'),

-- 10. UPDATE habitacion GYE 201 a estado OCU (post check-in)
('eeeeeeee-eeee-eeee-eeee-eeeeeeee0010',
 'alojamiento.habitacion', 'UPDATE',
 '66666666-6666-6666-6666-666666666007',
 '7',
 '{"estado_habitacion":"DIS"}'::jsonb,
 '{"estado_habitacion":"OCU"}'::jsonb,
 'vendedor1', '21111111-1111-1111-1111-111111111002', '10.10.0.15', 'accommodation-service',
 '2026-05-09 15:30:02+00'),

-- 11. Cargo de room service para Ana Lopez
('eeeeeeee-eeee-eeee-eeee-eeeeeeee0011',
 'hospedaje.cargo_estadia', 'INSERT',
 'cccccccc-cccc-cccc-cccc-cccccccc1001',
 '1', NULL,
 '{"estadia_guid":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002","catalogo":"SRV-RS","monto_total":17.25,"estado":"PEN"}'::jsonb,
 'vendedor1', '21111111-1111-1111-1111-111111111002', '10.10.0.15', 'stay-service',
 '2026-05-09 20:00:00+00'),

-- 12. Pago aprobado de Ana Lopez (tarjeta de credito via Stripe)
('eeeeeeee-eeee-eeee-eeee-eeeeeeee0012',
 'finanzas.pago', 'INSERT',
 'cccccccc-cccc-cccc-cccc-cccccccc2001',
 '1', NULL,
 '{"factura_guid":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002","monto":874.00,"metodo":"TARJETA_CREDITO","pasarela":"STRIPE_SANDBOX","estado":"APR"}'::jsonb,
 'vendedor1', '21111111-1111-1111-1111-111111111002', '10.10.0.15', 'finance-service',
 '2026-05-05 11:05:00+00'),

-- 13. Valoracion publicada de Pedro Garcia
('eeeeeeee-eeee-eeee-eeee-eeeeeeee0013',
 'hospedaje.valoracion', 'INSERT',
 'dddddddd-dddd-dddd-dddd-dddddddd1003',
 '1', NULL,
 '{"estadia_guid":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003","cliente_guid":"33333333-3333-3333-3333-333333333003","sucursal_guid":"44444444-4444-4444-4444-444444444003","puntuacion_general":9.0,"tipo_viaje":"solo","estado":"PUB","publicada_en_portal":true}'::jsonb,
 'pedro.garcia', NULL, '186.45.67.89', 'stay-service',
 '2026-04-25 09:00:00+00');


-- ============================================================
-- VERIFICACION FINAL
-- ============================================================
SELECT 'Eventos de auditoria registrados: ' || COUNT(*)::text FROM auditoria.evento_auditoria;

-- Resumen agrupado por servicio_origen
SELECT
    servicio_origen,
    COUNT(*)                                    AS eventos,
    MIN(fecha_evento_utc)                       AS primer_evento,
    MAX(fecha_evento_utc)                       AS ultimo_evento,
    COUNT(*) FILTER (WHERE operacion='INSERT')  AS inserts,
    COUNT(*) FILTER (WHERE operacion='UPDATE')  AS updates,
    COUNT(*) FILTER (WHERE operacion='DELETE')  AS deletes
FROM   auditoria.evento_auditoria
GROUP  BY servicio_origen
ORDER  BY servicio_origen;

-- Linea de tiempo del flujo end-to-end (filtrable)
SELECT
    fecha_evento_utc,
    servicio_origen,
    tabla_afectada,
    operacion,
    usuario_ejecutor,
    entidad_guid
FROM   auditoria.evento_auditoria
ORDER  BY fecha_evento_utc;
