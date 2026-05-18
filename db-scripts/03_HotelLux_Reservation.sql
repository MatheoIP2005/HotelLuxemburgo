-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio RESERVATION
-- Base de datos: HotelLux_Reservation
-- Motor: PostgreSQL 18
-- Version: 1.0
--
-- DEPENDENCIAS LOGICAS (sin FK fisica):
--   - HotelLux_Accommodation: sucursal_guid, habitacion_guid, tarifa_guid
--   - HotelLux_Auth:          ningun cruce (cliente_guid se replica)
--
-- CONTENIDO:
--   Schema: reservas
--   Tablas: cliente, reserva, reserva_habitacion
--   Datos semilla: 3 clientes, 3 reservas (CON, EMI, FIN), 3 lineas
--
-- INSTRUCCIONES EN pgAdmin:
--   1. Create Database: HotelLux_Reservation
--   2. Query Tool -> Open file -> F5
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
    tipo_identificacion      VARCHAR(20)  NOT NULL,         -- CED | RUC | PAS
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
-- Las FK a SUCURSAL y HABITACION se reemplazan por GUIDs logicos
-- porque ahora viven en otro microservicio (Accommodation).
-- ============================================================
CREATE TABLE reservas.reserva (
    id_reserva               INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    reserva_guid             UUID         NOT NULL DEFAULT gen_random_uuid(),
    codigo_reserva           VARCHAR(40)  NOT NULL,
    id_cliente               INT          NOT NULL,                            -- FK local
    sucursal_guid            UUID         NOT NULL,                            -- referencia logica a Accommodation
    fecha_reserva_utc        TIMESTAMPTZ  NOT NULL DEFAULT now(),
    fecha_inicio             TIMESTAMPTZ  NOT NULL,
    fecha_fin                TIMESTAMPTZ  NOT NULL,
    subtotal_reserva         NUMERIC(12,2) NOT NULL,
    valor_iva                NUMERIC(12,2) NOT NULL,
    total_reserva            NUMERIC(12,2) NOT NULL,
    descuento_aplicado       NUMERIC(12,2) NOT NULL DEFAULT 0,
    saldo_pendiente          NUMERIC(12,2) NOT NULL DEFAULT 0,
    origen_canal_reserva     VARCHAR(50)  NOT NULL,
    estado_reserva           CHAR(3)      NOT NULL DEFAULT 'PEN',
    fecha_confirmacion_utc   TIMESTAMPTZ  NULL,
    fecha_cancelacion_utc    TIMESTAMPTZ  NULL,
    motivo_cancelacion       VARCHAR(250) NULL,
    observaciones            TEXT         NULL,
    es_walkin                BOOLEAN      NOT NULL DEFAULT FALSE,
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    creado_por_usuario       VARCHAR(100) NOT NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    modificado_por_usuario   VARCHAR(100) NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          VARCHAR(45)  NULL,
    servicio_origen          VARCHAR(50)  NOT NULL DEFAULT 'reservation-service',
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    motivo_inhabilitacion    VARCHAR(250) NULL,
    CONSTRAINT uq_reserva_guid   UNIQUE (reserva_guid),
    CONSTRAINT uq_reserva_codigo UNIQUE (codigo_reserva),
    CONSTRAINT fk_reserva_cliente FOREIGN KEY (id_cliente)
        REFERENCES reservas.cliente(id_cliente),
    -- PEN=Pendiente | CON=Confirmada | CAN=Cancelada | EXP=Expirada | FIN=Finalizada | EMI=En marcha (post check-in)
    CONSTRAINT chk_reserva_estado  CHECK (estado_reserva IN ('PEN','CON','CAN','EXP','FIN','EMI')),
    CONSTRAINT chk_reserva_subtotal CHECK (subtotal_reserva >= 0),
    CONSTRAINT chk_reserva_iva      CHECK (valor_iva >= 0),
    CONSTRAINT chk_reserva_total    CHECK (total_reserva >= 0),
    CONSTRAINT chk_reserva_desc     CHECK (descuento_aplicado >= 0),
    CONSTRAINT chk_reserva_saldo    CHECK (saldo_pendiente >= 0),
    CONSTRAINT chk_reserva_fechas   CHECK (fecha_fin > fecha_inicio),
    CONSTRAINT chk_reserva_coherente CHECK (total_reserva >= subtotal_reserva - descuento_aplicado)
);


