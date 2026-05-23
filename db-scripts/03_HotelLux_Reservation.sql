-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio RESERVATION
-- Base de datos: HotelLux_Reservation
-- Motor: PostgreSQL 18
-- Version: 2.0
--
-- DEPENDENCIAS LOGICAS (sin FK fisica):
--   - HotelLux_Accommodation: sucursal_guid, habitacion_guid,
--                             tarifa_guid
--   - HotelLux_Auth:          ninguno (cliente_guid se origina
--                             aqui y se replica en Auth)
--
-- CONTENIDO:
--   Schema: reservas
--   Tablas: cliente, reserva, reserva_habitacion
--
--   Datos semilla:
--     3 clientes ecuatorianos (cedula)
--     3 reservas en distintos estados del ciclo de vida:
--       RES-2026-000001: Juan Perez   -- CONFIRMADA  (futura)
--       RES-2026-000002: Ana Lopez    -- EN MARCHA   (activa)
--       RES-2026-000003: Pedro Garcia -- FINALIZADA  (historica)
--     3 lineas de reserva_habitacion
--
-- VALIDACION MATEMATICA (IVA 15%, Ecuador):
--   RES-001: 5n x $120.00 = $600.00 + $90.00 IVA  = $690.00
--   RES-002: 4n x $190.00 = $760.00 + $114.00 IVA = $874.00
--   RES-003: 3n x  $75.00 = $225.00 + $33.75 IVA  = $258.75
--
-- INSTRUCCIONES EN pgAdmin:
--   1. Create Database: HotelLux_Reservation / Owner: postgres
--   2. Query Tool -> File -> Open este archivo -> F5
--   3. Verificar los SELECT de conteo al final.
-- ============================================================


-- ============================================================
-- SCHEMA
-- ============================================================
CREATE SCHEMA IF NOT EXISTS reservas;


-- ============================================================
-- TABLA: reservas.cliente
-- ============================================================
CREATE TABLE reservas.cliente (
    id_cliente               INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cliente_guid             UUID         NOT NULL DEFAULT gen_random_uuid(),
    tipo_identificacion      VARCHAR(20)  NOT NULL,
    numero_identificacion    VARCHAR(30)  NOT NULL,
    nombres                  VARCHAR(160) NOT NULL,
    apellidos                VARCHAR(160) NULL,
    razon_social             VARCHAR(200) NULL,
    correo                   VARCHAR(150) NOT NULL,
    telefono                 VARCHAR(30)  NOT NULL,
    direccion                VARCHAR(250) NOT NULL,
    estado                   CHAR(3)      NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    creado_por_usuario       VARCHAR(100) NOT NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    modificado_por_usuario   VARCHAR(100) NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          VARCHAR(45)  NULL,
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    motivo_inhabilitacion    VARCHAR(250) NULL,
    servicio_origen          VARCHAR(50)  NOT NULL DEFAULT 'reservation-service',
    CONSTRAINT uq_cliente_guid          UNIQUE (cliente_guid),
    CONSTRAINT uq_cliente_identif       UNIQUE (numero_identificacion),
    CONSTRAINT uq_cliente_correo        UNIQUE (correo),
    CONSTRAINT chk_cliente_estado       CHECK (estado IN ('ACT','INA')),
    CONSTRAINT chk_cliente_tipo_identif CHECK (tipo_identificacion IN ('CED','RUC','PAS'))
);


