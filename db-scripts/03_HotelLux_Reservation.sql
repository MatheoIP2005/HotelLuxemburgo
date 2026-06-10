-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio RESERVATION
-- Base de datos: HotelLux_Reservation
-- Motor: PostgreSQL 18
-- Version: 3.0
--
-- ALCANCE: gestiona clientes, reservas y detalle de
-- habitaciones reservadas. Expone endpoints REST publicos
-- para crear/consultar/cancelar reservas y emite eventos
-- gRPC al microservicio AUDIT tras cada operacion CRUD.
--
-- DEPENDENCIAS LOGICAS (sin FK fisica entre BDs):
--   - HotelLux_Accommodation : sucursal_guid, habitacion_guid,
--                              tarifa_guid (verificados via gRPC)
--   - HotelLux_Auth          : cliente_guid propagado a Auth
--                              al registrar cuenta en portal
--
-- CONTENIDO:
--   Schema: reservas
--   Tablas: cliente, reserva, reserva_habitacion
-- ============================================================


-- ============================================================
-- SCHEMA
-- ============================================================
CREATE SCHEMA IF NOT EXISTS reservas;


-- ============================================================
-- TABLA: reservas.cliente
--
-- Almacena los datos del huesped o empresa que realiza la
-- reserva. El cliente_guid se propaga a HotelLux_Auth cuando
-- el cliente crea una cuenta en el portal web.
-- ============================================================
CREATE TABLE reservas.cliente (
    id_cliente               INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cliente_guid             UUID         NOT NULL DEFAULT gen_random_uuid(),
    tipo_identificacion      CHAR(3)      NOT NULL,  -- CED | RUC | PAS
    numero_identificacion    CHAR(30)     NOT NULL,
    nombres                  CHAR(50)     NOT NULL,
    apellidos                CHAR(50)     NULL,      -- NULL para razon_social = EMP
    razon_social             CHAR(3)      NOT NULL,  -- NAT=Persona natural | EMP=Empresa
    correo                   VARCHAR(100) NOT NULL,
    telefono                 CHAR(10)     NOT NULL,  -- 10 digitos, formato 09XXXXXXXX
    direccion                VARCHAR(200) NOT NULL,
    estado                   CHAR(3)      NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    creado_por_usuario       CHAR(30)     NOT NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    modificado_por_usuario   CHAR(30)     NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          CHAR(25)     NULL,
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    motivo_inhabilitacion    VARCHAR(150) NULL,
    servicio_origen          CHAR(50)     NOT NULL DEFAULT 'reservation-service',
    CONSTRAINT uq_cliente_guid          UNIQUE (cliente_guid),
    CONSTRAINT uq_cliente_identif       UNIQUE (numero_identificacion),
    CONSTRAINT uq_cliente_correo        UNIQUE (correo),
    CONSTRAINT chk_cliente_estado       CHECK (estado IN ('ACT','INA')),
    CONSTRAINT chk_cliente_tipo_identif CHECK (tipo_identificacion IN ('CED','RUC','PAS')),
    CONSTRAINT chk_cliente_razon_social CHECK (razon_social IN ('NAT','EMP'))
);