-- ============================================================
-- TABLA: reservas.reserva_habitacion
-- Linea por habitacion dentro de una reserva. Las referencias
-- a habitacion y tarifa son GUIDs logicos (Accommodation).
-- ============================================================
CREATE TABLE reservas.reserva_habitacion (
    id_reserva_habitacion    INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    reserva_habitacion_guid  UUID         NOT NULL DEFAULT gen_random_uuid(),
    id_reserva               INT          NOT NULL,
    habitacion_guid          UUID         NOT NULL,                     -- referencia logica a Accommodation
    tarifa_guid              UUID         NULL,                          -- referencia logica a Accommodation
    fecha_inicio             TIMESTAMPTZ  NOT NULL,
    fecha_fin                TIMESTAMPTZ  NOT NULL,
    num_adultos              INT          NOT NULL DEFAULT 1,
    num_ninos                INT          NOT NULL DEFAULT 0,
    precio_noche_aplicado    NUMERIC(12,2) NOT NULL,
    subtotal_linea           NUMERIC(12,2) NOT NULL,
    valor_iva_linea          NUMERIC(12,2) NOT NULL,
    descuento_linea          NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_linea              NUMERIC(12,2) NOT NULL,
    estado_detalle           CHAR(3)      NOT NULL DEFAULT 'PEN',
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100) NOT NULL,
    modificado_por_usuario   VARCHAR(100) NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          VARCHAR(45)  NULL,
    servicio_origen          VARCHAR(50)  NOT NULL DEFAULT 'reservation-service',
    CONSTRAINT uq_reserva_hab_guid  UNIQUE (reserva_habitacion_guid),
    CONSTRAINT uq_reserva_hab_linea UNIQUE (id_reserva, habitacion_guid, fecha_inicio),
    CONSTRAINT fk_reserva_hab_reserva FOREIGN KEY (id_reserva)
        REFERENCES reservas.reserva(id_reserva),
    CONSTRAINT chk_reserva_hab_fechas   CHECK (fecha_fin > fecha_inicio),
    CONSTRAINT chk_reserva_hab_adultos  CHECK (num_adultos > 0),
    CONSTRAINT chk_reserva_hab_ninos    CHECK (num_ninos >= 0),
    CONSTRAINT chk_reserva_hab_precio   CHECK (precio_noche_aplicado > 0),
    CONSTRAINT chk_reserva_hab_subtotal CHECK (subtotal_linea >= 0),
    CONSTRAINT chk_reserva_hab_iva      CHECK (valor_iva_linea >= 0),
    CONSTRAINT chk_reserva_hab_desc     CHECK (descuento_linea >= 0),
    CONSTRAINT chk_reserva_hab_total    CHECK (total_linea >= 0),
    CONSTRAINT chk_reserva_hab_estado   CHECK (estado_detalle IN ('PEN','CON','CAN','FIN','EMI'))
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
-- DATOS SEMILLA
--
-- CLIENTES (33xxxxxx-...):
--   001 = Juan Carlos Perez   (vinculado a usuario juan.perez en Auth)
--   002 = Ana Maria Lopez     (vinculada a usuario ana.lopez en Auth)
--   003 = Pedro Antonio Garcia (sin cuenta, walk-in)
-- ============================================================

INSERT INTO reservas.cliente (
    cliente_guid, tipo_identificacion, numero_identificacion,
    nombres, apellidos, correo, telefono, direccion,
    creado_por_usuario
) VALUES
('33333333-3333-3333-3333-333333333001', 'CED', '1712345678',
 'Juan Carlos', 'Perez Mendoza', 'juan.perez@gmail.com', '+593 99 123 4567',
 'Av. Eloy Alfaro N32-110, Quito', 'system'),

('33333333-3333-3333-3333-333333333002', 'CED', '0923456789',
 'Ana Maria', 'Lopez Vargas', 'ana.lopez@gmail.com', '+593 98 765 4321',
 'Av. Las Aguas 305 y Mz J, Guayaquil', 'system'),