-- ============================================================
-- TABLA: reservas.reserva
-- sucursal_guid -> referencia logica a HotelLux_Accommodation
-- ============================================================
CREATE TABLE reservas.reserva (
    id_reserva               INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    reserva_guid             UUID          NOT NULL DEFAULT gen_random_uuid(),
    codigo_reserva           VARCHAR(40)   NOT NULL,
    id_cliente               INT           NOT NULL,
    sucursal_guid            UUID          NOT NULL,
    fecha_reserva_utc        TIMESTAMPTZ   NOT NULL DEFAULT now(),
    fecha_inicio             TIMESTAMPTZ   NOT NULL,
    fecha_fin                TIMESTAMPTZ   NOT NULL,
    subtotal_reserva         NUMERIC(12,2) NOT NULL,
    valor_iva                NUMERIC(12,2) NOT NULL,
    total_reserva            NUMERIC(12,2) NOT NULL,
    descuento_aplicado       NUMERIC(12,2) NOT NULL DEFAULT 0,
    saldo_pendiente          NUMERIC(12,2) NOT NULL DEFAULT 0,
    origen_canal_reserva     VARCHAR(50)   NOT NULL,
    estado_reserva           CHAR(3)       NOT NULL DEFAULT 'PEN',
    fecha_confirmacion_utc   TIMESTAMPTZ   NULL,
    fecha_cancelacion_utc    TIMESTAMPTZ   NULL,
    motivo_cancelacion       VARCHAR(250)  NULL,
    observaciones            TEXT          NULL,
    es_walkin                BOOLEAN       NOT NULL DEFAULT FALSE,
    es_eliminado             BOOLEAN       NOT NULL DEFAULT FALSE,
    creado_por_usuario       VARCHAR(100)  NOT NULL,
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    modificado_por_usuario   VARCHAR(100)  NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          VARCHAR(45)   NULL,
    servicio_origen          VARCHAR(50)   NOT NULL DEFAULT 'reservation-service',
    fecha_inhabilitacion_utc TIMESTAMPTZ   NULL,
    motivo_inhabilitacion    VARCHAR(250)  NULL,
    CONSTRAINT uq_reserva_guid   UNIQUE (reserva_guid),
    CONSTRAINT uq_reserva_codigo UNIQUE (codigo_reserva),
    CONSTRAINT fk_reserva_cliente FOREIGN KEY (id_cliente)
        REFERENCES reservas.cliente(id_cliente),
    -- PEN | CON | CAN | EXP | FIN | EMI
    CONSTRAINT chk_reserva_estado    CHECK (estado_reserva IN ('PEN','CON','CAN','EXP','FIN','EMI')),
    CONSTRAINT chk_reserva_subtotal  CHECK (subtotal_reserva >= 0),
    CONSTRAINT chk_reserva_iva       CHECK (valor_iva >= 0),
    CONSTRAINT chk_reserva_total     CHECK (total_reserva >= 0),
    CONSTRAINT chk_reserva_desc      CHECK (descuento_aplicado >= 0),
    CONSTRAINT chk_reserva_saldo     CHECK (saldo_pendiente >= 0),
    CONSTRAINT chk_reserva_fechas    CHECK (fecha_fin > fecha_inicio),
    CONSTRAINT chk_reserva_coherente CHECK (total_reserva >= subtotal_reserva - descuento_aplicado)
);


