-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio AUDIT
-- Base de datos: HotelLux_Audit
-- Motor: PostgreSQL 18
-- Version: 1.1
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
-- CORRECCIONES v1.1 (respecto al original):
--   [EVT-02] Evento reemplazado: 'juan.perez' (GUID 21...004)
--            no existe en Auth. Ahora registra creacion real
--            del usuario 'vendedor' (GUID 21...002).
--   [EVT-03] usuario_ejecutor: 'juan.perez' -> 'vendedor'
--            usuario_guid: 21...004 -> 21...002
--   [EVT-06] usuario_ejecutor: 'juan.perez' -> 'vendedor'
--   [EVT-07] usuario_ejecutor: 'juan.perez' -> 'vendedor'
--   [EVT-08] usuario_ejecutor: 'juan.perez' -> 'vendedor'
--   [EVT-09] usuario_ejecutor: 'vendedor1'  -> 'vendedor'
--   [EVT-10] usuario_ejecutor: 'vendedor1'  -> 'vendedor'
--   [EVT-11] usuario_ejecutor: 'vendedor1'  -> 'vendedor'
--   [EVT-12] usuario_ejecutor: 'vendedor1'  -> 'vendedor'
--            fecha_evento_utc: '2026-05-05 11:05:00+00'
--                           -> '2026-05-02 11:31:10+00'
--            (alineada con finanzas.pago cc2001 en HotelLux_Finance)
--   [EVT-13] usuario_ejecutor: 'pedro.garcia' -> 'vendedor'
--            (pedro.garcia no tiene cuenta en Auth)
--            usuario_guid: NULL -> 21...002
--   [NUEVOS]  Eventos 14-23: flujo completo Carlos Mora
--            (cliente 33...005, RES-005, FAC-006, pago cc2004,
--            estadia aa...005, valoracion dd...1005, pago cc2005)
--            y Turismo Andes (cliente 33...004, RES-004, FAC-005).
--
-- CONTENIDO:
--   Schema: auditoria
--   Tablas: evento_auditoria
--   Datos semilla: 23 eventos representativos del flujo end-to-end
--                  alineados con las otras 5 BDs del sistema.
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
--     VARCHAR(MAX) de SQL Server: indexable, consultable,
--     valida sintaxis JSON automaticamente)
--   - servicio_origen: VARCHAR(80) con CHECK de los servicios
--     conocidos (auth, accommodation, reservation, stay,
--     finance, gateway). Si llega otro nombre, se rechaza.
-- ============================================================
CREATE TABLE auditoria.evento_auditoria (
    id_auditoria             BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    auditoria_guid           UUID         NOT NULL DEFAULT gen_random_uuid(),
    tabla_afectada           VARCHAR(100) NOT NULL,   -- ej: alojamiento.sucursal
    operacion                VARCHAR(10)  NOT NULL,   -- INSERT | UPDATE | DELETE
    entidad_guid             UUID         NULL,       -- ref logica a la entidad de otra BD
    id_registro_afectado     VARCHAR(100) NULL,       -- ID secuencial del registro (opcional)
    datos_anteriores         JSONB        NULL,       -- estado previo (UPDATE/DELETE)
    datos_nuevos             JSONB        NULL,       -- estado nuevo (INSERT/UPDATE)
    usuario_ejecutor         VARCHAR(100) NOT NULL,   -- username de quien realizo la accion
    usuario_guid             UUID         NULL,       -- ref logica al usuario en Auth
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
-- Queries tipicos del endpoint GET /auditoria:
--   - eventos de la tabla X en rango de fechas Y
--   - eventos generados por el usuario Z
--   - eventos sobre la entidad con GUID W
--   - eventos del servicio S
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

-- Indice GIN sobre JSONB para queries tipo datos_nuevos->>'campo' = X
CREATE INDEX ix_auditoria_datos_nuevos_gin
    ON auditoria.evento_auditoria USING GIN (datos_nuevos);

-- ============================================================
-- DATOS SEMILLA: 23 eventos representativos del flujo end-to-end
-- v1.1 — alineados con Auth, Accommodation, Reservation,
--          Stay y Finance.
--
-- Unicos usuario_ejecutor validos en datos (no comentarios):
--   'admin'   (21111111-1111-1111-1111-111111111001)
--   'vendedor'(21111111-1111-1111-1111-111111111002)
--   'system'  (NULL — arranque del sistema)
-- ============================================================


-- ------------------------------------------------------------
-- EVT-01  Bootstrap: creacion cuenta admin
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0001',
    'seguridad.usuario_app', 'INSERT',
    '21111111-1111-1111-1111-111111111001',
    NULL,
    NULL,
    '{"username":"admin","correo":"admin@hotelluxemburgo.com","nombres":"Administrador","apellidos":"Sistema","rol":"ADMIN"}',
    'system', NULL,
    '10.0.0.1', 'auth-service', '2026-01-15 08:00:00+00'
);