('33333333-3333-3333-3333-333333333003', 'CED', '1098765432',
 'Pedro Antonio', 'Garcia Cordova', 'pedro.garcia@hotmail.com', '+593 97 555 0011',
 'Calle Bolivar 10-25, Cuenca', 'system');


-- ============================================================
-- RESERVAS (99xxxxxx-...):
--   001 = Juan Perez en Quito,     Suite Doble 102,    CONFIRMADA (futura)
--   002 = Ana Lopez en Guayaquil,  Suite Familiar 201, EMI (check-in hecho)
--   003 = Pedro Garcia en Cuenca,  Suite Single 101,   FINALIZADA (pasada)
-- ============================================================

-- Reserva 1: Juan Perez en Quito, 5 noches, CONFIRMADA, futura
-- 5 noches x 120 = 600 subtotal, IVA 15% = 90, total 690
INSERT INTO reservas.reserva (
    reserva_guid, codigo_reserva, id_cliente, sucursal_guid,
    fecha_inicio, fecha_fin,
    subtotal_reserva, valor_iva, total_reserva, saldo_pendiente,
    origen_canal_reserva, estado_reserva, fecha_confirmacion_utc,
    observaciones, creado_por_usuario
) VALUES
('99999999-9999-9999-9999-999999999001',
 'RES-2026-000001',
 (SELECT id_cliente FROM reservas.cliente WHERE cliente_guid='33333333-3333-3333-3333-333333333001'),
 '44444444-4444-4444-4444-444444444001',          -- sucursal Quito
 '2026-06-10 15:00:00+00', '2026-06-15 12:00:00+00',
 600.00, 90.00, 690.00, 690.00,
 'PORTAL', 'CON', now(),
 'Reserva confirmada vio portal publico. Pago pendiente.',
 'juan.perez');

-- Reserva 2: Ana Lopez en GYE, 4 noches, EMI (en curso), check-in hoy
-- 4 noches x 190 = 760 subtotal, IVA 15% = 114, total 874
INSERT INTO reservas.reserva (
    reserva_guid, codigo_reserva, id_cliente, sucursal_guid,
    fecha_inicio, fecha_fin,
    subtotal_reserva, valor_iva, total_reserva, saldo_pendiente,
    origen_canal_reserva, estado_reserva, fecha_confirmacion_utc,
    observaciones, creado_por_usuario
) VALUES
('99999999-9999-9999-9999-999999999002',
 'RES-2026-000002',
 (SELECT id_cliente FROM reservas.cliente WHERE cliente_guid='33333333-3333-3333-3333-333333333002'),
 '44444444-4444-4444-4444-444444444002',          -- sucursal GYE
 '2026-05-09 15:00:00+00', '2026-05-13 12:00:00+00',
 760.00, 114.00, 874.00, 0.00,                    -- ya pagada
 'PORTAL', 'EMI', '2026-05-05 10:00:00+00',
 'Reserva con check-in realizado. En curso.',
 'vendedor1');

-- Reserva 3: Pedro Garcia en Cuenca, 3 noches, FINALIZADA (pasada)
-- 3 noches x 75 = 225 subtotal, IVA 15% = 33.75, total 258.75
INSERT INTO reservas.reserva (
    reserva_guid, codigo_reserva, id_cliente, sucursal_guid,
    fecha_inicio, fecha_fin,
    subtotal_reserva, valor_iva, total_reserva, saldo_pendiente,
    origen_canal_reserva, estado_reserva, fecha_confirmacion_utc,
    observaciones, es_walkin, creado_por_usuario
) VALUES
('99999999-9999-9999-9999-999999999003',
 'RES-2026-000003',
 (SELECT id_cliente FROM reservas.cliente WHERE cliente_guid='33333333-3333-3333-3333-333333333003'),
 '44444444-4444-4444-4444-444444444003',          -- sucursal Cuenca
 '2026-04-20 15:00:00+00', '2026-04-23 12:00:00+00',
 225.00, 33.75, 258.75, 0.00,                     -- pagada y completada
 'WALKIN', 'FIN', '2026-04-20 14:30:00+00',
 'Cliente walk-in. Estadia completada exitosamente.',
 TRUE, 'vendedor1');