-- ============================================================
-- TABLA: reservas.reserva_habitacion
-- habitacion_guid y tarifa_guid -> referencias logicas a
-- HotelLux_Accommodation (sin FK fisica)
-- ============================================================
CREATE TABLE reservas.reserva_habitacion (
    id_reserva_habitacion    INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    reserva_habitacion_guid  UUID          NOT NULL DEFAULT gen_random_uuid(),
    id_reserva               INT           NOT NULL,
    habitacion_guid          UUID          NOT NULL,
    tarifa_guid              UUID          NULL,
    fecha_inicio             TIMESTAMPTZ   NOT NULL,
    fecha_fin                TIMESTAMPTZ   NOT NULL,
    num_adultos              INT           NOT NULL DEFAULT 1,
    num_ninos                INT           NOT NULL DEFAULT 0,
    precio_noche_aplicado    NUMERIC(12,2) NOT NULL,
    subtotal_linea           NUMERIC(12,2) NOT NULL,
    valor_iva_linea          NUMERIC(12,2) NOT NULL,
    descuento_linea          NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_linea              NUMERIC(12,2) NOT NULL,
    estado_detalle           CHAR(3)       NOT NULL DEFAULT 'PEN',
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100)  NOT NULL,
    modificado_por_usuario   VARCHAR(100)  NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          VARCHAR(45)   NULL,
    servicio_origen          VARCHAR(50)   NOT NULL DEFAULT 'reservation-service',
    CONSTRAINT uq_reserva_hab_guid  UNIQUE (reserva_habitacion_guid),
    CONSTRAINT uq_reserva_hab_linea UNIQUE (id_reserva, habitacion_guid, fecha_inicio),
    CONSTRAINT fk_reserva_hab_reserva FOREIGN KEY (id_reserva)
        REFERENCES reservas.reserva(id_reserva),
    -- PEN | CON | CAN | FIN | EMI
    CONSTRAINT chk_reserva_hab_estado   CHECK (estado_detalle IN ('PEN','CON','CAN','FIN','EMI')),
    CONSTRAINT chk_reserva_hab_fechas   CHECK (fecha_fin > fecha_inicio),
    CONSTRAINT chk_reserva_hab_adultos  CHECK (num_adultos > 0),
    CONSTRAINT chk_reserva_hab_ninos    CHECK (num_ninos >= 0),
    CONSTRAINT chk_reserva_hab_precio   CHECK (precio_noche_aplicado > 0),
    CONSTRAINT chk_reserva_hab_subtotal CHECK (subtotal_linea >= 0),
    CONSTRAINT chk_reserva_hab_iva      CHECK (valor_iva_linea >= 0),
    CONSTRAINT chk_reserva_hab_desc     CHECK (descuento_linea >= 0),
    CONSTRAINT chk_reserva_hab_total    CHECK (total_linea >= 0)
);


-- ============================================================
-- INDICES DE APOYO
-- ============================================================
CREATE INDEX ix_cliente_identif_correo
    ON reservas.cliente(tipo_identificacion, numero_identificacion, correo);

CREATE INDEX ix_reserva_cliente_estado
    ON reservas.reserva(id_cliente, estado_reserva, fecha_inicio, fecha_fin);

CREATE INDEX ix_reserva_sucursal_fechas
    ON reservas.reserva(sucursal_guid, fecha_inicio, fecha_fin, estado_reserva);

-- Critico para validar solapamiento de habitaciones
CREATE INDEX ix_reserva_hab_habitacion_fechas
    ON reservas.reserva_habitacion(habitacion_guid, fecha_inicio, fecha_fin, estado_detalle);


-- ============================================================
-- ============================================================
-- DATOS SEMILLA
-- ============================================================
-- ============================================================


-- ============================================================
-- CLIENTES (prefijo GUID: 33xxxxxx-...)
--
--   33...001 = Juan Carlos Perez Mendoza  (Quito,     CED 1712345678)
--   33...002 = Ana Maria Lopez Vargas     (Guayaquil, CED 0923456789)
--   33...003 = Pedro Antonio Garcia Reyes (Cuenca,    CED 1098765432)
--
-- Nota: cliente_guid 33...001 y 33...002 estan vinculados
-- logicamente a los usuarios del portal en HotelLux_Auth.
-- Pedro Garcia (33...003) es cliente walk-in sin cuenta en Auth.
-- ============================================================
INSERT INTO reservas.cliente (
    cliente_guid,
    tipo_identificacion, numero_identificacion,
    nombres, apellidos,
    correo, telefono, direccion,
    creado_por_usuario
) VALUES
-- Cliente 1: Juan Carlos Perez - Quito, reservo por el portal
(
    '33333333-3333-3333-3333-333333333001',
    'CED', '1712345678',
    'Juan Carlos', 'Perez Mendoza',
    'juan.perez@gmail.com',
    '+593 99 123-4567',
    'Av. Eloy Alfaro N32-110 y Av. 6 de Diciembre, Quito',
    'vendedor'
),
-- Cliente 2: Ana Maria Lopez - Guayaquil, reservo por el portal
(
    '33333333-3333-3333-3333-333333333002',
    'CED', '0923456789',
    'Ana Maria', 'Lopez Vargas',
    'ana.lopez@gmail.com',
    '+593 98 765-4321',
    'Av. Francisco de Orellana, Cdla. Kennedy Norte, Guayaquil',
    'vendedor'
),
-- Cliente 3: Pedro Garcia - Cuenca, walk-in (sin cuenta en portal)
(
    '33333333-3333-3333-3333-333333333003',
    'CED', '1098765432',
    'Pedro Antonio', 'Garcia Reyes',
    'pedro.garcia@hotmail.com',
    '+593 97 555-0011',
    'Calle Bolivar 10-25 y Padre Aguirre, Cuenca',
    'vendedor'
);


