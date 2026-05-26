-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio FINANCE
-- Base de datos: HotelLux_Finance
--
-- ALCANCE: fusiona billing + payment. Garantiza transacciones
-- ACID locales al emitir facturas y registrar pagos.
--
-- DEPENDENCIAS LOGICAS (sin FK fisica entre BDs):
--   - HotelLux_Reservation  : reserva_guid, cliente_guid
--   - HotelLux_Accommodation: sucursal_guid
--   - HotelLux_Stay         : cargo_guid (detalle tipo SERVICIO)
--
-- CAMBIOS v3.0 vs spec original:
--   finanzas.pago
--     - chk_pago_metodo    : nuevo CHECK TARJETA_CREDITO|DEBITO|
--                            EFECTIVO|TRANSFERENCIA|CHEQUE|OTRO
--     - chk_pago_pasarela  : nuevo CHECK IS NULL OR STRIPE_*|PAYPAL_*|OTRO
--   finanzas.factura
--     - chk_factura_canal  : nuevo CHECK PORTAL|ADMIN|WALKIN en
--                            origen_canal_factura
--     - fecha_emision FAC-001: corregida 2026-06-01 → 2026-05-11
--                             (alineada con audit event 8 y RES-001)
--
-- CONTENIDO:
--   Schema: finanzas
--   Tablas: factura, factura_detalle, pago
--   Datos : 7 facturas | 7 detalles | 5 pagos
-- ============================================================

-- ============================================================
-- SCHEMA
-- ============================================================
CREATE SCHEMA IF NOT EXISTS finanzas;

-- ============================================================
-- TABLA: finanzas.factura
--
-- Snapshot de cada reserva o estadia en el momento de emision.
-- cliente_guid, reserva_guid y sucursal_guid son referencias
-- logicas a otras BDs (sin FK fisica).
-- ============================================================
CREATE TABLE finanzas.factura (
    id_factura               INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    factura_guid             UUID          NOT NULL DEFAULT gen_random_uuid(),
    cliente_guid             UUID          NOT NULL,  -- ref logica a reservas.cliente
    reserva_guid             UUID          NOT NULL,  -- ref logica a reservas.reserva
    sucursal_guid            UUID          NOT NULL,  -- ref logica a alojamiento.sucursal
    numero_factura           CHAR(40)      NOT NULL,
    tipo_factura             CHAR(20)      NOT NULL DEFAULT 'RESERVA',
    -- RESERVA=Pre-stay alojamiento | FINAL=Post-stay consumos | AJUSTE
    fecha_emision            TIMESTAMPTZ   NOT NULL DEFAULT now(),
    subtotal                 NUMERIC(12,2) NOT NULL,  -- base sin IVA
    valor_iva                NUMERIC(12,2) NOT NULL,  -- subtotal * 15 / 100
    descuento_total          NUMERIC(12,2) NOT NULL DEFAULT 0,
    total                    NUMERIC(12,2) NOT NULL,  -- subtotal + valor_iva - descuento
    saldo_pendiente          NUMERIC(12,2) NOT NULL DEFAULT 0,
    moneda                   VARCHAR(10)   NOT NULL DEFAULT 'USD',
    observaciones_factura    VARCHAR(200)  NULL,
    origen_canal_factura     VARCHAR(50)   NULL,      -- PORTAL | ADMIN | WALKIN
    estado                   CHAR(3)       NOT NULL DEFAULT 'EMI',
    -- EMI=Emitida | PAG=Pagada | ANU=Anulada
    fecha_inhabilitacion_utc TIMESTAMPTZ   NULL,
    es_eliminado             BOOLEAN       NOT NULL DEFAULT FALSE,
    creado_por_usuario       VARCHAR(100)  NOT NULL,
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    modificado_por_usuario   VARCHAR(100)  NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          VARCHAR(45)   NULL,
    servicio_origen          VARCHAR(50)   NOT NULL DEFAULT 'finance-service',
    motivo_inhabilitacion    VARCHAR(150)  NULL,
    CONSTRAINT uq_factura_guid       UNIQUE (factura_guid),
    CONSTRAINT uq_factura_numero     UNIQUE (numero_factura),
    CONSTRAINT chk_factura_tipo      CHECK (tipo_factura IN ('RESERVA','FINAL','AJUSTE')),
    CONSTRAINT chk_factura_estado    CHECK (estado IN ('EMI','PAG','ANU')),
    CONSTRAINT chk_factura_canal     CHECK (origen_canal_factura IS NULL OR
        origen_canal_factura IN ('PORTAL','ADMIN','WALKIN')),
    CONSTRAINT chk_factura_subtotal  CHECK (subtotal >= 0),
    CONSTRAINT chk_factura_iva       CHECK (valor_iva >= 0),
    CONSTRAINT chk_factura_descuento CHECK (descuento_total >= 0),
    CONSTRAINT chk_factura_total     CHECK (total >= 0),
    CONSTRAINT chk_factura_saldo     CHECK (saldo_pendiente >= 0),
    CONSTRAINT chk_factura_coherente CHECK (total >= subtotal - descuento_total)
);


