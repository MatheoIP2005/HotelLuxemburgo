-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio FINANCE
-- Base de datos: HotelLux_Finance
-- Motor: PostgreSQL 18
-- Version: 1.0
--
-- ALCANCE: este microservicio FUSIONA billing + payment del plan
-- original (decision: 6 microservicios). Maneja facturacion,
-- detalle de facturas y pagos en una sola BD para garantizar
-- transacciones ACID locales en el endpoint
-- /facturas/final-y-pago-simulado.
--
-- DEPENDENCIAS LOGICAS (sin FK fisica):
--   - HotelLux_Reservation:    reserva_guid
--   - HotelLux_Reservation:    cliente_guid (replica desde clientes)
--   - HotelLux_Accommodation:  sucursal_guid
--
-- CONTENIDO:
--   Schema: finanzas
--   Tablas: factura, factura_detalle, pago
--   Datos semilla:
--     4 facturas: 1 emitida pendiente (Juan), 2 RESERVA pagadas
--                 (Ana, Pedro), 1 FINAL pagada (Pedro spa)
--     4 lineas de detalle
--     3 pagos aprobados
--
-- INSTRUCCIONES EN pgAdmin:
--   1. Create Database: HotelLux_Finance
--   2. Query Tool -> Open file -> F5
-- ============================================================


-- ============================================================
-- SCHEMA
-- ============================================================
CREATE SCHEMA IF NOT EXISTS finanzas;


-- ============================================================
-- TABLA: finanzas.factura
-- ============================================================
CREATE TABLE finanzas.factura (
    id_factura               INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    factura_guid             UUID         NOT NULL DEFAULT gen_random_uuid(),
    cliente_guid             UUID         NOT NULL,                       -- ref logica
    reserva_guid             UUID         NOT NULL,                       -- ref logica
    sucursal_guid            UUID         NOT NULL,                       -- ref logica
    numero_factura           VARCHAR(40)  NOT NULL,
    tipo_factura             VARCHAR(20)  NOT NULL DEFAULT 'RESERVA',
    fecha_emision            TIMESTAMPTZ  NOT NULL DEFAULT now(),
    subtotal                 NUMERIC(12,2) NOT NULL,
    valor_iva                NUMERIC(12,2) NOT NULL,
    descuento_total          NUMERIC(12,2) NOT NULL DEFAULT 0,
    total                    NUMERIC(12,2) NOT NULL,
    saldo_pendiente          NUMERIC(12,2) NOT NULL DEFAULT 0,
    moneda                   VARCHAR(10)  NOT NULL DEFAULT 'USD',
    observaciones_factura    VARCHAR(300) NULL,
    origen_canal_factura     VARCHAR(50)  NULL,
    estado                   CHAR(3)      NOT NULL DEFAULT 'EMI',
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    creado_por_usuario       VARCHAR(100) NOT NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    modificado_por_usuario   VARCHAR(100) NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          VARCHAR(45)  NULL,
    servicio_origen          VARCHAR(50)  NOT NULL DEFAULT 'finance-service',
    motivo_inhabilitacion    VARCHAR(250) NULL,
    CONSTRAINT uq_factura_guid    UNIQUE (factura_guid),
    CONSTRAINT uq_factura_numero  UNIQUE (numero_factura),
    -- RESERVA = factura inicial al confirmar la reserva (alojamiento)
    -- FINAL   = factura de checkout con cargos de estadia adicionales
    -- AJUSTE  = correccion manual si aplica
    CONSTRAINT chk_factura_tipo      CHECK (tipo_factura IN ('RESERVA','FINAL','AJUSTE')),
    -- EMI=Emitida (recien generada) | PAG=Pagada totalmente | ANU=Anulada
    CONSTRAINT chk_factura_estado    CHECK (estado IN ('EMI','PAG','ANU')),
    CONSTRAINT chk_factura_subtotal  CHECK (subtotal >= 0),
    CONSTRAINT chk_factura_iva       CHECK (valor_iva >= 0),
    CONSTRAINT chk_factura_descuento CHECK (descuento_total >= 0),
    CONSTRAINT chk_factura_total     CHECK (total >= 0),
    CONSTRAINT chk_factura_saldo     CHECK (saldo_pendiente >= 0),
    CONSTRAINT chk_factura_coherente CHECK (total >= subtotal - descuento_total)
);


