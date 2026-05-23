-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio FINANCE
-- Base de datos: HotelLux_Finance
-- Motor: PostgreSQL 18
-- Version: 2.0
--
-- ALCANCE: fusiona billing + payment. Garantiza transacciones
-- ACID locales al emitir facturas y registrar pagos.
--
-- DEPENDENCIAS LOGICAS (sin FK fisica):
--   - HotelLux_Reservation : reserva_guid, cliente_guid
--   - HotelLux_Accommodation: sucursal_guid
--   - HotelLux_Stay        : cargo_guid (referencia en detalle FINAL)
--
-- CONTENIDO:
--   Schema: finanzas
--   Tablas: factura, factura_detalle, pago
--
--   Datos semilla:
--     4 facturas:
--       FAC-RES-2026-000001: Juan Perez   RESERVA $690.00   EMI (sin pago)
--       FAC-RES-2026-000002: Ana Lopez    RESERVA $874.00   PAG
--       FAC-RES-2026-000003: Pedro Garcia RESERVA $258.75   PAG (walk-in)
--       FAC-FIN-2026-000001: Pedro Garcia FINAL   $51.75    PAG (cargo spa)
--     4 lineas de detalle (una por factura)
--     3 pagos aprobados (Juan no paga hasta check-in)
--
-- VALIDACION MATEMATICA (IVA 15%):
--   FAC-001: 5n x $120.00 = $600.00 + $90.00   = $690.00
--   FAC-002: 4n x $190.00 = $760.00 + $114.00  = $874.00
--   FAC-003: 3n x  $75.00 = $225.00 + $33.75   = $258.75
--   FAC-004: 1  x  $45.00 =  $45.00 + $6.75    =  $51.75
--
-- INSTRUCCIONES EN pgAdmin:
--   1. Create Database: HotelLux_Finance / Owner: postgres
--   2. Query Tool -> File -> Open este archivo -> F5
--   3. Verificar los SELECT de conteo al final.
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
    factura_guid             UUID          NOT NULL DEFAULT gen_random_uuid(),
    cliente_guid             UUID          NOT NULL,
    reserva_guid             UUID          NOT NULL,
    sucursal_guid            UUID          NOT NULL,
    numero_factura           VARCHAR(40)   NOT NULL,
    tipo_factura             VARCHAR(20)   NOT NULL DEFAULT 'RESERVA',
    fecha_emision            TIMESTAMPTZ   NOT NULL DEFAULT now(),
    subtotal                 NUMERIC(12,2) NOT NULL,
    valor_iva                NUMERIC(12,2) NOT NULL,
    descuento_total          NUMERIC(12,2) NOT NULL DEFAULT 0,
    total                    NUMERIC(12,2) NOT NULL,
    saldo_pendiente          NUMERIC(12,2) NOT NULL DEFAULT 0,
    moneda                   VARCHAR(10)   NOT NULL DEFAULT 'USD',
    observaciones_factura    VARCHAR(300)  NULL,
    origen_canal_factura     VARCHAR(50)   NULL,
    estado                   CHAR(3)       NOT NULL DEFAULT 'EMI',
    fecha_inhabilitacion_utc TIMESTAMPTZ   NULL,
    es_eliminado             BOOLEAN       NOT NULL DEFAULT FALSE,
    creado_por_usuario       VARCHAR(100)  NOT NULL,
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    modificado_por_usuario   VARCHAR(100)  NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          VARCHAR(45)   NULL,
    servicio_origen          VARCHAR(50)   NOT NULL DEFAULT 'finance-service',
    motivo_inhabilitacion    VARCHAR(250)  NULL,
    CONSTRAINT uq_factura_guid   UNIQUE (factura_guid),
    CONSTRAINT uq_factura_numero UNIQUE (numero_factura),
    -- RESERVA | FINAL | AJUSTE
    CONSTRAINT chk_factura_tipo     CHECK (tipo_factura IN ('RESERVA','FINAL','AJUSTE')),
    -- EMI=Emitida | PAG=Pagada | ANU=Anulada
    CONSTRAINT chk_factura_estado   CHECK (estado IN ('EMI','PAG','ANU')),
    CONSTRAINT chk_factura_subtotal CHECK (subtotal >= 0),
    CONSTRAINT chk_factura_iva      CHECK (valor_iva >= 0),
    CONSTRAINT chk_factura_descuento CHECK (descuento_total >= 0),
    CONSTRAINT chk_factura_total    CHECK (total >= 0),
    CONSTRAINT chk_factura_saldo    CHECK (saldo_pendiente >= 0),
    CONSTRAINT chk_factura_coherente CHECK (total >= subtotal - descuento_total)
);