-- ------------------------------------------------------------
-- EVT-02  Bootstrap: creacion cuenta vendedor
--         [Correccion v1.1: reemplaza registro de juan.perez
--          (GUID 21...004) que NO existe en Auth]
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0002',
    'seguridad.usuario_app', 'INSERT',
    '21111111-1111-1111-1111-111111111002',
    NULL,
    NULL,
    '{"username":"vendedor","correo":"vendedor@hotelluxemburgo.com","nombres":"Vendedor","apellidos":"Recepcion","rol":"VENDEDOR"}',
    'admin', '21111111-1111-1111-1111-111111111001',
    '192.168.1.10', 'auth-service', '2026-01-15 08:05:00+00'
);

-- ------------------------------------------------------------
-- EVT-03  Registro cliente Juan Perez (33...001)
--         [Correccion v1.1: usuario_ejecutor 'juan.perez' -> 'vendedor']
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0003',
    'reservas.cliente', 'INSERT',
    '33333333-3333-3333-3333-333333333001',
    NULL,
    NULL,
    '{"nombres":"Juan Carlos","apellidos":"Perez Romero","tipo_identificacion":"CED","numero_identificacion":"1712345678","correo":"juan.perez@gmail.com","sucursal_referencia":"LUX-UIO"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.10', 'reservation-service', '2026-04-15 14:22:05+00'
);

-- ------------------------------------------------------------
-- EVT-04  Registro cliente Ana Lopez (33...002)
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0004',
    'reservas.cliente', 'INSERT',
    '33333333-3333-3333-3333-333333333002',
    NULL,
    NULL,
    '{"nombres":"Ana Maria","apellidos":"Lopez Vega","tipo_identificacion":"CED","numero_identificacion":"0912345678","correo":"ana.lopez@gmail.com","sucursal_referencia":"LUX-GYE"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.10', 'reservation-service', '2026-05-01 10:00:00+00'
);

-- ------------------------------------------------------------
-- EVT-05  Registro cliente Pedro Garcia walk-in (33...003)
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0005',
    'reservas.cliente', 'INSERT',
    '33333333-3333-3333-3333-333333333003',
    NULL,
    NULL,
    '{"nombres":"Pedro Andres","apellidos":"Garcia Mora","tipo_identificacion":"CED","numero_identificacion":"0103456789","correo":"pedro.garcia@hotmail.com","tipo_cliente":"walk-in","sucursal_referencia":"LUX-CUE"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.11', 'reservation-service', '2026-04-20 14:00:00+00'
);

-- ------------------------------------------------------------
-- EVT-06  Creacion reserva RES-001 Juan Perez — LUX-UIO
--         [Correccion v1.1: usuario_ejecutor 'juan.perez' -> 'vendedor']
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0006',
    'reservas.reserva', 'INSERT',
    '99999999-9999-9999-9999-999999999001',
    'RES-2026-000001',
    NULL,
    '{"codigo_reserva":"RES-2026-000001","cliente_guid":"33333333-3333-3333-3333-333333333001","sucursal_guid":"44444444-4444-4444-4444-444444444401","sucursal_codigo":"LUX-UIO","check_in":"2026-06-10","check_out":"2026-06-15","estado":"CON"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.10', 'reservation-service', '2026-05-11 10:00:05+00'
);

-- ------------------------------------------------------------
-- EVT-07  Creacion reserva RES-002 Ana Lopez — LUX-GYE
--         [Correccion v1.1: usuario_ejecutor 'juan.perez' -> 'vendedor']
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0007',
    'reservas.reserva', 'INSERT',
    '99999999-9999-9999-9999-999999999002',
    'RES-2026-000002',
    NULL,
    '{"codigo_reserva":"RES-2026-000002","cliente_guid":"33333333-3333-3333-3333-333333333002","sucursal_guid":"44444444-4444-4444-4444-444444444402","sucursal_codigo":"LUX-GYE","check_in":"2026-05-09","check_out":"2026-05-13","estado":"EMI"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.10', 'reservation-service', '2026-05-02 11:30:05+00'
);