-- ============================================================
-- TABLA: finanzas.factura_detalle
-- Snapshot inmutable del detalle de la factura. Nunca se edita
-- manualmente; lo puebla la logica de generacion (antes SP).
-- ============================================================
CREATE TABLE finanzas.factura_detalle (
    id_factura_detalle       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    factura_detalle_guid     UUID         NOT NULL DEFAULT gen_random_uuid(),
    id_factura               INT          NOT NULL,                          -- FK local
    tipo_item                VARCHAR(30)  NOT NULL,
    -- referencia_origen apunta logicamente a tablas de OTROS microservicios
    -- (reserva_habitacion / cargo_estadia). Es VARCHAR + GUID, sin FK.
    referencia_tipo          VARCHAR(30)  NULL,
    referencia_guid          UUID         NULL,
    descripcion_item         VARCHAR(250) NOT NULL,
    cantidad                 INT          NOT NULL DEFAULT 1,
    precio_unitario          NUMERIC(12,2) NOT NULL,
    subtotal_linea           NUMERIC(12,2) NOT NULL,
    valor_iva_linea          NUMERIC(12,2) NOT NULL DEFAULT 0,
    descuento_linea          NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_linea              NUMERIC(12,2) NOT NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100) NOT NULL,
    CONSTRAINT uq_factura_detalle_guid UNIQUE (factura_detalle_guid),
    CONSTRAINT fk_factura_detalle_factura FOREIGN KEY (id_factura)
        REFERENCES finanzas.factura(id_factura) ON DELETE CASCADE,
    CONSTRAINT chk_factura_detalle_tipo     CHECK (tipo_item IN ('ALOJAMIENTO','SERVICIO','DESCUENTO','AJUSTE')),
    CONSTRAINT chk_factura_detalle_cantidad CHECK (cantidad > 0),
    CONSTRAINT chk_factura_detalle_precio   CHECK (precio_unitario >= 0),
    CONSTRAINT chk_factura_detalle_subtotal CHECK (subtotal_linea >= 0),
    CONSTRAINT chk_factura_detalle_iva      CHECK (valor_iva_linea >= 0),
    CONSTRAINT chk_factura_detalle_desc     CHECK (descuento_linea >= 0),
    CONSTRAINT chk_factura_detalle_total    CHECK (total_linea >= 0)
);