-- ============================================================
-- RESERVAS (prefijo GUID: 99xxxxxx-...)
-- ============================================================

-- ----------------------------------------------------------
-- RESERVA 001: Juan Perez — Quito — CONFIRMADA (futura)
--
-- Escenario: Juan reservo 5 noches en la Suite Doble de Quito
-- (hab 102) para junio. La reserva esta confirmada pero aun
-- no ha realizado el pago ni el check-in.
--
-- Matematica:
--   5 noches x $120.00 = $600.00 subtotal
--   IVA 15%            =  $90.00
--   Total              = $690.00
--   Saldo pendiente    = $690.00 (sin pagar)
-- ----------------------------------------------------------
INSERT INTO reservas.reserva (
    reserva_guid, codigo_reserva,
    id_cliente, sucursal_guid,
    fecha_inicio, fecha_fin,
    subtotal_reserva, valor_iva, total_reserva,
    descuento_aplicado, saldo_pendiente,
    origen_canal_reserva, estado_reserva,
    fecha_confirmacion_utc,
    observaciones,
    es_walkin, creado_por_usuario
) VALUES
(
    '99999999-9999-9999-9999-999999999001',
    'RES-2026-000001',
    (SELECT id_cliente FROM reservas.cliente WHERE cliente_guid = '33333333-3333-3333-3333-333333333001'),
    '44444444-4444-4444-4444-444444444001',          -- sucursal Quito
    '2026-06-10 15:00:00+00',
    '2026-06-15 12:00:00+00',
    600.00, 90.00, 690.00,
    0.00, 690.00,
    'PORTAL', 'CON',
    '2026-06-01 09:15:00+00',
    'Reserva confirmada via portal web. Cliente solicita habitacion en piso alto de ser posible. Pago pendiente previo al check-in.',
    FALSE, 'vendedor'
);


-- ----------------------------------------------------------
-- RESERVA 002: Ana Lopez — Guayaquil — EN MARCHA (activa)
--
-- Escenario: Ana realizo el check-in el 9 de mayo. Actualmente
-- se encuentra hospedada en la Suite Familiar (hab 201 GYE).
-- La reserva ya fue pagada en su totalidad al confirmarla.
--
-- Matematica:
--   4 noches x $190.00 = $760.00 subtotal
--   IVA 15%            = $114.00
--   Total              = $874.00
--   Saldo pendiente    =   $0.00 (pagada)
-- ----------------------------------------------------------
INSERT INTO reservas.reserva (
    reserva_guid, codigo_reserva,
    id_cliente, sucursal_guid,
    fecha_inicio, fecha_fin,
    subtotal_reserva, valor_iva, total_reserva,
    descuento_aplicado, saldo_pendiente,
    origen_canal_reserva, estado_reserva,
    fecha_confirmacion_utc,
    observaciones,
    es_walkin, creado_por_usuario
) VALUES
(
    '99999999-9999-9999-9999-999999999002',
    'RES-2026-000002',
    (SELECT id_cliente FROM reservas.cliente WHERE cliente_guid = '33333333-3333-3333-3333-333333333002'),
    '44444444-4444-4444-4444-444444444002',          -- sucursal Guayaquil
    '2026-05-09 15:00:00+00',
    '2026-05-13 12:00:00+00',
    760.00, 114.00, 874.00,
    0.00, 0.00,
    'PORTAL', 'EMI',
    '2026-05-02 11:30:00+00',
    'Reserva pagada en linea con tarjeta de credito. Check-in realizado el 2026-05-09. Huesped solicito cuna adicional para infante.',
    FALSE, 'vendedor'
);