-- ------------------------------------------------------------
-- EVT-08  Creacion reserva RES-003 Pedro Garcia walk-in — LUX-CUE
--         [Correccion v1.1: usuario_ejecutor 'juan.perez' -> 'vendedor']
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0008',
    'reservas.reserva', 'INSERT',
    '99999999-9999-9999-9999-999999999003',
    'RES-2026-000003',
    NULL,
    '{"codigo_reserva":"RES-2026-000003","cliente_guid":"33333333-3333-3333-3333-333333333003","sucursal_guid":"44444444-4444-4444-4444-444444444403","sucursal_codigo":"LUX-CUE","check_in":"2026-04-20","check_out":"2026-04-23","estado":"FIN","walk_in":true}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.11', 'reservation-service', '2026-04-20 14:00:05+00'
);

-- ------------------------------------------------------------
-- EVT-09  Emision factura FAC-001 para Juan Perez (reserva)
--         [Correccion v1.1: usuario_ejecutor 'vendedor1' -> 'vendedor']
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0009',
    'finanzas.factura', 'INSERT',
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0001',
    'FAC-RES-2026-000001',
    NULL,
    '{"factura_guid":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0001","reserva_guid":"99999999-9999-9999-9999-999999999001","tipo":"RESERVA","estado":"PEN","total_usd":862.50}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.10', 'finance-service', '2026-05-11 10:00:08+00'
);

-- ------------------------------------------------------------
-- EVT-10  Emision factura FAC-002 para Ana Lopez (reserva)
--         [Correccion v1.1: usuario_ejecutor 'vendedor1' -> 'vendedor']
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0010',
    'finanzas.factura', 'INSERT',
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002',
    'FAC-RES-2026-000002',
    NULL,
    '{"factura_guid":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002","reserva_guid":"99999999-9999-9999-9999-999999999002","tipo":"RESERVA","estado":"APR","total_usd":529.00}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.10', 'finance-service', '2026-05-02 11:30:45+00'
);

-- ------------------------------------------------------------
-- EVT-11  Emision factura FAC-003 para Pedro Garcia (reserva walk-in)
--         [Correccion v1.1: usuario_ejecutor 'vendedor1' -> 'vendedor']
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0011',
    'finanzas.factura', 'INSERT',
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0003',
    'FAC-RES-2026-000003',
    NULL,
    '{"factura_guid":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0003","reserva_guid":"99999999-9999-9999-9999-999999999003","tipo":"RESERVA","estado":"APR","total_usd":207.00,"metodo_pago":"EFECTIVO"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.11', 'finance-service', '2026-04-20 14:05:00+00'
);

-- ------------------------------------------------------------
-- EVT-12  Pago cc2001 — Ana Lopez cancela FAC-002 via Visa/Stripe
--         [Correccion v1.1: usuario_ejecutor 'vendedor1' -> 'vendedor'
--          fecha_evento_utc ajustada a 2026-05-02 11:31:10+00
--          (alineada con finanzas.pago cc2001 en HotelLux_Finance)]
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0012',
    'finanzas.pago', 'INSERT',
    'cccccccc-cccc-cccc-cccc-cccccccc2001',
    NULL,
    NULL,
    '{"pago_guid":"cccccccc-cccc-cccc-cccc-cccccccc2001","factura_guid":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002","monto":529.00,"moneda":"USD","estado":"APR","pasarela":"STRIPE","transaccion":"pi_3OqzAna2026050200001","autorizacion":"AUTH-2026-05-02-A7B3C1"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '10.0.1.5', 'finance-service', '2026-05-02 11:31:10+00'
);