-- ============================================================
-- TABLA: finanzas.factura_detalle
-- Snapshot inmutable. La logica de generacion lo puebla;
-- nunca se edita manualmente.
-- ============================================================
CREATE TABLE finanzas.factura_detalle (
    id_factura_detalle       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    factura_detalle_guid     UUID          NOT NULL DEFAULT gen_random_uuid(),
    id_factura               INT           NOT NULL,
    tipo_item                VARCHAR(30)   NOT NULL,
    referencia_tipo          VARCHAR(30)   NULL,
    referencia_guid          UUID          NULL,
    descripcion_item         VARCHAR(250)  NOT NULL,
    cantidad                 INT           NOT NULL DEFAULT 1,
    precio_unitario          NUMERIC(12,2) NOT NULL,
    subtotal_linea           NUMERIC(12,2) NOT NULL,
    valor_iva_linea          NUMERIC(12,2) NOT NULL DEFAULT 0,
    descuento_linea          NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_linea              NUMERIC(12,2) NOT NULL,
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100)  NOT NULL,
    CONSTRAINT uq_factura_detalle_guid UNIQUE (factura_detalle_guid),
    CONSTRAINT fk_factura_detalle_factura FOREIGN KEY (id_factura)
        REFERENCES finanzas.factura(id_factura) ON DELETE CASCADE,
    -- ALOJAMIENTO | SERVICIO | DESCUENTO | AJUSTE
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
-- Fusionada aqui desde payment-service para garantizar
-- transacciones ACID al marcar facturas como pagadas.
-- ============================================================
CREATE TABLE finanzas.pago (
    id_pago                  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    pago_guid                UUID          NOT NULL DEFAULT gen_random_uuid(),
    id_factura               INT           NOT NULL,
    reserva_guid             UUID          NOT NULL,
    monto                    NUMERIC(12,2) NOT NULL,
    metodo_pago              VARCHAR(40)   NOT NULL,
    es_pago_electronico      BOOLEAN       NOT NULL DEFAULT FALSE,
    proveedor_pasarela       VARCHAR(50)   NULL,
    transaccion_externa      VARCHAR(150)  NULL,
    codigo_autorizacion      VARCHAR(150)  NULL,
    referencia               VARCHAR(150)  NULL,
    estado_pago              CHAR(3)       NOT NULL DEFAULT 'PEN',
    fecha_pago_utc           TIMESTAMPTZ   NOT NULL DEFAULT now(),
    moneda                   VARCHAR(10)   NOT NULL DEFAULT 'USD',
    tipo_cambio              NUMERIC(10,4) NOT NULL DEFAULT 1.0000,
    respuesta_pasarela       TEXT          NULL,
    creado_por_usuario       VARCHAR(100)  NOT NULL,
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    modificado_por_usuario   VARCHAR(100)  NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          VARCHAR(45)   NULL,
    servicio_origen          VARCHAR(50)   NOT NULL DEFAULT 'finance-service',
    CONSTRAINT uq_pago_guid    UNIQUE (pago_guid),
    CONSTRAINT fk_pago_factura FOREIGN KEY (id_factura)
        REFERENCES finanzas.factura(id_factura),
    -- PEN | PRO | APR | REC | CAN
    CONSTRAINT chk_pago_estado      CHECK (estado_pago IN ('PEN','PRO','APR','REC','CAN')),
    CONSTRAINT chk_pago_monto       CHECK (monto > 0),
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

-- Idempotencia: dos pagos no pueden compartir transaccion externa
CREATE UNIQUE INDEX uq_pago_transaccion_externa
    ON finanzas.pago(transaccion_externa)
    WHERE transaccion_externa IS NOT NULL;


-- ============================================================
-- ============================================================
-- DATOS SEMILLA
-- ============================================================
-- ============================================================


-- ============================================================
-- FACTURAS (prefijo GUID: bbxxxxxx-...)
-- ============================================================

-- ----------------------------------------------------------
-- FAC-RES-2026-000001: Juan Perez — Quito — RESERVA — EMI
--
-- Generada al confirmar la reserva RES-001. Juan aun no ha
-- pagado ni hecho check-in. Saldo pendiente = $690.00.
--
-- subtotal $600.00 + IVA $90.00 = $690.00
-- ----------------------------------------------------------
INSERT INTO finanzas.factura (
    factura_guid,
    cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura,
    fecha_emision,
    subtotal, valor_iva, descuento_total, total, saldo_pendiente,
    moneda, estado,
    origen_canal_factura, observaciones_factura,
    creado_por_usuario
) VALUES
(
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0001',
    '33333333-3333-3333-3333-333333333001',  -- Juan Perez
    '99999999-9999-9999-9999-999999999001',  -- reserva 001
    '44444444-4444-4444-4444-444444444001',  -- sucursal Quito
    'FAC-RES-2026-000001', 'RESERVA',
    '2026-06-01 09:15:30+00',
    600.00, 90.00, 0.00, 690.00, 690.00,
    'USD', 'EMI',
    'PORTAL',
    'Factura de alojamiento emitida al confirmar reserva. Pendiente de pago previo al check-in del 2026-06-10.',
    'vendedor'
);

-- ----------------------------------------------------------
-- FAC-RES-2026-000002: Ana Lopez — Guayaquil — RESERVA — PAG
--
-- Generada al confirmar la reserva RES-002. Ana pago con
-- tarjeta de credito al confirmar. Saldo = $0.00.
--
-- subtotal $760.00 + IVA $114.00 = $874.00
-- ----------------------------------------------------------
INSERT INTO finanzas.factura (
    factura_guid,
    cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura,
    fecha_emision,
    subtotal, valor_iva, descuento_total, total, saldo_pendiente,
    moneda, estado,
    origen_canal_factura, observaciones_factura,
    creado_por_usuario
) VALUES
(
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002',
    '33333333-3333-3333-3333-333333333002',  -- Ana Lopez
    '99999999-9999-9999-9999-999999999002',  -- reserva 002
    '44444444-4444-4444-4444-444444444002',  -- sucursal Guayaquil
    'FAC-RES-2026-000002', 'RESERVA',
    '2026-05-02 11:30:45+00',
    760.00, 114.00, 0.00, 874.00, 0.00,
    'USD', 'PAG',
    'PORTAL',
    'Factura de alojamiento pagada con tarjeta de credito via Stripe al confirmar la reserva el 2026-05-02.',
    'vendedor'
);

-- ----------------------------------------------------------
-- FAC-RES-2026-000003: Pedro Garcia — Cuenca — RESERVA — PAG
--
-- Walk-in. Factura generada en el momento del check-in y
-- pagada de inmediato en efectivo en caja.
--
-- subtotal $225.00 + IVA $33.75 = $258.75
-- ----------------------------------------------------------
INSERT INTO finanzas.factura (
    factura_guid,
    cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura,
    fecha_emision,
    subtotal, valor_iva, descuento_total, total, saldo_pendiente,
    moneda, estado,
    origen_canal_factura, observaciones_factura,
    creado_por_usuario
) VALUES
(
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0003',
    '33333333-3333-3333-3333-333333333003',  -- Pedro Garcia
    '99999999-9999-9999-9999-999999999003',  -- reserva 003
    '44444444-4444-4444-4444-444444444003',  -- sucursal Cuenca
    'FAC-RES-2026-000003', 'RESERVA',
    '2026-04-20 14:05:00+00',
    225.00, 33.75, 0.00, 258.75, 0.00,
    'USD', 'PAG',
    'WALKIN',
    'Factura de alojamiento walk-in. Pago en efectivo recibido en caja al momento del check-in.',
    'vendedor'
);

-- ----------------------------------------------------------
-- FAC-FIN-2026-000001: Pedro Garcia — Cuenca — FINAL — PAG
--
-- Factura final generada al checkout por el cargo de spa
-- consumido durante la estadia. Pagada en efectivo en caja.
--
-- subtotal $45.00 + IVA $6.75 = $51.75
-- ----------------------------------------------------------
INSERT INTO finanzas.factura (
    factura_guid,
    cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura,
    fecha_emision,
    subtotal, valor_iva, descuento_total, total, saldo_pendiente,
    moneda, estado,
    origen_canal_factura, observaciones_factura,
    creado_por_usuario
) VALUES
(
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0004',
    '33333333-3333-3333-3333-333333333003',  -- Pedro Garcia
    '99999999-9999-9999-9999-999999999003',  -- reserva 003 (misma reserva)
    '44444444-4444-4444-4444-444444444003',  -- sucursal Cuenca
    'FAC-FIN-2026-000001', 'FINAL',
    '2026-04-23 11:30:00+00',
    45.00, 6.75, 0.00, 51.75, 0.00,
    'USD', 'PAG',
    'WALKIN',
    'Factura final de checkout por cargos de consumo durante estadia. Spa Andes Wellness incluido. Pago en efectivo.',
    'vendedor'
);


-- ============================================================
-- DETALLE DE FACTURAS
-- Snapshot de cada linea al momento de emision.
-- referencia_guid apunta logicamente a otras BDs.
-- ============================================================

-- ----------------------------------------------------------
-- Detalle FAC-001: alojamiento Juan Perez
-- 5 noches Suite Doble Quito 102
-- ref: reserva_habitacion a1a1...101 (HotelLux_Reservation)
-- ----------------------------------------------------------
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura,
    tipo_item, referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    creado_por_usuario
) VALUES
(
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd001',
    (SELECT id_factura FROM finanzas.factura WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0001'),
    'ALOJAMIENTO', 'RESERVA_HABITACION',
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a101',
    'Suite Doble Quito hab. 102 - 5 noche(s) del 10/06 al 15/06/2026',
    5,
    120.00, 600.00, 90.00, 0.00, 690.00,
    'vendedor'
);

-- ----------------------------------------------------------
-- Detalle FAC-002: alojamiento Ana Lopez
-- 4 noches Suite Familiar GYE 201
-- ref: reserva_habitacion a1a1...102 (HotelLux_Reservation)
-- ----------------------------------------------------------
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura,
    tipo_item, referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    creado_por_usuario
) VALUES
(
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd002',
    (SELECT id_factura FROM finanzas.factura WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002'),
    'ALOJAMIENTO', 'RESERVA_HABITACION',
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a102',
    'Suite Familiar Guayaquil hab. 201 - 4 noche(s) del 09/05 al 13/05/2026',
    4,
    190.00, 760.00, 114.00, 0.00, 874.00,
    'vendedor'
);

-- ----------------------------------------------------------
-- Detalle FAC-003: alojamiento Pedro Garcia (walk-in)
-- 3 noches Suite Single Cuenca 101
-- ref: reserva_habitacion a1a1...103 (HotelLux_Reservation)
-- ----------------------------------------------------------
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura,
    tipo_item, referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    creado_por_usuario
) VALUES
(
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd003',
    (SELECT id_factura FROM finanzas.factura WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0003'),
    'ALOJAMIENTO', 'RESERVA_HABITACION',
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a103',
    'Suite Single Cuenca hab. 101 - 3 noche(s) del 20/04 al 23/04/2026',
    3,
    75.00, 225.00, 33.75, 0.00, 258.75,
    'vendedor'
);

-- ----------------------------------------------------------
-- Detalle FAC-004: cargo de spa Pedro Garcia (factura FINAL)
-- ref: cargo_estadia cc1003 (HotelLux_Stay)
-- ----------------------------------------------------------
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura,
    tipo_item, referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    creado_por_usuario
) VALUES
(
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd004',
    (SELECT id_factura FROM finanzas.factura WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0004'),
    'SERVICIO', 'CARGO_ESTADIA',
    'cccccccc-cccc-cccc-cccc-cccccccc1003',
    'Spa Andes Wellness - masaje relajante 60 min con aceites esenciales',
    1,
    45.00, 45.00, 6.75, 0.00, 51.75,
    'vendedor'
);


-- ============================================================
-- PAGOS (prefijo GUID: cc...2xxx)
--
-- Solo facturas en estado PAG tienen pago registrado.
-- Juan Perez (FAC-001, EMI) aun NO ha pagado.
-- ============================================================

-- ----------------------------------------------------------
-- Pago cc2001: Ana Lopez — Tarjeta de credito via Stripe
-- Factura FAC-002 | $874.00 | APR
-- Pago electronico realizado al confirmar la reserva.
-- ----------------------------------------------------------
INSERT INTO finanzas.pago (
    pago_guid, id_factura, reserva_guid,
    monto, metodo_pago, es_pago_electronico,
    proveedor_pasarela, transaccion_externa, codigo_autorizacion,
    estado_pago, fecha_pago_utc, moneda,
    creado_por_usuario
) VALUES
(
    'cccccccc-cccc-cccc-cccc-cccccccc2001',
    (SELECT id_factura FROM finanzas.factura WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002'),
    '99999999-9999-9999-9999-999999999002',  -- reserva 002
    874.00, 'TARJETA_CREDITO', TRUE,
    'STRIPE_SANDBOX',
    'pi_3OqzAna2024050200001',
    'AUTH-2026-05-02-A7B3C1',
    'APR', '2026-05-02 11:31:10+00', 'USD',
    'vendedor'
);

-- ----------------------------------------------------------
-- Pago cc2002: Pedro Garcia — Efectivo walk-in al check-in
-- Factura FAC-003 | $258.75 | APR
-- ----------------------------------------------------------
INSERT INTO finanzas.pago (
    pago_guid, id_factura, reserva_guid,
    monto, metodo_pago, es_pago_electronico,
    referencia,
    estado_pago, fecha_pago_utc, moneda,
    creado_por_usuario
) VALUES
(
    'cccccccc-cccc-cccc-cccc-cccccccc2002',
    (SELECT id_factura FROM finanzas.factura WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0003'),
    '99999999-9999-9999-9999-999999999003',  -- reserva 003
    258.75, 'EFECTIVO', FALSE,
    'Pago en caja sucursal Cuenca - Recibo interno N° 2026-CUE-0042',
    'APR', '2026-04-20 14:08:00+00', 'USD',
    'vendedor'
);

-- ----------------------------------------------------------
-- Pago cc2003: Pedro Garcia — Efectivo al checkout
-- Factura FAC-004 (FINAL, cargo spa) | $51.75 | APR
-- ----------------------------------------------------------
INSERT INTO finanzas.pago (
    pago_guid, id_factura, reserva_guid,
    monto, metodo_pago, es_pago_electronico,
    referencia,
    estado_pago, fecha_pago_utc, moneda,
    creado_por_usuario
) VALUES
(
    'cccccccc-cccc-cccc-cccc-cccccccc2003',
    (SELECT id_factura FROM finanzas.factura WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0004'),
    '99999999-9999-9999-9999-999999999003',  -- reserva 003
    51.75, 'EFECTIVO', FALSE,
    'Pago en caja sucursal Cuenca al checkout - Recibo interno N° 2026-CUE-0043',
    'APR', '2026-04-23 11:32:00+00', 'USD',
    'vendedor'
);


-- ============================================================
-- VERIFICACION FINAL
-- Resultados esperados:
--   Facturas         : 4
--   Lineas de detalle: 4
--   Pagos            : 3
-- ============================================================
SELECT 'Facturas:           ' || COUNT(*)::text AS resultado FROM finanzas.factura;
SELECT 'Lineas de detalle:  ' || COUNT(*)::text AS resultado FROM finanzas.factura_detalle;
SELECT 'Pagos:              ' || COUNT(*)::text AS resultado FROM finanzas.pago;

-- Vista resumen: facturas con sus pagos
SELECT
    f.numero_factura,
    f.tipo_factura                              AS tipo,
    f.cliente_guid,
    f.subtotal                                  AS subtotal_usd,
    f.valor_iva                                 AS iva_usd,
    f.total                                     AS total_usd,
    f.saldo_pendiente                           AS saldo_usd,
    f.estado,
    COUNT(p.id_pago)                            AS pagos_registrados,
    COALESCE(SUM(p.monto), 0)                   AS suma_pagos_usd,
    f.total - COALESCE(SUM(p.monto), 0)         AS diferencia
FROM      finanzas.factura f
LEFT JOIN finanzas.pago p
       ON p.id_factura = f.id_factura
      AND p.estado_pago = 'APR'
GROUP BY  f.id_factura
ORDER BY  f.fecha_emision;