-- ----------------------------------------------------------
-- RESERVA 003: Pedro Garcia — Cuenca — FINALIZADA (historica)
--
-- Escenario: Pedro se presento directamente en recepcion
-- (walk-in) y se hospedo 3 noches en la Suite Single de
-- Cuenca. Pago en efectivo al ingreso. Estadia ya completada.
--
-- Matematica:
--    3 noches x $75.00 = $225.00 subtotal
--    IVA 15%           =  $33.75
--    Total             = $258.75
--    Saldo pendiente   =   $0.00 (pagado en caja)
-- ----------------------------------------------------------
INSERT INTO reservas.reserva (
    reserva_guid, codigo_reserva,
    id_cliente, sucursal_guid,
    fecha_inicio, fecha_fin,
    subtotal_reserva, valor_iva, total_reserva,
    descuento_aplicado, saldo_pendiente,
    origen_canal_reserva, estado_reserva,
    fecha_confirmacion_utc,
    observaciones,
    es_walkin, creado_por_usuario
) VALUES
(
    '99999999-9999-9999-9999-999999999003',
    'RES-2026-000003',
    (SELECT id_cliente FROM reservas.cliente WHERE cliente_guid = '33333333-3333-3333-3333-333333333003'),
    '44444444-4444-4444-4444-444444444003',          -- sucursal Cuenca
    '2026-04-20 14:00:00+00',
    '2026-04-23 12:00:00+00',
    225.00, 33.75, 258.75,
    0.00, 0.00,
    'WALKIN', 'FIN',
    '2026-04-20 14:05:00+00',
    'Cliente walk-in. Cedula verificada. Pago en efectivo realizado en caja al ingreso. Estadia sin novedades.',
    TRUE, 'vendedor'
);


-- ============================================================
-- LINEAS DE RESERVA (reserva_habitacion)
-- prefijo GUID: a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1XX
--
-- Cada linea referencia logicamente una habitacion y tarifa
-- de HotelLux_Accommodation (sin FK fisica).
-- ============================================================

-- ----------------------------------------------------------
-- Linea 001: Juan Perez — Suite Doble Quito hab 102
--   habitacion_guid: 66...002 (hab 102 Quito Doble)
--   tarifa_guid    : 77...002 (TAR-UIO-DOBLE-2026)
--   5 noches x $120.00 | subtotal $600.00 | IVA $90.00 | total $690.00
-- ----------------------------------------------------------
INSERT INTO reservas.reserva_habitacion (
    reserva_habitacion_guid,
    id_reserva, habitacion_guid, tarifa_guid,
    fecha_inicio, fecha_fin,
    num_adultos, num_ninos,
    precio_noche_aplicado,
    subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    estado_detalle, creado_por_usuario
) VALUES
(
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a101',
    (SELECT id_reserva FROM reservas.reserva WHERE reserva_guid = '99999999-9999-9999-9999-999999999001'),
    '66666666-6666-6666-6666-666666666002',          -- hab 102 Quito Suite Doble
    '77777777-7777-7777-7777-777777777002',          -- tarifa TAR-UIO-DOBLE-2026
    '2026-06-10 15:00:00+00',
    '2026-06-15 12:00:00+00',
    2, 0,
    120.00,
    600.00, 90.00, 0.00, 690.00,
    'CON', 'vendedor'
);