-- ------------------------------------------------------------
-- EVT-13  Check-in estadia aa...003 Pedro Garcia — LUX-CUE hab 101
--         [Correccion v1.1: usuario_ejecutor 'pedro.garcia' -> 'vendedor'
--          usuario_guid: NULL -> 21...002
--          (pedro.garcia no tiene cuenta en Auth)]
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0013',
    'hospedaje.estadia', 'INSERT',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003',
    NULL,
    NULL,
    '{"estadia_guid":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003","reserva_guid":"99999999-9999-9999-9999-999999999003","cliente_guid":"33333333-3333-3333-3333-333333333003","sucursal_codigo":"LUX-CUE","habitacion":"101","check_in":"2026-04-20T14:35:00Z","check_out":"2026-04-23T11:45:00Z","estado":"FIN"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.11', 'stay-service', '2026-04-20 14:35:00+00'
);

-- ============================================================
-- NUEVOS EVENTOS (14-23) — Flujo completo Carlos Mora
-- y Turismo Andes  [agregados en v1.1]
-- ============================================================

-- ------------------------------------------------------------
-- EVT-14  Registro cliente Carlos Mora — turista colombiano (33...005)
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0014',
    'reservas.cliente', 'INSERT',
    '33333333-3333-3333-3333-333333333005',
    NULL,
    NULL,
    '{"nombres":"Carlos Renato","apellidos":"Mora Salinas","tipo_identificacion":"PAS","numero_identificacion":"PA12345678","correo":"cmora.viajes@gmail.com","nacionalidad":"Colombia","sucursal_referencia":"LUX-CUE"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.11', 'reservation-service', '2026-03-10 16:00:00+00'
);

-- ------------------------------------------------------------
-- EVT-15  Creacion reserva RES-005 Carlos Mora — LUX-CUE
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0015',
    'reservas.reserva', 'INSERT',
    '99999999-9999-9999-9999-999999999005',
    'RES-2026-000005',
    NULL,
    '{"codigo_reserva":"RES-2026-000005","cliente_guid":"33333333-3333-3333-3333-333333333005","sucursal_guid":"44444444-4444-4444-4444-444444444403","sucursal_codigo":"LUX-CUE","check_in":"2026-03-15","check_out":"2026-03-19","estado":"FIN","pasaporte":"PA12345678"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.11', 'reservation-service', '2026-03-10 16:00:05+00'
);

-- ------------------------------------------------------------
-- EVT-16  Emision factura FAC-006 Carlos Mora (reserva)
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0016',
    'finanzas.factura', 'INSERT',
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0006',
    'FAC-RES-2026-000005',
    NULL,
    '{"factura_guid":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0006","reserva_guid":"99999999-9999-9999-9999-999999999005","tipo":"RESERVA","estado":"APR","total_usd":529.00,"cliente":"Carlos Mora","pasaporte":"PA12345678"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.11', 'finance-service', '2026-03-10 16:00:08+00'
);

-- ------------------------------------------------------------
-- EVT-17  Pago cc2004 — Carlos Mora cancela FAC-006 via Mastercard
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0017',
    'finanzas.pago', 'INSERT',
    'cccccccc-cccc-cccc-cccc-cccccccc2004',
    NULL,
    NULL,
    '{"pago_guid":"cccccccc-cccc-cccc-cccc-cccccccc2004","factura_guid":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0006","monto":529.00,"moneda":"USD","estado":"APR","pasarela":"STRIPE","transaccion":"pi_3MrkCar2026031000001","autorizacion":"AUTH-2026-03-10-K9L2M4"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '10.0.1.5', 'finance-service', '2026-03-10 16:01:20+00'
);

-- ------------------------------------------------------------
-- EVT-18  Check-in estadia aa...005 Carlos Mora — LUX-CUE hab 102
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0018',
    'hospedaje.estadia', 'INSERT',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa005',
    NULL,
    NULL,
    '{"estadia_guid":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa005","reserva_guid":"99999999-9999-9999-9999-999999999005","cliente_guid":"33333333-3333-3333-3333-333333333005","sucursal_codigo":"LUX-CUE","habitacion":"102","check_in":"2026-03-15T14:00:00Z","check_out":"2026-03-19T12:00:00Z","estado":"FIN","pasaporte":"PA12345678"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.11', 'stay-service', '2026-03-15 14:00:00+00'
);