-- ============================================================
-- TABLA: finanzas.factura_detalle
--
-- Snapshot inmutable de cada linea al momento de emision.
-- referencia_guid apunta logicamente a otras BDs:
--   RESERVA_HABITACION → reservas.reserva_habitacion
--   CARGO_ESTADIA      → hospedaje.cargo_estadia
-- ============================================================
CREATE TABLE finanzas.factura_detalle (
    id_factura_detalle       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    factura_detalle_guid     UUID          NOT NULL DEFAULT gen_random_uuid(),
    id_factura               INT           NOT NULL,
    tipo_item                CHAR(30)      NOT NULL,
    -- ALOJAMIENTO | SERVICIO | DESCUENTO | AJUSTE
    referencia_tipo          CHAR(30)      NULL,
    -- RESERVA_HABITACION | CARGO_ESTADIA
    referencia_guid          UUID          NULL,      -- ref logica a la entidad origen
    descripcion_item         VARCHAR(250)  NOT NULL,
    cantidad                 INT           NOT NULL DEFAULT 1,
    precio_unitario          NUMERIC(12,2) NOT NULL,
    subtotal_linea           NUMERIC(12,2) NOT NULL,  -- precio_unitario * cantidad
    valor_iva_linea          NUMERIC(12,2) NOT NULL DEFAULT 0,  -- subtotal * 15 / 100
    descuento_linea          NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_linea              NUMERIC(12,2) NOT NULL,  -- subtotal + iva - descuento
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    creado_por_usuario       CHAR(30)      NOT NULL,
    CONSTRAINT uq_factura_detalle_guid    UNIQUE (factura_detalle_guid),
    CONSTRAINT fk_factura_detalle_factura FOREIGN KEY (id_factura)
        REFERENCES finanzas.factura(id_factura) ON DELETE CASCADE,
    CONSTRAINT chk_factura_detalle_tipo     CHECK (tipo_item IN ('ALOJAMIENTO','SERVICIO','DESCUENTO','AJUSTE')),
    CONSTRAINT chk_factura_detalle_ref_tipo CHECK (referencia_tipo IS NULL OR
        referencia_tipo IN ('RESERVA_HABITACION','CARGO_ESTADIA')),
    CONSTRAINT chk_factura_detalle_cantidad CHECK (cantidad > 0),
    CONSTRAINT chk_factura_detalle_precio   CHECK (precio_unitario >= 0),
    CONSTRAINT chk_factura_detalle_subtotal CHECK (subtotal_linea >= 0),
    CONSTRAINT chk_factura_detalle_iva      CHECK (valor_iva_linea >= 0),
    CONSTRAINT chk_factura_detalle_desc     CHECK (descuento_linea >= 0),
    CONSTRAINT chk_factura_detalle_total    CHECK (total_linea >= 0)
);