-- ============================================================
-- TABLA: finanzas.pago
-- Antes vivia en payment-service. Fusionada aqui para evitar
-- gRPC en UpdateInvoiceBalance / MarkInvoicePaid (ahora son
-- transacciones ACID locales).
-- ============================================================
CREATE TABLE finanzas.pago (
    id_pago                  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    pago_guid                UUID         NOT NULL DEFAULT gen_random_uuid(),
    id_factura               INT          NOT NULL,                          -- FK local
    reserva_guid             UUID         NOT NULL,                          -- ref logica
    monto                    NUMERIC(12,2) NOT NULL,
    metodo_pago              VARCHAR(40)  NOT NULL,
    es_pago_electronico      BOOLEAN      NOT NULL DEFAULT FALSE,
    proveedor_pasarela       VARCHAR(50)  NULL,
    transaccion_externa      VARCHAR(150) NULL,
    codigo_autorizacion      VARCHAR(150) NULL,
    referencia               VARCHAR(150) NULL,
    estado_pago              CHAR(3)      NOT NULL DEFAULT 'PEN',
    fecha_pago_utc           TIMESTAMPTZ  NOT NULL DEFAULT now(),
    moneda                   VARCHAR(10)  NOT NULL DEFAULT 'USD',
    tipo_cambio              NUMERIC(10,4) NOT NULL DEFAULT 1.0000,
    respuesta_pasarela       TEXT         NULL,
    creado_por_usuario       VARCHAR(100) NOT NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    modificado_por_usuario   VARCHAR(100) NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          VARCHAR(45)  NULL,
    servicio_origen          VARCHAR(50)  NOT NULL DEFAULT 'finance-service',
    CONSTRAINT uq_pago_guid   UNIQUE (pago_guid),
    CONSTRAINT fk_pago_factura FOREIGN KEY (id_factura)
        REFERENCES finanzas.factura(id_factura),
    CONSTRAINT chk_pago_monto CHECK (monto > 0),
    -- PEN=Pendiente | PRO=Procesando | APR=Aprobado | REC=Rechazado | CAN=Cancelado
    CONSTRAINT chk_pago_estado     CHECK (estado_pago IN ('PEN','PRO','APR','REC','CAN')),
    CONSTRAINT chk_pago_tipo_cambio CHECK (tipo_cambio > 0)
);


-- ============================================================
-- INDICES DE APOYO
-- ============================================================
CREATE INDEX ix_factura_reserva_estado
    ON finanzas.factura(reserva_guid, estado, fecha_emision);

CREATE INDEX ix_factura_cliente
    ON finanzas.factura(cliente_guid, fecha_emision DESC);

CREATE INDEX ix_factura_sucursal
    ON finanzas.factura(sucursal_guid, fecha_emision DESC);

CREATE INDEX ix_pago_factura_estado
    ON finanzas.pago(id_factura, estado_pago, fecha_pago_utc);

CREATE INDEX ix_pago_reserva
    ON finanzas.pago(reserva_guid);

-- Idempotencia: dos pagos distintos no pueden compartir transaccion externa
CREATE UNIQUE INDEX uq_pago_transaccion_externa
    ON finanzas.pago(transaccion_externa)
    WHERE transaccion_externa IS NOT NULL;


-- ============================================================
-- DATOS SEMILLA
--
-- FACTURAS (bbxxxxxx-...):
--   bb...0001 = RESERVA Juan Perez (EMI, pendiente de pago)
--   bb...0002 = RESERVA Ana Lopez  (PAG, totalmente pagada)
--   bb...0003 = RESERVA Pedro      (PAG, walk-in pagado al ingreso)
--   bb...0004 = FINAL   Pedro      (PAG, cargo de spa al checkout)
-- ============================================================

-- Factura 1: Juan Perez (RESERVA Quito, EMI pendiente)
INSERT INTO finanzas.factura (
    factura_guid, cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura, fecha_emision,
    subtotal, valor_iva, total, saldo_pendiente,
    estado, origen_canal_factura, observaciones_factura,
    creado_por_usuario
) VALUES
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0001',
 '33333333-3333-3333-3333-333333333001',                  -- cliente Juan
 '99999999-9999-9999-9999-999999999001',                  -- reserva 001
 '44444444-4444-4444-4444-444444444001',                  -- sucursal Quito
 'FAC-RES-2026-000001', 'RESERVA', '2026-05-11 10:00:00+00',
 600.00, 90.00, 690.00, 690.00,
 'EMI', 'PORTAL',
 'Factura inicial de alojamiento. Cliente debe pagar antes del check-in.',
 'juan.perez');

-- Factura 2: Ana Lopez (RESERVA GYE, PAG)
INSERT INTO finanzas.factura (
    factura_guid, cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura, fecha_emision,
    subtotal, valor_iva, total, saldo_pendiente,
    estado, origen_canal_factura, observaciones_factura,
    creado_por_usuario
) VALUES
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002',
 '33333333-3333-3333-3333-333333333002',                  -- cliente Ana
 '99999999-9999-9999-9999-999999999002',                  -- reserva 002
 '44444444-4444-4444-4444-444444444002',                  -- sucursal GYE
 'FAC-RES-2026-000002', 'RESERVA', '2026-05-05 11:00:00+00',
 760.00, 114.00, 874.00, 0.00,
 'PAG', 'PORTAL',
 'Factura de alojamiento. Pago electronico aprobado al confirmar reserva.',
 'vendedor1');