-- ============================================================
-- TABLA: reservas.reserva
--
-- Cabecera de la reserva. Referencia logica a sucursal_guid
-- de HotelLux_Accommodation; el microservicio valida via gRPC
-- que la sucursal exista y este activa antes de insertar.
-- ============================================================
CREATE TABLE reservas.reserva (
    id_reserva               INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    reserva_guid             UUID          NOT NULL DEFAULT gen_random_uuid(),
    codigo_reserva           CHAR(20)      NOT NULL, -- ej. RES-2026-000001 (16 chars)
    id_cliente               INT           NOT NULL,
    sucursal_guid            UUID          NOT NULL, -- ref logica a alojamiento.sucursal
    fecha_reserva_utc        TIMESTAMPTZ   NOT NULL DEFAULT now(),
    fecha_inicio             DATE          NOT NULL,
    fecha_fin                DATE          NOT NULL,
    subtotal_reserva         NUMERIC(12,2) NOT NULL,
    valor_iva                NUMERIC(12,2) NOT NULL, -- subtotal * porcentaje_iva / 100
    total_reserva            NUMERIC(12,2) NOT NULL, -- subtotal + valor_iva
    descuento_aplicado       NUMERIC(12,2) NOT NULL DEFAULT 0,
    saldo_pendiente          NUMERIC(12,2) NOT NULL DEFAULT 0, -- total - descuento
    origen_canal_reserva     CHAR(10)      NOT NULL, -- PORTAL | ADMIN | WALKIN
    estado_reserva           CHAR(3)       NOT NULL DEFAULT 'PEN',
    -- PEN=Pendiente | CON=Confirmada | CAN=Cancelada |
    -- EXP=Expirada | FIN=Finalizada | EMI=En estadia
    fecha_confirmacion_utc   TIMESTAMPTZ   NULL,
    fecha_cancelacion_utc    TIMESTAMPTZ   NULL,
    motivo_cancelacion       VARCHAR(150)  NULL,
    observaciones            TEXT          NULL,
    es_walkin                BOOLEAN       NOT NULL DEFAULT FALSE,
    es_eliminado             BOOLEAN       NOT NULL DEFAULT FALSE,
    creado_por_usuario       CHAR(30)      NOT NULL,
    creado_desde_ip          VARCHAR(45)   NULL,
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    modificado_por_usuario   CHAR(30)      NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          CHAR(25)      NULL,
    servicio_origen          CHAR(50)      NOT NULL DEFAULT 'reservation-service',
    fecha_inhabilitacion_utc TIMESTAMPTZ   NULL,
    motivo_inhabilitacion    VARCHAR(150)  NULL,
    CONSTRAINT uq_reserva_guid    UNIQUE (reserva_guid),
    CONSTRAINT uq_reserva_codigo  UNIQUE (codigo_reserva),
    CONSTRAINT fk_reserva_cliente FOREIGN KEY (id_cliente)
        REFERENCES reservas.cliente(id_cliente),
    CONSTRAINT chk_reserva_estado    CHECK (estado_reserva IN ('PEN','CON','CAN','EXP','FIN','EMI')),
    CONSTRAINT chk_reserva_origen    CHECK (origen_canal_reserva IN ('PORTAL','ADMIN','WALKIN')),
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
--
-- Detalle (lineas) de la reserva. Una reserva puede tener
-- N lineas (N habitaciones). habitacion_guid y tarifa_guid
-- son referencias logicas a HotelLux_Accommodation.
-- ============================================================
CREATE TABLE reservas.reserva_habitacion (
    id_reserva_habitacion    INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    reserva_habitacion_guid  UUID          NOT NULL DEFAULT gen_random_uuid(),
    id_reserva               INT           NOT NULL,
    habitacion_guid          UUID          NOT NULL, -- ref logica a alojamiento.habitacion
    tarifa_guid              UUID          NULL,     -- ref logica a alojamiento.tarifa
    fecha_inicio             DATE          NOT NULL,
    fecha_fin                DATE          NOT NULL,
    num_adultos              INT           NOT NULL DEFAULT 1,
    num_ninos                INT           NOT NULL DEFAULT 0,
    precio_noche_aplicado    NUMERIC(12,2) NOT NULL,
    subtotal_linea           NUMERIC(12,2) NOT NULL, -- precio_noche * noches
    valor_iva_linea          NUMERIC(12,2) NOT NULL, -- subtotal * 15 / 100
    descuento_linea          NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_linea              NUMERIC(12,2) NOT NULL, -- subtotal + iva - descuento
    estado_detalle           CHAR(3)       NOT NULL DEFAULT 'PEN',
    -- PEN | CON | CAN | FIN | EMI
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    creado_por_usuario       CHAR(30)      NOT NULL,
    modificado_por_usuario   CHAR(30)      NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          CHAR(25)      NULL,
    servicio_origen          CHAR(50)      NOT NULL DEFAULT 'reservation-service',
    CONSTRAINT uq_reserva_hab_guid  UNIQUE (reserva_habitacion_guid),
    CONSTRAINT uq_reserva_hab_linea UNIQUE (id_reserva, habitacion_guid, fecha_inicio),
    CONSTRAINT fk_reserva_hab_reserva FOREIGN KEY (id_reserva)
        REFERENCES reservas.reserva(id_reserva),
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

-- Critico para validar solapamiento de habitaciones en disponibilidad
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
-- Tipo / Identificacion / Sucursal de origen:
--   33...001  Juan Carlos Perez Mendoza    NAT  CED 1712345678    Quito
--   33...002  Ana Maria Lopez Vargas       NAT  CED 0923456789    Guayaquil
--   33...003  Pedro Antonio Garcia Reyes   NAT  CED 1098765432    Cuenca (walk-in)
--   33...004  Turismo Andes S.A.           EMP  RUC 1791845623001 Quito
--   33...005  Carlos Renato Mora Salinas   NAT  PAS PA12345678    Colombia (turista)
--
-- Cross-BD:
--   33...001  cliente_guid vinculado a usuario juan.perez en HotelLux_Auth
--   33...002  cliente_guid vinculado a usuario ana.lopez  en HotelLux_Auth
--   33...003  walk-in, sin cuenta en el portal (no existe en HotelLux_Auth)
--   33...004  empresa, cuenta administrada por admin
--   33...005  turista extranjero con pasaporte colombiano
-- ============================================================
INSERT INTO reservas.cliente (
    cliente_guid,
    tipo_identificacion, numero_identificacion,
    nombres, apellidos, razon_social,
    correo, telefono, direccion,
    creado_por_usuario, fecha_registro_utc
) VALUES

-- 1: Juan Carlos Perez Mendoza — Quito — portal
(
    '33333333-3333-3333-3333-333333333001',
    'CED', '1712345678',
    'Juan Carlos', 'Perez Mendoza', 'NAT',
    'juan.perez@gmail.com',
    '0991234567',
    'Av. Eloy Alfaro N32-110 y Av. 6 de Diciembre, La Mariscal, Quito, Pichincha',
    'vendedor', '2026-04-15 14:22:05+00'
),

-- 2: Ana Maria Lopez Vargas — Guayaquil — portal
(
    '33333333-3333-3333-3333-333333333002',
    'CED', '0923456789',
    'Ana Maria', 'Lopez Vargas', 'NAT',
    'ana.lopez@gmail.com',
    '0987654321',
    'Av. Francisco de Orellana, Cdla. Kennedy Norte, Guayaquil, Guayas',
    'vendedor', '2026-05-01 10:00:00+00'
),

-- 3: Pedro Antonio Garcia Reyes — Cuenca — walk-in (sin cuenta portal)
(
    '33333333-3333-3333-3333-333333333003',
    'CED', '1098765432',
    'Pedro Antonio', 'Garcia Reyes', 'NAT',
    'pedro.garcia@hotmail.com',
    '0975550011',
    'Calle Bolivar 10-25 y Padre Aguirre, Centro Historico, Cuenca, Azuay',
    'vendedor', '2026-04-20 14:00:00+00'
),

-- 4: Turismo Andes S.A. — empresa Quito — canal admin
-- RUC: 1791845623001 (Pichincha=17, tipo=9, secuencial, establecimiento 001)
(
    '33333333-3333-3333-3333-333333333004',
    'RUC', '1791845623001',
    'Turismo Andes S.A.', NULL, 'EMP',
    'reservas@turismoandes.com.ec',
    '0993456789',
    'Av. Republica del Salvador N36-140, Edif. Naciones Unidas piso 4, Quito, Pichincha',
    'admin', '2026-05-20 09:00:00+00'
),

-- 5: Carlos Renato Mora Salinas — turista colombiano — pasaporte
(
    '33333333-3333-3333-3333-333333333005',
    'PAS', 'PA12345678',
    'Carlos Renato', 'Mora Salinas', 'NAT',
    'cmora.viajes@gmail.com',
    '0939876543',
    'Cra. 7 No. 15-22, Bogota D.C., Colombia',
    'vendedor', '2026-03-10 16:00:00+00'
);


-- ============================================================
-- RESERVAS (prefijo GUID: 99xxxxxx-...)
-- ============================================================

-- ------------------------------------------------------------
-- RESERVA 001: Juan Perez — Quito — CONFIRMADA (futura junio)
--
-- Juan reservo 5 noches en Suite Doble Quito (hab 102) via
-- portal. Pago pendiente: abonara al momento del check-in.
-- Reserva confirmada automaticamente por el sistema.
--
-- Matematica (IVA 15%):
--   5 noches x $120.00  = $600.00  subtotal
--   IVA 15%             =  $90.00
--   Total               = $690.00
--   Saldo               = $690.00  (sin pagar)
-- ------------------------------------------------------------
INSERT INTO reservas.reserva (
    reserva_guid, codigo_reserva,
    id_cliente, sucursal_guid,
    fecha_reserva_utc, fecha_inicio, fecha_fin,
    subtotal_reserva, valor_iva, total_reserva,
    descuento_aplicado, saldo_pendiente,
    origen_canal_reserva, estado_reserva,
    fecha_confirmacion_utc,
    observaciones, es_walkin,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    '99999999-9999-9999-9999-999999999001',
    'RES-2026-000001',
    (SELECT id_cliente FROM reservas.cliente
     WHERE cliente_guid = '33333333-3333-3333-3333-333333333001'),
    '44444444-4444-4444-4444-444444444001',  -- sucursal Quito
    '2026-05-11 10:00:00+00',
    '2026-06-10 15:00:00+00',
    '2026-06-15 12:00:00+00',
    600.00, 90.00, 690.00,
    0.00, 690.00,
    'PORTAL', 'CON',
    '2026-05-11 10:00:05+00',
    'Reserva confirmada via portal web. Cliente solicita habitacion en piso alto de ser posible. Pago pendiente previo al check-in.',
    FALSE, 'vendedor', '2026-05-11 10:00:00+00'
);


-- ------------------------------------------------------------
-- RESERVA 002: Ana Lopez — Guayaquil — EN ESTADIA (EMI)
--
-- Ana pago en linea con tarjeta Visa al confirmar. Check-in
-- realizado el 9 de mayo. Actualmente hospedada en Suite
-- Familiar GYE (hab 201). Estado EMI = estadia activa.
--
-- Matematica (IVA 15%):
--   4 noches x $190.00  = $760.00  subtotal
--   IVA 15%             = $114.00
--   Total               = $874.00
--   Saldo               =   $0.00  (pagada)
-- ------------------------------------------------------------
INSERT INTO reservas.reserva (
    reserva_guid, codigo_reserva,
    id_cliente, sucursal_guid,
    fecha_reserva_utc, fecha_inicio, fecha_fin,
    subtotal_reserva, valor_iva, total_reserva,
    descuento_aplicado, saldo_pendiente,
    origen_canal_reserva, estado_reserva,
    fecha_confirmacion_utc,
    observaciones, es_walkin,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    '99999999-9999-9999-9999-999999999002',
    'RES-2026-000002',
    (SELECT id_cliente FROM reservas.cliente
     WHERE cliente_guid = '33333333-3333-3333-3333-333333333002'),
    '44444444-4444-4444-4444-444444444002',  -- sucursal Guayaquil
    '2026-05-02 11:30:00+00',
    '2026-05-09 15:00:00+00',
    '2026-05-13 12:00:00+00',
    760.00, 114.00, 874.00,
    0.00, 0.00,
    'PORTAL', 'EMI',
    '2026-05-02 11:30:05+00',
    'Reserva pagada en linea con tarjeta de credito Visa. Check-in realizado el 2026-05-09. Huesped solicito cuna adicional para infante menor de 2 anos.',
    FALSE, 'vendedor', '2026-05-02 11:30:00+00'
);


-- ------------------------------------------------------------
-- RESERVA 003: Pedro Garcia — Cuenca — FINALIZADA (historica)
--
-- Walk-in en recepcion el 20 de abril. Cedula verificada.
-- Pago en efectivo al ingreso. Estadia sin novedades.
--
-- Matematica (IVA 15%):
--   3 noches x $75.00   = $225.00  subtotal
--   IVA 15%             =  $33.75
--   Total               = $258.75
--   Saldo               =   $0.00  (pagado en caja)
-- ------------------------------------------------------------
INSERT INTO reservas.reserva (
    reserva_guid, codigo_reserva,
    id_cliente, sucursal_guid,
    fecha_reserva_utc, fecha_inicio, fecha_fin,
    subtotal_reserva, valor_iva, total_reserva,
    descuento_aplicado, saldo_pendiente,
    origen_canal_reserva, estado_reserva,
    fecha_confirmacion_utc,
    observaciones, es_walkin,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    '99999999-9999-9999-9999-999999999003',
    'RES-2026-000003',
    (SELECT id_cliente FROM reservas.cliente
     WHERE cliente_guid = '33333333-3333-3333-3333-333333333003'),
    '44444444-4444-4444-4444-444444444003',  -- sucursal Cuenca
    '2026-04-20 14:00:00+00',
    '2026-04-20 14:00:00+00',
    '2026-04-23 12:00:00+00',
    225.00, 33.75, 258.75,
    0.00, 0.00,
    'WALKIN', 'FIN',
    '2026-04-20 14:00:05+00',
    'Cliente walk-in. Cedula verificada en recepcion. Pago en efectivo realizado en caja al ingreso. Estadia sin novedades. Check-out puntual el 2026-04-23.',
    TRUE, 'vendedor', '2026-04-20 14:00:00+00'
);


-- ------------------------------------------------------------
-- RESERVA 004: Turismo Andes S.A. — Quito — CONFIRMADA (julio)
--
-- Empresa de turismo reservo Suite Premium Quito (hab 301)
-- para ejecutivo extranjero. Canal ADMIN. Factura a nombre
-- de empresa; pago por transferencia bancaria pendiente.
--
-- Matematica (IVA 15%):
--   2 noches x $250.00  = $500.00  subtotal
--   IVA 15%             =  $75.00
--   Total               = $575.00
--   Saldo               = $575.00  (factura pendiente)
-- ------------------------------------------------------------
INSERT INTO reservas.reserva (
    reserva_guid, codigo_reserva,
    id_cliente, sucursal_guid,
    fecha_reserva_utc, fecha_inicio, fecha_fin,
    subtotal_reserva, valor_iva, total_reserva,
    descuento_aplicado, saldo_pendiente,
    origen_canal_reserva, estado_reserva,
    fecha_confirmacion_utc,
    observaciones, es_walkin,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    '99999999-9999-9999-9999-999999999004',
    'RES-2026-000004',
    (SELECT id_cliente FROM reservas.cliente
     WHERE cliente_guid = '33333333-3333-3333-3333-333333333004'),
    '44444444-4444-4444-4444-444444444001',  -- sucursal Quito
    '2026-06-15 09:00:00+00',
    '2026-07-01 15:00:00+00',
    '2026-07-03 12:00:00+00',
    500.00, 75.00, 575.00,
    0.00, 575.00,
    'ADMIN', 'CON',
    '2026-06-15 09:05:00+00',
    'Reserva corporativa Turismo Andes S.A. RUC 1791845623001. Factura requerida a nombre de empresa con detalle de consumo. Pago por transferencia bancaria previo al check-in.',
    FALSE, 'admin', '2026-06-15 09:00:00+00'
);


-- ------------------------------------------------------------
-- RESERVA 005: Carlos Mora — Cuenca — FINALIZADA (historica)
--
-- Turista colombiano. Reservo Suite Doble Cuenca (hab 102)
-- via portal antes de su viaje. Pago con Mastercard
-- internacional al confirmar. Check-out sin novedades.
--
-- Matematica (IVA 15%):
--   4 noches x $115.00  = $460.00  subtotal
--   IVA 15%             =  $69.00
--   Total               = $529.00
--   Saldo               =   $0.00  (pagado)
-- ------------------------------------------------------------
INSERT INTO reservas.reserva (
    reserva_guid, codigo_reserva,
    id_cliente, sucursal_guid,
    fecha_reserva_utc, fecha_inicio, fecha_fin,
    subtotal_reserva, valor_iva, total_reserva,
    descuento_aplicado, saldo_pendiente,
    origen_canal_reserva, estado_reserva,
    fecha_confirmacion_utc,
    observaciones, es_walkin,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    '99999999-9999-9999-9999-999999999005',
    'RES-2026-000005',
    (SELECT id_cliente FROM reservas.cliente
     WHERE cliente_guid = '33333333-3333-3333-3333-333333333005'),
    '44444444-4444-4444-4444-444444444003',  -- sucursal Cuenca
    '2026-03-10 16:00:00+00',
    '2026-03-15 14:00:00+00',
    '2026-03-19 12:00:00+00',
    460.00, 69.00, 529.00,
    0.00, 0.00,
    'PORTAL', 'FIN',
    '2026-03-10 16:00:05+00',
    'Turista colombiano pasaporte PA12345678. Pago con tarjeta Mastercard internacional al confirmar reserva. Check-out realizado el 2026-03-19 sin novedades. Dejo valoracion positiva.',
    FALSE, 'vendedor', '2026-03-10 16:00:00+00'
);


-- ============================================================
-- LINEAS DE RESERVA (reserva_habitacion)
-- prefijo GUID: a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1XX
--
-- Referencias logicas a HotelLux_Accommodation (sin FK):
--   habitacion_guid : 66666666-6666-6666-6666-66666666600X
--   tarifa_guid     : 77777777-7777-7777-7777-77777777700X
-- ============================================================

-- ------------------------------------------------------------
-- Linea 101: RES-001 Juan Perez — Suite Doble Quito hab 102
--   habitacion  : 66...002  (102 Quito TH-DOBLE)
--   tarifa      : 77...002  (TAR-UIO-DOBLE-2026  $120/noche)
--   5 noches    : $120 x 5 = $600 | IVA $90 | Total $690
-- ------------------------------------------------------------
INSERT INTO reservas.reserva_habitacion (
    reserva_habitacion_guid,
    id_reserva, habitacion_guid, tarifa_guid,
    fecha_inicio, fecha_fin,
    num_adultos, num_ninos,
    precio_noche_aplicado,
    subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    estado_detalle, creado_por_usuario, fecha_registro_utc
) VALUES (
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a101',
    (SELECT id_reserva FROM reservas.reserva
     WHERE reserva_guid = '99999999-9999-9999-9999-999999999001'),
    '66666666-6666-6666-6666-666666666002',  -- hab 102 Quito Suite Doble
    '77777777-7777-7777-7777-777777777002',  -- TAR-UIO-DOBLE-2026
    '2026-06-10 15:00:00+00',
    '2026-06-15 12:00:00+00',
    2, 0,
    120.00,
    600.00, 90.00, 0.00, 690.00,
    'CON', 'vendedor', '2026-05-11 10:00:00+00'
);

-- ------------------------------------------------------------
-- Linea 102: RES-002 Ana Lopez — Suite Familiar Guayaquil hab 201
--   habitacion  : 66...007  (201 GYE TH-FAMILIAR)
--   tarifa      : 77...006  (TAR-GYE-FAMILIAR-2026  $190/noche)
--   4 noches    : $190 x 4 = $760 | IVA $114 | Total $874
-- ------------------------------------------------------------
INSERT INTO reservas.reserva_habitacion (
    reserva_habitacion_guid,
    id_reserva, habitacion_guid, tarifa_guid,
    fecha_inicio, fecha_fin,
    num_adultos, num_ninos,
    precio_noche_aplicado,
    subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    estado_detalle, creado_por_usuario, fecha_registro_utc
) VALUES (
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a102',
    (SELECT id_reserva FROM reservas.reserva
     WHERE reserva_guid = '99999999-9999-9999-9999-999999999002'),
    '66666666-6666-6666-6666-666666666007',  -- hab 201 GYE Suite Familiar
    '77777777-7777-7777-7777-777777777006',  -- TAR-GYE-FAMILIAR-2026
    '2026-05-09 15:00:00+00',
    '2026-05-13 12:00:00+00',
    2, 1,
    190.00,
    760.00, 114.00, 0.00, 874.00,
    'EMI', 'vendedor', '2026-05-02 11:30:00+00'
);

-- ------------------------------------------------------------
-- Linea 103: RES-003 Pedro Garcia — Suite Single Cuenca hab 101
--   habitacion  : 66...008  (101 Cuenca TH-SINGLE)
--   tarifa      : 77...007  (TAR-CUE-SINGLE-2026  $75/noche)
--   3 noches    : $75 x 3 = $225 | IVA $33.75 | Total $258.75
-- ------------------------------------------------------------
INSERT INTO reservas.reserva_habitacion (
    reserva_habitacion_guid,
    id_reserva, habitacion_guid, tarifa_guid,
    fecha_inicio, fecha_fin,
    num_adultos, num_ninos,
    precio_noche_aplicado,
    subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    estado_detalle, creado_por_usuario, fecha_registro_utc
) VALUES (
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a103',
    (SELECT id_reserva FROM reservas.reserva
     WHERE reserva_guid = '99999999-9999-9999-9999-999999999003'),
    '66666666-6666-6666-6666-666666666008',  -- hab 101 Cuenca Suite Single
    '77777777-7777-7777-7777-777777777007',  -- TAR-CUE-SINGLE-2026
    '2026-04-20 14:00:00+00',
    '2026-04-23 12:00:00+00',
    1, 0,
    75.00,
    225.00, 33.75, 0.00, 258.75,
    'FIN', 'vendedor', '2026-04-20 14:00:00+00'
);

-- ------------------------------------------------------------
-- Linea 104: RES-004 Turismo Andes — Suite Premium Quito hab 301
--   habitacion  : 66...004  (301 Quito TH-PREMIUM)
--   tarifa      : 77...004  (TAR-UIO-PREMIUM-2026  $250/noche)
--   2 noches    : $250 x 2 = $500 | IVA $75 | Total $575
-- ------------------------------------------------------------
INSERT INTO reservas.reserva_habitacion (
    reserva_habitacion_guid,
    id_reserva, habitacion_guid, tarifa_guid,
    fecha_inicio, fecha_fin,
    num_adultos, num_ninos,
    precio_noche_aplicado,
    subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    estado_detalle, creado_por_usuario, fecha_registro_utc
) VALUES (
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a104',
    (SELECT id_reserva FROM reservas.reserva
     WHERE reserva_guid = '99999999-9999-9999-9999-999999999004'),
    '66666666-6666-6666-6666-666666666004',  -- hab 301 Quito Suite Premium
    '77777777-7777-7777-7777-777777777004',  -- TAR-UIO-PREMIUM-2026
    '2026-07-01 15:00:00+00',
    '2026-07-03 12:00:00+00',
    2, 0,
    250.00,
    500.00, 75.00, 0.00, 575.00,
    'CON', 'admin', '2026-06-15 09:00:00+00'
);

-- ------------------------------------------------------------
-- Linea 105: RES-005 Carlos Mora — Suite Doble Cuenca hab 102
--   habitacion  : 66...009  (102 Cuenca TH-DOBLE)
--   tarifa      : 77...011  (TAR-CUE-DOBLE-2026  $115/noche)
--   4 noches    : $115 x 4 = $460 | IVA $69 | Total $529
-- ------------------------------------------------------------
INSERT INTO reservas.reserva_habitacion (
    reserva_habitacion_guid,
    id_reserva, habitacion_guid, tarifa_guid,
    fecha_inicio, fecha_fin,
    num_adultos, num_ninos,
    precio_noche_aplicado,
    subtotal_linea, valor_iva_linea, descuento_linea, total_linea,
    estado_detalle, creado_por_usuario, fecha_registro_utc
) VALUES (
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a105',
    (SELECT id_reserva FROM reservas.reserva
     WHERE reserva_guid = '99999999-9999-9999-9999-999999999005'),
    '66666666-6666-6666-6666-666666666009',  -- hab 102 Cuenca Suite Doble
    '77777777-7777-7777-7777-777777777011',  -- TAR-CUE-DOBLE-2026
    '2026-03-15 14:00:00+00',
    '2026-03-19 12:00:00+00',
    2, 0,
    115.00,
    460.00, 69.00, 0.00, 529.00,
    'FIN', 'vendedor', '2026-03-10 16:00:00+00'
);


-- ============================================================
-- VERIFICACION FINAL
-- Resultados esperados:
--   Clientes          : 5
--   Reservas          : 5
--   Lineas de reserva : 5
-- ============================================================
SELECT 'Clientes:           ' || COUNT(*)::text AS resultado FROM reservas.cliente;
SELECT 'Reservas:           ' || COUNT(*)::text AS resultado FROM reservas.reserva;
SELECT 'Lineas de reserva:  ' || COUNT(*)::text AS resultado FROM reservas.reserva_habitacion;

-- Vista resumen completa: reservas con totales y estado de pago
SELECT
    TRIM(r.codigo_reserva)                                         AS reserva,
    TRIM(c.nombres) || ' ' || COALESCE(TRIM(c.apellidos), '')      AS cliente,
    TRIM(c.tipo_identificacion) || ' ' || TRIM(c.numero_identificacion)
                                                                   AS identificacion,
    TRIM(c.razon_social)                                           AS tipo,
    r.fecha_inicio::date                                           AS desde,
    r.fecha_fin::date                                              AS hasta,
    (r.fecha_fin::date - r.fecha_inicio::date)                     AS noches,
    r.subtotal_reserva                                             AS subtotal_usd,
    r.valor_iva                                                    AS iva_usd,
    r.total_reserva                                                AS total_usd,
    r.saldo_pendiente                                              AS saldo_usd,
    TRIM(r.estado_reserva)                                         AS estado,
    TRIM(r.origen_canal_reserva)                                   AS canal,
    r.es_walkin
FROM   reservas.reserva r
JOIN   reservas.cliente c ON c.id_cliente = r.id_cliente
ORDER  BY r.fecha_inicio;


ALTER TABLE reservas.cliente 
ALTER COLUMN razon_social SET DEFAULT 'NAT';

-- 1. Eliminar el CHECK viejo
ALTER TABLE reservas.reserva 
    DROP CONSTRAINT chk_reserva_origen;

-- 2. Cambiar CHAR(10) → VARCHAR(50)
ALTER TABLE reservas.reserva 
    ALTER COLUMN origen_canal_reserva TYPE VARCHAR(50);

-- 3. Nuevo CHECK que acepta los valores reales del app
ALTER TABLE reservas.reserva 
    ADD CONSTRAINT chk_reserva_origen 
    CHECK (origen_canal_reserva IN (
        'PORTAL', 'ADMIN', 'WALKIN',
        'BOOKING_PUBLIC', 'WEB_BOOKING', 'PUBLIC', 
        'PORTAL_WEB', 'API', 'DIRECT'
    ));


-- creado_por_usuario / modificado_por_usuario (pueden recibir emails o GUIDs)
ALTER TABLE reservas.cliente 
    ALTER COLUMN creado_por_usuario TYPE VARCHAR(100);
ALTER TABLE reservas.cliente 
    ALTER COLUMN modificado_por_usuario TYPE VARCHAR(100);

ALTER TABLE reservas.reserva 
    ALTER COLUMN creado_por_usuario TYPE VARCHAR(100);
ALTER TABLE reservas.reserva 
    ALTER COLUMN modificado_por_usuario TYPE VARCHAR(100);

ALTER TABLE reservas.reserva_habitacion 
    ALTER COLUMN creado_por_usuario TYPE VARCHAR(100);
ALTER TABLE reservas.reserva_habitacion 
    ALTER COLUMN modificado_por_usuario TYPE VARCHAR(100);

-- codigo_reserva CHAR(20) — 'RES-2026-000001' = 16 chars, OK por ahora
-- telefono CHAR(10) — 10 dígitos exactos, bien
-- modificacion_ip CHAR(25) — IPv6 puede tener hasta 39 chars, mejor ampliar
ALTER TABLE reservas.cliente 
    ALTER COLUMN modificacion_ip TYPE VARCHAR(45);
ALTER TABLE reservas.reserva 
    ALTER COLUMN modificacion_ip TYPE VARCHAR(45);
ALTER TABLE reservas.reserva_habitacion 
    ALTER COLUMN modificacion_ip TYPE VARCHAR(45);

ALTER TABLE reservas.reserva DROP CONSTRAINT chk_reserva_origen;