-- ----------------------------------------------------------
-- Linea 002: Ana Lopez — Suite Familiar Guayaquil hab 201
--   habitacion_guid: 66...007 (hab 201 GYE Familiar)
--   tarifa_guid    : 77...006 (TAR-GYE-FAMILIAR-2026)
--   4 noches x $190.00 | subtotal $760.00 | IVA $114.00 | total $874.00
-- ----------------------------------------------------------
INSERT INTO reservas.reserva_habitacion (
    reserva_habitacion_guid,
    id_reserva, habitacion_guid, tarifa_guid,
    fecha_inicio, fecha_fin,
    num_adultos, num_ninos,
    precio_noche_aplicado,
    subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    estado_detalle, creado_por_usuario
) VALUES
(
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a102',
    (SELECT id_reserva FROM reservas.reserva WHERE reserva_guid = '99999999-9999-9999-9999-999999999002'),
    '66666666-6666-6666-6666-666666666007',          -- hab 201 GYE Suite Familiar
    '77777777-7777-7777-7777-777777777006',          -- tarifa TAR-GYE-FAMILIAR-2026
    '2026-05-09 15:00:00+00',
    '2026-05-13 12:00:00+00',
    2, 1,
    190.00,
    760.00, 114.00, 0.00, 874.00,
    'EMI', 'vendedor'
);

-- ----------------------------------------------------------
-- Linea 003: Pedro Garcia — Suite Single Cuenca hab 101
--   habitacion_guid: 66...008 (hab 101 Cuenca Single)
--   tarifa_guid    : 77...007 (TAR-CUE-SINGLE-2026)
--   3 noches x $75.00 | subtotal $225.00 | IVA $33.75 | total $258.75
-- ----------------------------------------------------------
INSERT INTO reservas.reserva_habitacion (
    reserva_habitacion_guid,
    id_reserva, habitacion_guid, tarifa_guid,
    fecha_inicio, fecha_fin,
    num_adultos, num_ninos,
    precio_noche_aplicado,
    subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    estado_detalle, creado_por_usuario
) VALUES
(
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a103',
    (SELECT id_reserva FROM reservas.reserva WHERE reserva_guid = '99999999-9999-9999-9999-999999999003'),
    '66666666-6666-6666-6666-666666666008',          -- hab 101 Cuenca Suite Single
    '77777777-7777-7777-7777-777777777007',          -- tarifa TAR-CUE-SINGLE-2026
    '2026-04-20 14:00:00+00',
    '2026-04-23 12:00:00+00',
    1, 0,
    75.00,
    225.00, 33.75, 0.00, 258.75,
    'FIN', 'vendedor'
);


-- ============================================================
-- VERIFICACION FINAL
-- Resultados esperados:
--   Clientes          : 3
--   Reservas          : 3
--   Lineas de reserva : 3
-- ============================================================
SELECT 'Clientes:           ' || COUNT(*)::text AS resultado FROM reservas.cliente;
SELECT 'Reservas:           ' || COUNT(*)::text AS resultado FROM reservas.reserva;
SELECT 'Lineas de reserva:  ' || COUNT(*)::text AS resultado FROM reservas.reserva_habitacion;

-- Vista resumen completa
SELECT
    r.codigo_reserva,
    c.nombres || ' ' || COALESCE(c.apellidos, '')   AS cliente,
    c.numero_identificacion                          AS cedula,
    r.fecha_inicio::date                             AS desde,
    r.fecha_fin::date                                AS hasta,
    (r.fecha_fin::date - r.fecha_inicio::date)       AS noches,
    r.subtotal_reserva                               AS subtotal_usd,
    r.valor_iva                                      AS iva_usd,
    r.total_reserva                                  AS total_usd,
    r.saldo_pendiente                                AS saldo_usd,
    r.estado_reserva                                 AS estado,
    r.origen_canal_reserva                           AS canal,
    r.es_walkin
FROM   reservas.reserva r
JOIN   reservas.cliente c ON c.id_cliente = r.id_cliente
ORDER  BY r.fecha_inicio;