-- Factura 3: Pedro Garcia (RESERVA Cuenca walk-in, PAG)
INSERT INTO finanzas.factura (
    factura_guid, cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura, fecha_emision,
    subtotal, valor_iva, total, saldo_pendiente,
    estado, origen_canal_factura, observaciones_factura,
    creado_por_usuario
) VALUES
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0003',
 '33333333-3333-3333-3333-333333333003',                  -- cliente Pedro
 '99999999-9999-9999-9999-999999999003',                  -- reserva 003
 '44444444-4444-4444-4444-444444444003',                  -- sucursal Cuenca
 'FAC-RES-2026-000003', 'RESERVA', '2026-04-20 14:35:00+00',
 225.00, 33.75, 258.75, 0.00,
 'PAG', 'WALKIN',
 'Cliente walk-in. Pago efectivo al ingreso.',
 'vendedor1');

-- Factura 4: Pedro Garcia (FINAL cargo spa, PAG)
INSERT INTO finanzas.factura (
    factura_guid, cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura, fecha_emision,
    subtotal, valor_iva, total, saldo_pendiente,
    estado, origen_canal_factura, observaciones_factura,
    creado_por_usuario
) VALUES
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0004',
 '33333333-3333-3333-3333-333333333003',                  -- cliente Pedro
 '99999999-9999-9999-9999-999999999003',                  -- reserva 003
 '44444444-4444-4444-4444-444444444003',                  -- sucursal Cuenca
 'FAC-FIN-2026-000001', 'FINAL', '2026-04-23 11:30:00+00',
 45.00, 6.75, 51.75, 0.00,
 'PAG', 'WALKIN',
 'Factura final con cargos de estadia (spa). Pago efectivo al checkout.',
 'vendedor1');


-- ============================================================
-- DETALLE DE FACTURAS
-- ============================================================

-- Detalle factura 1 (Juan Perez, alojamiento)
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura, tipo_item,
    referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, total_linea,
    creado_por_usuario
) VALUES
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd001',
 (SELECT id_factura FROM finanzas.factura WHERE factura_guid='bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0001'),
 'ALOJAMIENTO', 'RESERVA_HABITACION', 'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a101',
 'Suite Doble Quito 102 - 5 noche(s)', 5,
 120.00, 600.00, 90.00, 690.00,
 'juan.perez');

-- Detalle factura 2 (Ana Lopez, alojamiento)
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura, tipo_item,
    referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, total_linea,
    creado_por_usuario
) VALUES
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd002',
 (SELECT id_factura FROM finanzas.factura WHERE factura_guid='bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002'),
 'ALOJAMIENTO', 'RESERVA_HABITACION', 'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a102',
 'Suite Familiar GYE 201 - 4 noche(s)', 4,
 190.00, 760.00, 114.00, 874.00,
 'vendedor1');

-- Detalle factura 3 (Pedro RESERVA, alojamiento)
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura, tipo_item,
    referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, total_linea,
    creado_por_usuario
) VALUES
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd003',
 (SELECT id_factura FROM finanzas.factura WHERE factura_guid='bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0003'),
 'ALOJAMIENTO', 'RESERVA_HABITACION', 'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a103',
 'Suite Single Cuenca 101 - 3 noche(s)', 3,
 75.00, 225.00, 33.75, 258.75,
 'vendedor1');