-- ------------------------------------------------------------
-- EVT-19  Cargo spa cc1004 durante estadia Carlos Mora
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0019',
    'hospedaje.cargo_estadia', 'INSERT',
    'cccccccc-cccc-cccc-cccc-cccccccc1004',
    NULL,
    NULL,
    '{"cargo_guid":"cccccccc-cccc-cccc-cccc-cccccccc1004","estadia_guid":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa005","catalogo":"SPA-ANDES-WELLNESS","descripcion":"Sesion spa 60 min","monto_usd":45.00,"iva_15pct":6.75,"total_usd":51.75,"estado":"FAC"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.11', 'stay-service', '2026-03-17 17:00:00+00'
);

-- ------------------------------------------------------------
-- EVT-20  Emision factura final FAC-007 Carlos Mora (cargo spa)
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0020',
    'finanzas.factura', 'INSERT',
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0007',
    'FAC-FIN-2026-000002',
    NULL,
    '{"factura_guid":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0007","estadia_guid":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa005","tipo":"FINAL","estado":"APR","subtotal_usd":45.00,"iva_usd":6.75,"total_usd":51.75,"concepto":"Sesion spa 60 min — Hotel Luxemburgo Cuenca"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.11', 'finance-service', '2026-03-19 12:05:00+00'
);

-- ------------------------------------------------------------
-- EVT-21  Pago cc2005 — Carlos Mora cancela FAC-007 (spa)
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0021',
    'finanzas.pago', 'INSERT',
    'cccccccc-cccc-cccc-cccc-cccccccc2005',
    NULL,
    NULL,
    '{"pago_guid":"cccccccc-cccc-cccc-cccc-cccccccc2005","factura_guid":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0007","monto":51.75,"moneda":"USD","estado":"APR","pasarela":"STRIPE","transaccion":"pi_3MrkCar2026031900001","autorizacion":"AUTH-2026-03-19-P5Q7R8"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '10.0.1.5', 'finance-service', '2026-03-19 12:10:00+00'
);

-- ------------------------------------------------------------
-- EVT-22  Valoracion dd...1005 de Carlos Mora — LUX-CUE
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0022',
    'hospedaje.valoracion', 'INSERT',
    'dddddddd-dddd-dddd-dddd-dddddddd1005',
    NULL,
    NULL,
    '{"valoracion_guid":"dddddddd-dddd-dddd-dddd-dddddddd1005","estadia_guid":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa005","cliente_guid":"33333333-3333-3333-3333-333333333005","sucursal_codigo":"LUX-CUE","puntaje":4,"comentario":"Excellent spa and bilingual team. Breakfast variety could be improved.","idioma":"EN","fecha_publicacion":"2026-03-22"}',
    'vendedor', '21111111-1111-1111-1111-111111111002',
    '192.168.1.11', 'stay-service', '2026-03-21 08:00:00+00'
);

-- ------------------------------------------------------------
-- EVT-23  Registro cliente Turismo Andes (33...004) y reserva
--         corporativa RES-004 / FAC-005 — LUX-UIO
-- ------------------------------------------------------------
INSERT INTO auditoria.evento_auditoria (
    auditoria_guid, tabla_afectada, operacion,
    entidad_guid, id_registro_afectado,
    datos_anteriores, datos_nuevos,
    usuario_ejecutor, usuario_guid,
    ip_origen, servicio_origen, fecha_evento_utc
) VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeee0023',
    'reservas.reserva', 'INSERT',
    '99999999-9999-9999-9999-999999999004',
    'RES-2026-000004',
    NULL,
    '{"codigo_reserva":"RES-2026-000004","cliente_guid":"33333333-3333-3333-3333-333333333004","razon_social":"Turismo Andes S.A.","ruc":"1791845623001","sucursal_guid":"44444444-4444-4444-4444-444444444401","sucursal_codigo":"LUX-UIO","check_in":"2026-07-01","check_out":"2026-07-03","estado":"CON","factura_guid":"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0005","factura_codigo":"FAC-RES-2026-000004","total_usd":575.00}',
    'admin', '21111111-1111-1111-1111-111111111001',
    '192.168.1.10', 'reservation-service', '2026-06-15 09:05:00+00'
);


-- ============================================================
-- VERIFICACION POST-CARGA
-- Ejecutar para confirmar integridad de los 23 eventos.
-- ============================================================
-- SELECT
--     servicio_origen,
--     usuario_ejecutor,
--     COUNT(*) AS eventos
-- FROM  auditoria.evento_auditoria
-- GROUP BY servicio_origen, usuario_ejecutor
-- ORDER BY servicio_origen, usuario_ejecutor;