-- ============================================================
-- TABLA: finanzas.pago
--
-- Fusionada desde payment-service para garantizar ACID:
-- el UPDATE de factura.saldo_pendiente y el INSERT de pago
-- ocurren en la misma transaccion.
-- ============================================================
CREATE TABLE finanzas.pago (
    id_pago                  INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    pago_guid                UUID          NOT NULL DEFAULT gen_random_uuid(),
    id_factura               INT           NOT NULL,
    reserva_guid             UUID          NOT NULL,  -- ref logica a reservas.reserva (denorm)
    monto                    NUMERIC(12,2) NOT NULL,
    metodo_pago              CHAR(40)      NOT NULL,
    -- TARJETA_CREDITO | TARJETA_DEBITO | EFECTIVO |
    -- TRANSFERENCIA   | CHEQUE         | OTRO
    es_pago_electronico      BOOLEAN       NOT NULL DEFAULT FALSE,
    proveedor_pasarela       CHAR(50)      NULL,
    -- STRIPE_SANDBOX | STRIPE_PROD | PAYPAL_SANDBOX | PAYPAL_PROD | OTRO
    transaccion_externa      CHAR(150)     NULL,
    codigo_autorizacion      CHAR(150)     NULL,
    referencia               CHAR(150)     NULL,
    estado_pago              CHAR(3)       NOT NULL DEFAULT 'PEN',
    -- PEN | PRO | APR | REC | CAN
    fecha_pago_utc           TIMESTAMPTZ   NOT NULL DEFAULT now(),
    moneda                   CHAR(10)      NOT NULL DEFAULT 'USD',
    tipo_cambio              NUMERIC(10,4) NOT NULL DEFAULT 1.0000,
    respuesta_pasarela       TEXT          NULL,
    creado_por_usuario       CHAR(30)      NOT NULL,
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    modificado_por_usuario   CHAR(30)      NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          CHAR(25)      NULL,
    servicio_origen          CHAR(50)      NOT NULL DEFAULT 'finance-service',
    CONSTRAINT uq_pago_guid    UNIQUE (pago_guid),
    CONSTRAINT fk_pago_factura FOREIGN KEY (id_factura)
        REFERENCES finanzas.factura(id_factura),
    CONSTRAINT chk_pago_estado      CHECK (estado_pago IN ('PEN','PRO','APR','REC','CAN')),
    CONSTRAINT chk_pago_metodo      CHECK (metodo_pago IN (
        'TARJETA_CREDITO','TARJETA_DEBITO','EFECTIVO',
        'TRANSFERENCIA','CHEQUE','OTRO')),
    CONSTRAINT chk_pago_pasarela    CHECK (proveedor_pasarela IS NULL OR proveedor_pasarela IN (
        'STRIPE_SANDBOX','STRIPE_PROD',
        'PAYPAL_SANDBOX','PAYPAL_PROD','OTRO')),
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
--
-- Mapa reserva → factura(s):
--   RES-001 Juan Perez       → bb...0001 RESERVA   EMI  $690.00
--   RES-002 Ana Lopez        → bb...0002 RESERVA   PAG  $874.00
--   RES-003 Pedro Garcia     → bb...0003 RESERVA   PAG  $258.75
--                            → bb...0004 FINAL     PAG   $51.75  (spa cc1003)
--   RES-004 Turismo Andes    → bb...0005 RESERVA   EMI  $575.00
--   RES-005 Carlos Mora      → bb...0006 RESERVA   PAG  $529.00
--                            → bb...0007 FINAL     PAG   $51.75  (spa cc1004)
--
-- Cross-BD preservados:
--   bb...0001 → referenciado en audit event 8 (FAC-RES-2026-000001)
--   bb...0002 → referenciado en audit event 12 (pago cc2001 Ana Lopez)
-- ============================================================

-- ------------------------------------------------------------
-- bb...0001: FAC-RES-2026-000001 — Juan Perez — Quito — RESERVA — EMI
-- 5 noches Suite Doble Quito x $120 | sub $600 + IVA $90 = $690
-- Saldo $690 pendiente: Juan pagara al check-in (junio 2026).
-- CORRECCION: fecha_emision era 2026-06-01, corregida a 2026-05-11
--             (alineada con audit event 8: 2026-05-11 10:00:08 UTC)
-- ------------------------------------------------------------
INSERT INTO finanzas.factura (
    factura_guid,
    cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura, fecha_emision,
    subtotal, valor_iva, descuento_total, total, saldo_pendiente,
    moneda, estado, origen_canal_factura,
    observaciones_factura, creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0001',
    '33333333-3333-3333-3333-333333333001',  -- Juan Perez
    '99999999-9999-9999-9999-999999999001',  -- RES-001
    '44444444-4444-4444-4444-444444444001',  -- Quito
    'FAC-RES-2026-000001', 'RESERVA',
    '2026-05-11 10:00:08+00',
    600.00, 90.00, 0.00, 690.00, 690.00,
    'USD', 'EMI', 'PORTAL',
    'Factura de alojamiento emitida al confirmar reserva RES-2026-000001. Pendiente de pago previo al check-in del 2026-06-10.',
    'vendedor', '2026-05-11 10:00:08+00'
);

-- ------------------------------------------------------------
-- bb...0002: FAC-RES-2026-000002 — Ana Lopez — Guayaquil — RESERVA — PAG
-- 4 noches Suite Familiar GYE x $190 | sub $760 + IVA $114 = $874
-- Saldo $0: pagada con tarjeta Visa via Stripe al confirmar.
-- ------------------------------------------------------------
INSERT INTO finanzas.factura (
    factura_guid,
    cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura, fecha_emision,
    subtotal, valor_iva, descuento_total, total, saldo_pendiente,
    moneda, estado, origen_canal_factura,
    observaciones_factura, creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002',
    '33333333-3333-3333-3333-333333333002',  -- Ana Lopez
    '99999999-9999-9999-9999-999999999002',  -- RES-002
    '44444444-4444-4444-4444-444444444002',  -- Guayaquil
    'FAC-RES-2026-000002', 'RESERVA',
    '2026-05-02 11:30:45+00',
    760.00, 114.00, 0.00, 874.00, 0.00,
    'USD', 'PAG', 'PORTAL',
    'Factura de alojamiento pagada con tarjeta Visa via Stripe al confirmar reserva RES-2026-000002 el 2026-05-02.',
    'vendedor', '2026-05-02 11:30:45+00'
);

-- ------------------------------------------------------------
-- bb...0003: FAC-RES-2026-000003 — Pedro Garcia — Cuenca — RESERVA — PAG
-- Walk-in. 3 noches Suite Single x $75 | sub $225 + IVA $33.75 = $258.75
-- Saldo $0: pagada en efectivo en caja al momento del check-in.
-- ------------------------------------------------------------
INSERT INTO finanzas.factura (
    factura_guid,
    cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura, fecha_emision,
    subtotal, valor_iva, descuento_total, total, saldo_pendiente,
    moneda, estado, origen_canal_factura,
    observaciones_factura, creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0003',
    '33333333-3333-3333-3333-333333333003',  -- Pedro Garcia
    '99999999-9999-9999-9999-999999999003',  -- RES-003
    '44444444-4444-4444-4444-444444444003',  -- Cuenca
    'FAC-RES-2026-000003', 'RESERVA',
    '2026-04-20 14:05:00+00',
    225.00, 33.75, 0.00, 258.75, 0.00,
    'USD', 'PAG', 'WALKIN',
    'Factura walk-in. Pago en efectivo recibido en caja al check-in. CED 1098765432 verificada.',
    'vendedor', '2026-04-20 14:05:00+00'
);

-- ------------------------------------------------------------
-- bb...0004: FAC-FIN-2026-000001 — Pedro Garcia — Cuenca — FINAL — PAG
-- Cargo de spa cc1003 generado durante estadia.
-- sub $45 + IVA $6.75 = $51.75 | Pagada en efectivo al checkout.
-- ------------------------------------------------------------
INSERT INTO finanzas.factura (
    factura_guid,
    cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura, fecha_emision,
    subtotal, valor_iva, descuento_total, total, saldo_pendiente,
    moneda, estado, origen_canal_factura,
    observaciones_factura, creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0004',
    '33333333-3333-3333-3333-333333333003',  -- Pedro Garcia
    '99999999-9999-9999-9999-999999999003',  -- RES-003 (misma reserva)
    '44444444-4444-4444-4444-444444444003',  -- Cuenca
    'FAC-FIN-2026-000001', 'FINAL',
    '2026-04-23 11:30:00+00',
    45.00, 6.75, 0.00, 51.75, 0.00,
    'USD', 'PAG', 'WALKIN',
    'Factura final de checkout. Cargo: Spa Andes Wellness masaje relajante 60 min. Pago en efectivo.',
    'vendedor', '2026-04-23 11:30:00+00'
);

-- ------------------------------------------------------------
-- bb...0005: FAC-RES-2026-000004 — Turismo Andes S.A. — Quito — RESERVA — EMI
-- 2 noches Suite Premium Quito x $250 | sub $500 + IVA $75 = $575
-- Saldo $575 pendiente: empresa paga por transferencia bancaria.
-- ------------------------------------------------------------
INSERT INTO finanzas.factura (
    factura_guid,
    cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura, fecha_emision,
    subtotal, valor_iva, descuento_total, total, saldo_pendiente,
    moneda, estado, origen_canal_factura,
    observaciones_factura, creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0005',
    '33333333-3333-3333-3333-333333333004',  -- Turismo Andes S.A. (EMP)
    '99999999-9999-9999-9999-999999999004',  -- RES-004
    '44444444-4444-4444-4444-444444444001',  -- Quito
    'FAC-RES-2026-000004', 'RESERVA',
    '2026-06-15 09:05:00+00',
    500.00, 75.00, 0.00, 575.00, 575.00,
    'USD', 'EMI', 'ADMIN',
    'Factura corporativa a nombre de Turismo Andes S.A. RUC 1791845623001. Pago pendiente por transferencia bancaria. Check-in previsto 2026-07-01.',
    'admin', '2026-06-15 09:05:00+00'
);

-- ------------------------------------------------------------
-- bb...0006: FAC-RES-2026-000005 — Carlos Mora — Cuenca — RESERVA — PAG
-- Turista colombiano. 4 noches Suite Doble x $115
-- sub $460 + IVA $69 = $529 | Pagada con Mastercard via Stripe.
-- ------------------------------------------------------------
INSERT INTO finanzas.factura (
    factura_guid,
    cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura, fecha_emision,
    subtotal, valor_iva, descuento_total, total, saldo_pendiente,
    moneda, estado, origen_canal_factura,
    observaciones_factura, creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0006',
    '33333333-3333-3333-3333-333333333005',  -- Carlos Mora (PAS)
    '99999999-9999-9999-9999-999999999005',  -- RES-005
    '44444444-4444-4444-4444-444444444003',  -- Cuenca
    'FAC-RES-2026-000005', 'RESERVA',
    '2026-03-10 16:00:08+00',
    460.00, 69.00, 0.00, 529.00, 0.00,
    'USD', 'PAG', 'PORTAL',
    'Factura de alojamiento. PAS PA12345678. Pagada con tarjeta Mastercard internacional via Stripe al confirmar reserva el 2026-03-10.',
    'vendedor', '2026-03-10 16:00:08+00'
);

-- ------------------------------------------------------------
-- bb...0007: FAC-FIN-2026-000002 — Carlos Mora — Cuenca — FINAL — PAG
-- Cargo de spa cc1004 consumido el 3er dia de estadia.
-- sub $45 + IVA $6.75 = $51.75 | Pagada con tarjeta al checkout.
-- ------------------------------------------------------------
INSERT INTO finanzas.factura (
    factura_guid,
    cliente_guid, reserva_guid, sucursal_guid,
    numero_factura, tipo_factura, fecha_emision,
    subtotal, valor_iva, descuento_total, total, saldo_pendiente,
    moneda, estado, origen_canal_factura,
    observaciones_factura, creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0007',
    '33333333-3333-3333-3333-333333333005',  -- Carlos Mora (PAS)
    '99999999-9999-9999-9999-999999999005',  -- RES-005
    '44444444-4444-4444-4444-444444444003',  -- Cuenca
    'FAC-FIN-2026-000002', 'FINAL',
    '2026-03-19 12:05:00+00',
    45.00, 6.75, 0.00, 51.75, 0.00,
    'USD', 'PAG', 'PORTAL',
    'Factura final checkout. Cargo: Spa Andes Wellness masaje tejido profundo 60 min. Pagada con tarjeta Mastercard al checkout.',
    'vendedor', '2026-03-19 12:05:00+00'
);


-- ============================================================
-- DETALLE DE FACTURAS
-- Snapshot inmutable de cada linea al momento de emision.
-- referencia_guid apunta logicamente a otras BDs.
-- ============================================================

-- d001: FAC-001 Juan Perez — Suite Doble Quito 5 noches
-- referencia: reserva_habitacion a1...101 (HotelLux_Reservation)
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura,
    tipo_item, referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd001',
    (SELECT id_factura FROM finanzas.factura
     WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0001'),
    'ALOJAMIENTO', 'RESERVA_HABITACION',
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a101',
    'Suite Doble Quito hab. 102 - 5 noches del 10/06 al 15/06/2026',
    5, 120.00, 600.00, 90.00, 0.00, 690.00,
    'vendedor', '2026-05-11 10:00:08+00'
);

-- d002: FAC-002 Ana Lopez — Suite Familiar GYE 4 noches
-- referencia: reserva_habitacion a1...102 (HotelLux_Reservation)
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura,
    tipo_item, referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd002',
    (SELECT id_factura FROM finanzas.factura
     WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002'),
    'ALOJAMIENTO', 'RESERVA_HABITACION',
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a102',
    'Suite Familiar Guayaquil hab. 201 - 4 noches del 09/05 al 13/05/2026',
    4, 190.00, 760.00, 114.00, 0.00, 874.00,
    'vendedor', '2026-05-02 11:30:45+00'
);

-- d003: FAC-003 Pedro Garcia — Suite Single Cuenca 3 noches
-- referencia: reserva_habitacion a1...103 (HotelLux_Reservation)
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura,
    tipo_item, referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd003',
    (SELECT id_factura FROM finanzas.factura
     WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0003'),
    'ALOJAMIENTO', 'RESERVA_HABITACION',
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a103',
    'Suite Single Cuenca hab. 101 - 3 noches del 20/04 al 23/04/2026',
    3, 75.00, 225.00, 33.75, 0.00, 258.75,
    'vendedor', '2026-04-20 14:05:00+00'
);

-- d004: FAC-004 Pedro Garcia — Spa Cuenca (factura FINAL)
-- referencia: cargo_estadia cc1003 (HotelLux_Stay)
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura,
    tipo_item, referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd004',
    (SELECT id_factura FROM finanzas.factura
     WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0004'),
    'SERVICIO', 'CARGO_ESTADIA',
    'cccccccc-cccc-cccc-cccc-cccccccc1003',
    'Spa Andes Wellness - masaje relajante 60 min con aceites esenciales de rosas de Quito',
    1, 45.00, 45.00, 6.75, 0.00, 51.75,
    'vendedor', '2026-04-23 11:30:00+00'
);

-- d005: FAC-005 Turismo Andes — Suite Premium Quito 2 noches
-- referencia: reserva_habitacion a1...104 (HotelLux_Reservation)
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura,
    tipo_item, referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd005',
    (SELECT id_factura FROM finanzas.factura
     WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0005'),
    'ALOJAMIENTO', 'RESERVA_HABITACION',
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a104',
    'Suite Premium Quito hab. 301 - 2 noches del 01/07 al 03/07/2026 (Turismo Andes S.A.)',
    2, 250.00, 500.00, 75.00, 0.00, 575.00,
    'admin', '2026-06-15 09:05:00+00'
);

-- d006: FAC-006 Carlos Mora — Suite Doble Cuenca 4 noches
-- referencia: reserva_habitacion a1...105 (HotelLux_Reservation)
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura,
    tipo_item, referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd006',
    (SELECT id_factura FROM finanzas.factura
     WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0006'),
    'ALOJAMIENTO', 'RESERVA_HABITACION',
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a105',
    'Suite Doble Cuenca hab. 102 - 4 noches del 15/03 al 19/03/2026 (PAS PA12345678)',
    4, 115.00, 460.00, 69.00, 0.00, 529.00,
    'vendedor', '2026-03-10 16:00:08+00'
);

-- d007: FAC-007 Carlos Mora — Spa Cuenca (factura FINAL)
-- referencia: cargo_estadia cc1004 (HotelLux_Stay)
INSERT INTO finanzas.factura_detalle (
    factura_detalle_guid, id_factura,
    tipo_item, referencia_tipo, referencia_guid,
    descripcion_item, cantidad,
    precio_unitario, subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbd007',
    (SELECT id_factura FROM finanzas.factura
     WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0007'),
    'SERVICIO', 'CARGO_ESTADIA',
    'cccccccc-cccc-cccc-cccc-cccccccc1004',
    'Spa Andes Wellness - masaje tejido profundo 60 min con barro andino',
    1, 45.00, 45.00, 6.75, 0.00, 51.75,
    'vendedor', '2026-03-19 12:05:00+00'
);


-- ============================================================
-- PAGOS (prefijo GUID: cccccccc-cccc-cccc-cccc-cccccccc2XXX)
--
-- Solo facturas PAG tienen pago registrado:
--   cc...2001  Ana Lopez     FAC-002  Visa/Stripe    $874.00  APR
--   cc...2002  Pedro Garcia  FAC-003  Efectivo       $258.75  APR
--   cc...2003  Pedro Garcia  FAC-004  Efectivo        $51.75  APR
--   cc...2004  Carlos Mora   FAC-006  Mastercard/Str $529.00  APR
--   cc...2005  Carlos Mora   FAC-007  Mastercard/Str  $51.75  APR
--
-- Sin pago aun:
--   FAC-001 Juan Perez    EMI  pagara al check-in (junio)
--   FAC-005 Turismo Andes EMI  pendiente transferencia bancaria
--
-- Cross-BD preservados:
--   cc...2001 → referenciado en audit event 12 (finanzas.pago INSERT)
-- ============================================================

-- cc2001: Ana Lopez — Visa via Stripe Sandbox — $874.00 — APR
INSERT INTO finanzas.pago (
    pago_guid, id_factura, reserva_guid,
    monto, metodo_pago, es_pago_electronico,
    proveedor_pasarela, transaccion_externa, codigo_autorizacion,
    estado_pago, fecha_pago_utc, moneda,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'cccccccc-cccc-cccc-cccc-cccccccc2001',
    (SELECT id_factura FROM finanzas.factura
     WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0002'),
    '99999999-9999-9999-9999-999999999002',
    874.00, 'TARJETA_CREDITO', TRUE,
    'STRIPE_SANDBOX',
    'pi_3OqzAna2026050200001',
    'AUTH-2026-05-02-A7B3C1',
    'APR', '2026-05-02 11:31:10+00', 'USD',
    'vendedor', '2026-05-02 11:31:10+00'
);

-- cc2002: Pedro Garcia — Efectivo en caja al check-in — $258.75 — APR
INSERT INTO finanzas.pago (
    pago_guid, id_factura, reserva_guid,
    monto, metodo_pago, es_pago_electronico,
    referencia,
    estado_pago, fecha_pago_utc, moneda,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'cccccccc-cccc-cccc-cccc-cccccccc2002',
    (SELECT id_factura FROM finanzas.factura
     WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0003'),
    '99999999-9999-9999-9999-999999999003',
    258.75, 'EFECTIVO', FALSE,
    'Pago en caja sucursal Cuenca - Recibo interno N° 2026-CUE-0042',
    'APR', '2026-04-20 14:08:00+00', 'USD',
    'vendedor', '2026-04-20 14:08:00+00'
);

-- cc2003: Pedro Garcia — Efectivo en caja al checkout — $51.75 — APR
INSERT INTO finanzas.pago (
    pago_guid, id_factura, reserva_guid,
    monto, metodo_pago, es_pago_electronico,
    referencia,
    estado_pago, fecha_pago_utc, moneda,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'cccccccc-cccc-cccc-cccc-cccccccc2003',
    (SELECT id_factura FROM finanzas.factura
     WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0004'),
    '99999999-9999-9999-9999-999999999003',
    51.75, 'EFECTIVO', FALSE,
    'Pago en caja sucursal Cuenca al checkout - Recibo interno N° 2026-CUE-0043',
    'APR', '2026-04-23 11:32:00+00', 'USD',
    'vendedor', '2026-04-23 11:32:00+00'
);

-- cc2004: Carlos Mora — Mastercard via Stripe Sandbox — $529.00 — APR
-- Pago de FAC-006 (RESERVA) realizado al confirmar la reserva.
INSERT INTO finanzas.pago (
    pago_guid, id_factura, reserva_guid,
    monto, metodo_pago, es_pago_electronico,
    proveedor_pasarela, transaccion_externa, codigo_autorizacion,
    estado_pago, fecha_pago_utc, moneda,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'cccccccc-cccc-cccc-cccc-cccccccc2004',
    (SELECT id_factura FROM finanzas.factura
     WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0006'),
    '99999999-9999-9999-9999-999999999005',
    529.00, 'TARJETA_CREDITO', TRUE,
    'STRIPE_SANDBOX',
    'pi_3MrkCar2026031000001',
    'AUTH-2026-03-10-K9L2M4',
    'APR', '2026-03-10 16:01:20+00', 'USD',
    'vendedor', '2026-03-10 16:01:20+00'
);

-- cc2005: Carlos Mora — Mastercard via Stripe Sandbox — $51.75 — APR
-- Pago de FAC-007 (FINAL spa) realizado al checkout.
INSERT INTO finanzas.pago (
    pago_guid, id_factura, reserva_guid,
    monto, metodo_pago, es_pago_electronico,
    proveedor_pasarela, transaccion_externa, codigo_autorizacion,
    estado_pago, fecha_pago_utc, moneda,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'cccccccc-cccc-cccc-cccc-cccccccc2005',
    (SELECT id_factura FROM finanzas.factura
     WHERE factura_guid = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbb0007'),
    '99999999-9999-9999-9999-999999999005',
    51.75, 'TARJETA_CREDITO', TRUE,
    'STRIPE_SANDBOX',
    'pi_3MrkCar2026031900001',
    'AUTH-2026-03-19-P5Q7R8',
    'APR', '2026-03-19 12:10:00+00', 'USD',
    'vendedor', '2026-03-19 12:10:00+00'
);


-- ============================================================
-- VERIFICACION FINAL
-- Resultados esperados:
--   Facturas         : 7
--   Lineas de detalle: 7
--   Pagos            : 5
-- ============================================================
SELECT 'Facturas:           ' || COUNT(*)::text AS resultado FROM finanzas.factura;
SELECT 'Lineas de detalle:  ' || COUNT(*)::text AS resultado FROM finanzas.factura_detalle;
SELECT 'Pagos:              ' || COUNT(*)::text AS resultado FROM finanzas.pago;

-- Vista resumen: facturas con pagos y diferencia
SELECT
    TRIM(f.numero_factura)                        AS factura,
    TRIM(f.tipo_factura)                          AS tipo,
    f.subtotal                                    AS sub_usd,
    f.valor_iva                                   AS iva_usd,
    f.total                                       AS total_usd,
    f.saldo_pendiente                             AS saldo_usd,
    TRIM(f.estado)                                AS estado,
    COUNT(p.id_pago)                              AS pagos,
    COALESCE(SUM(p.monto), 0)                     AS cobrado_usd,
    f.total - COALESCE(SUM(p.monto), 0)           AS diferencia
FROM      finanzas.factura f
LEFT JOIN finanzas.pago p
       ON p.id_factura   = f.id_factura
      AND p.estado_pago  = 'APR'
GROUP BY  f.id_factura
ORDER BY  f.fecha_emision;