-- Detalle factura 4 (Pedro FINAL, servicio de spa)
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura, tipo_item,
    referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, total_linea,
    creado_por_usuario
) VALUES
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd004',
 (SELECT id_factura FROM finanzas.factura WHERE factura_guid='bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0004'),
 'SERVICIO', 'CARGO_ESTADIA', 'cccccccc-cccc-cccc-cccc-cccccccc1003',
 'Spa - masaje relajante 60 min', 1,
 45.00, 45.00, 6.75, 51.75,
 'vendedor1');


-- ============================================================
-- PAGOS (cc...2xxx para diferenciarlos de cargos_estadia 1xxx)
-- Solo se generan pagos para facturas en estado PAG.
-- La factura 1 (Juan) NO tiene pago todavia: esta EMI pendiente.
-- ============================================================

-- Pago para factura 2 (Ana Lopez): tarjeta electronica
INSERT INTO finanzas.pago (
    pago_guid, id_factura, reserva_guid,
    monto, metodo_pago, es_pago_electronico,
    proveedor_pasarela, transaccion_externa, codigo_autorizacion,
    estado_pago, fecha_pago_utc, creado_por_usuario
) VALUES
('cccccccc-cccc-cccc-cccc-cccccccc2001',
 (SELECT id_factura FROM finanzas.factura WHERE factura_guid='bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002'),
 '99999999-9999-9999-9999-999999999002',                  -- reserva 002
 874.00, 'TARJETA_CREDITO', TRUE,
 'STRIPE_SANDBOX', 'pi_test_3OqzAna1234567890', 'AUTH-2026-05-05-A1B2',
 'APR', '2026-05-05 11:05:00+00', 'vendedor1');

-- Pago para factura 3 (Pedro RESERVA): efectivo walk-in
INSERT INTO finanzas.pago (
    pago_guid, id_factura, reserva_guid,
    monto, metodo_pago, es_pago_electronico,
    estado_pago, fecha_pago_utc, referencia, creado_por_usuario
) VALUES
('cccccccc-cccc-cccc-cccc-cccccccc2002',
 (SELECT id_factura FROM finanzas.factura WHERE factura_guid='bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0003'),
 '99999999-9999-9999-9999-999999999003',
 258.75, 'EFECTIVO', FALSE,
 'APR', '2026-04-20 14:40:00+00', 'Pago en caja al ingreso', 'vendedor1');

-- Pago para factura 4 (Pedro FINAL): efectivo al checkout
INSERT INTO finanzas.pago (
    pago_guid, id_factura, reserva_guid,
    monto, metodo_pago, es_pago_electronico,
    estado_pago, fecha_pago_utc, referencia, creado_por_usuario
) VALUES
('cccccccc-cccc-cccc-cccc-cccccccc2003',
 (SELECT id_factura FROM finanzas.factura WHERE factura_guid='bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0004'),
 '99999999-9999-9999-9999-999999999003',
 51.75, 'EFECTIVO', FALSE,
 'APR', '2026-04-23 11:35:00+00', 'Pago en caja al checkout (cargo de spa)', 'vendedor1');


-- ============================================================
-- VERIFICACION FINAL
-- ============================================================
SELECT 'Facturas:           ' || COUNT(*)::text FROM finanzas.factura;
SELECT 'Lineas de detalle:  ' || COUNT(*)::text FROM finanzas.factura_detalle;
SELECT 'Pagos:              ' || COUNT(*)::text FROM finanzas.pago;

-- Resumen facturas con sus pagos asociados
SELECT
    f.numero_factura,
    f.tipo_factura,
    f.cliente_guid,
    f.total,
    f.saldo_pendiente,
    f.estado,
    COUNT(p.id_pago)         AS pagos_registrados,
    COALESCE(SUM(p.monto),0) AS suma_pagos
FROM   finanzas.factura f
LEFT   JOIN finanzas.pago p ON p.id_factura = f.id_factura AND p.estado_pago = 'APR'
GROUP  BY f.id_factura
ORDER  BY f.fecha_emision;