-- ============================================================
-- LINEAS DE RESERVA (reserva_habitacion)
-- ============================================================

-- Linea de la reserva 1: Suite Doble Quito 102
INSERT INTO reservas.reserva_habitacion (
    reserva_habitacion_guid, id_reserva, habitacion_guid, tarifa_guid,
    fecha_inicio, fecha_fin, num_adultos, num_ninos,
    precio_noche_aplicado, subtotal_linea, valor_iva_linea, total_linea,
    estado_detalle, creado_por_usuario
) VALUES
('a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a101',
 (SELECT id_reserva FROM reservas.reserva WHERE reserva_guid='99999999-9999-9999-9999-999999999001'),
 '66666666-6666-6666-6666-666666666002',          -- habitacion Quito 102 doble
 '77777777-7777-7777-7777-777777777002',          -- tarifa UIO-DOBLE-2026
 '2026-06-10 15:00:00+00', '2026-06-15 12:00:00+00',
 2, 0,
 120.00, 600.00, 90.00, 690.00,
 'CON', 'juan.perez');

-- Linea de la reserva 2: Suite Familiar GYE 201
INSERT INTO reservas.reserva_habitacion (
    reserva_habitacion_guid, id_reserva, habitacion_guid, tarifa_guid,
    fecha_inicio, fecha_fin, num_adultos, num_ninos,
    precio_noche_aplicado, subtotal_linea, valor_iva_linea, total_linea,
    estado_detalle, creado_por_usuario
) VALUES
('a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a102',
 (SELECT id_reserva FROM reservas.reserva WHERE reserva_guid='99999999-9999-9999-9999-999999999002'),
 '66666666-6666-6666-6666-666666666007',          -- habitacion GYE 201 familiar
 '77777777-7777-7777-7777-777777777006',          -- tarifa GYE-FAMILIAR-2026
 '2026-05-09 15:00:00+00', '2026-05-13 12:00:00+00',
 2, 2,
 190.00, 760.00, 114.00, 874.00,
 'EMI', 'vendedor1');

-- Linea de la reserva 3: Suite Single Cuenca 101
INSERT INTO reservas.reserva_habitacion (
    reserva_habitacion_guid, id_reserva, habitacion_guid, tarifa_guid,
    fecha_inicio, fecha_fin, num_adultos, num_ninos,
    precio_noche_aplicado, subtotal_linea, valor_iva_linea, total_linea,
    estado_detalle, creado_por_usuario
) VALUES
('a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a103',
 (SELECT id_reserva FROM reservas.reserva WHERE reserva_guid='99999999-9999-9999-9999-999999999003'),
 '66666666-6666-6666-6666-666666666008',          -- habitacion Cuenca 101 single
 '77777777-7777-7777-7777-777777777007',          -- tarifa CUE-SINGLE-2026
 '2026-04-20 15:00:00+00', '2026-04-23 12:00:00+00',
 1, 0,
 75.00, 225.00, 33.75, 258.75,
 'FIN', 'vendedor1');


-- ============================================================
-- VERIFICACION FINAL
-- ============================================================
SELECT 'Clientes:           ' || COUNT(*)::text FROM reservas.cliente;
SELECT 'Reservas:           ' || COUNT(*)::text FROM reservas.reserva;
SELECT 'Lineas de reserva:  ' || COUNT(*)::text FROM reservas.reserva_habitacion;

-- Vista resumen: reservas con datos del cliente
SELECT
    r.codigo_reserva,
    c.nombres || ' ' || COALESCE(c.apellidos,'') AS cliente,
    r.fecha_inicio::date AS desde,
    r.fecha_fin::date    AS hasta,
    r.total_reserva,
    r.saldo_pendiente,
    r.estado_reserva,
    r.origen_canal_reserva
FROM   reservas.reserva r
JOIN   reservas.cliente c ON c.id_cliente = r.id_cliente
ORDER  BY r.fecha_inicio;
