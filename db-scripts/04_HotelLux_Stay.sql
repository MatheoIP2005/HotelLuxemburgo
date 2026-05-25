-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio STAY
-- Base de datos: HotelLux_Stay
-- Motor: PostgreSQL 18
-- Version: 3.0
--
-- ALCANCE: fusiona stay + rating. Maneja todo lo que ocurre
-- DURANTE y DESPUES de la estadia: check-in, cargos de
-- consumo, check-out y valoraciones del huesped.
--
-- DEPENDENCIAS LOGICAS (sin FK fisica entre BDs):
--   - HotelLux_Reservation  : reserva_guid,
--                             reserva_habitacion_guid, cliente_guid
--   - HotelLux_Accommodation: sucursal_guid, habitacion_guid,
--                             catalogo_guid (→ catalogo_servicios)
--
-- CAMBIOS v3.0 vs spec original:
--   hospedaje.valoracion
--     - creado_por_usuario  : 'pedro.garcia' → 'vendedor'
--                             (pedro.garcia no tiene cuenta Auth;
--                              el personal registra la valoracion)
--   Datos semilla
--     - estadia aa...005    : nueva, Carlos Mora (FIN, Cuenca)
--     - cargo  cc1004       : SPA Cuenca FAC para aa...005
--     - valoracion dd...1005: Carlos Mora (PUB) sobre aa...005
--
-- CONTENIDO:
--   Schema: hospedaje
--   Tablas: estadia, cargo_estadia, valoracion
--   Datos : 3 estadias | 4 cargos | 2 valoraciones
-- ============================================================


-- ============================================================
-- SCHEMA
-- ============================================================
CREATE SCHEMA IF NOT EXISTS hospedaje;


-- ============================================================
-- TABLA: hospedaje.estadia
--
-- Denormalizamos reserva_guid y sucursal_guid para evitar
-- viajes constantes a HotelLux_Reservation via gRPC.
-- Una estadia = un check-in fisico en una habitacion.
-- ============================================================
CREATE TABLE hospedaje.estadia (
    id_estadia                 INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    estadia_guid               UUID         NOT NULL DEFAULT gen_random_uuid(),
    reserva_habitacion_guid    UUID         NOT NULL, -- ref logica a reservas.reserva_habitacion
    reserva_guid               UUID         NOT NULL, -- ref logica a reservas.reserva (denorm)
    cliente_guid               UUID         NOT NULL, -- ref logica a reservas.cliente (denorm)
    sucursal_guid              UUID         NOT NULL, -- ref logica a alojamiento.sucursal (denorm)
    habitacion_guid            UUID         NOT NULL, -- ref logica a alojamiento.habitacion
    checkin_utc                TIMESTAMPTZ  NULL,
    checkout_utc               TIMESTAMPTZ  NULL,
    estado_estadia             CHAR(3)      NOT NULL DEFAULT 'ACT',
    -- ACT=Activa (check-in hecho, sin checkout)
    -- FIN=Finalizada | CAN=Cancelada
    observaciones_checkin      TEXT         NULL,
    observaciones_checkout     TEXT         NULL,
    requiere_mantenimiento     BOOLEAN      NOT NULL DEFAULT FALSE,
    fecha_registro_utc         TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario         CHAR(30)     NOT NULL,
    modificado_por_usuario     CHAR(30)     NULL,
    fecha_modificacion_utc     TIMESTAMPTZ  NULL,
    modificacion_ip            CHAR(25)     NULL,
    servicio_origen            CHAR(50)     NOT NULL DEFAULT 'stay-service',
    CONSTRAINT uq_estadia_guid        UNIQUE (estadia_guid),
    CONSTRAINT uq_estadia_reserva_hab UNIQUE (reserva_habitacion_guid),
    CONSTRAINT chk_estadia_estado     CHECK (estado_estadia IN ('ACT','FIN','CAN')),
    CONSTRAINT chk_estadia_fechas     CHECK (
        checkout_utc IS NULL OR checkin_utc IS NULL OR checkout_utc >= checkin_utc
    )
);


-- ============================================================
-- TABLA: hospedaje.cargo_estadia
--
-- Cargos de consumo generados durante la estadia (room service,
-- lavanderia, spa, etc.). catalogo_guid es referencia logica
-- a alojamiento.catalogo_servicios en HotelLux_Accommodation.
-- ============================================================
CREATE TABLE hospedaje.cargo_estadia (
    id_cargo_estadia           INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cargo_guid                 UUID          NOT NULL DEFAULT gen_random_uuid(),
    id_estadia                 INT           NOT NULL,
    catalogo_guid              UUID          NULL,     -- ref logica a alojamiento.catalogo_servicios
    descripcion_cargo          VARCHAR(250)  NOT NULL,
    cantidad                   INT           NOT NULL DEFAULT 1,
    precio_unitario            NUMERIC(12,2) NOT NULL,
    subtotal                   NUMERIC(12,2) NOT NULL, -- precio_unitario * cantidad
    valor_iva                  NUMERIC(12,2) NOT NULL DEFAULT 0, -- subtotal * 15 / 100
    total_cargo                NUMERIC(12,2) NOT NULL, -- subtotal + valor_iva
    fecha_consumo_utc          TIMESTAMPTZ   NOT NULL DEFAULT now(),
    estado_cargo               CHAR(3)       NOT NULL DEFAULT 'PEN',
    -- PEN=Pendiente de facturar | FAC=Facturado | ANU=Anulado
    fecha_registro_utc         TIMESTAMPTZ   NOT NULL DEFAULT now(),
    creado_por_usuario         CHAR(30)      NOT NULL,
    modificado_por_usuario     CHAR(30)      NULL,
    fecha_modificacion_utc     TIMESTAMPTZ   NULL,
    modificacion_ip            CHAR(25)      NULL,
    servicio_origen            CHAR(50)      NOT NULL DEFAULT 'stay-service',
    CONSTRAINT uq_cargo_estadia_guid    UNIQUE (cargo_guid),
    CONSTRAINT fk_cargo_estadia_estadia FOREIGN KEY (id_estadia)
        REFERENCES hospedaje.estadia(id_estadia),
    CONSTRAINT chk_cargo_estadia_cantidad CHECK (cantidad > 0),
    CONSTRAINT chk_cargo_estadia_precio   CHECK (precio_unitario >= 0),
    CONSTRAINT chk_cargo_estadia_subtotal CHECK (subtotal >= 0),
    CONSTRAINT chk_cargo_estadia_iva      CHECK (valor_iva >= 0),
    CONSTRAINT chk_cargo_estadia_total    CHECK (total_cargo >= 0),
    CONSTRAINT chk_cargo_estadia_estado   CHECK (estado_cargo IN ('PEN','FAC','ANU'))
);


-- ============================================================
-- TABLA: hospedaje.valoracion
--
-- Solo estadias en estado FIN pueden generar valoracion.
-- Fusionada aqui (antes era microservicio Rating separado).
-- Escala de puntuacion: 0.0 a 10.0.
-- ============================================================
CREATE TABLE hospedaje.valoracion (
    id_valoracion              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    valoracion_guid            UUID          NOT NULL DEFAULT gen_random_uuid(),
    id_estadia                 INT           NOT NULL,
    estadia_guid               UUID          NOT NULL,  -- denormalizado para evitar join
    cliente_guid               UUID          NOT NULL,  -- ref logica a reservas.cliente (denorm)
    sucursal_guid              UUID          NOT NULL,  -- ref logica a alojamiento.sucursal (denorm)
    habitacion_guid            UUID          NULL,      -- ref logica a alojamiento.habitacion (denorm)
    puntuacion_general         NUMERIC(3,1)  NOT NULL,
    puntuacion_limpieza        NUMERIC(3,1)  NULL,
    puntuacion_confort         NUMERIC(3,1)  NULL,
    puntuacion_ubicacion       NUMERIC(3,1)  NULL,
    puntuacion_instalaciones   NUMERIC(3,1)  NULL,
    puntuacion_personal        NUMERIC(3,1)  NULL,
    puntuacion_calidad_precio  NUMERIC(3,1)  NULL,
    comentario_positivo        TEXT          NULL,
    comentario_negativo        TEXT          NULL,
    tipo_viaje                 CHAR(20)      NULL,
    -- pareja | familia | negocios | amigos | solo
    estado_valoracion          CHAR(3)       NOT NULL DEFAULT 'PEN',
    -- PEN | PUB | OCU | REP
    publicada_en_portal        BOOLEAN       NOT NULL DEFAULT FALSE,
    respuesta_hotel            TEXT          NULL,
    fecha_respuesta_utc        TIMESTAMPTZ   NULL,
    moderada_por_usuario       CHAR(30)      NULL,
    motivo_moderacion          VARCHAR(150)  NULL,
    fecha_registro_utc         TIMESTAMPTZ   NOT NULL DEFAULT now(),
    creado_por_usuario         CHAR(30)      NOT NULL,
    modificado_por_usuario     CHAR(30)      NULL,
    fecha_modificacion_utc     TIMESTAMPTZ   NULL,
    modificacion_ip            CHAR(25)      NULL,
    servicio_origen            CHAR(50)      NOT NULL DEFAULT 'stay-service',
    CONSTRAINT uq_valoracion_guid         UNIQUE (valoracion_guid),
    CONSTRAINT uq_valoracion_estadia_clte UNIQUE (id_estadia, cliente_guid),
    CONSTRAINT fk_valoracion_estadia      FOREIGN KEY (id_estadia)
        REFERENCES hospedaje.estadia(id_estadia),
    CONSTRAINT chk_val_puntuacion    CHECK (puntuacion_general        BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_limp     CHECK (puntuacion_limpieza       IS NULL OR puntuacion_limpieza      BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_conf     CHECK (puntuacion_confort        IS NULL OR puntuacion_confort       BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_ubic     CHECK (puntuacion_ubicacion      IS NULL OR puntuacion_ubicacion     BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_inst     CHECK (puntuacion_instalaciones  IS NULL OR puntuacion_instalaciones BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_pers     CHECK (puntuacion_personal       IS NULL OR puntuacion_personal      BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_calp     CHECK (puntuacion_calidad_precio IS NULL OR puntuacion_calidad_precio BETWEEN 0 AND 10),
    CONSTRAINT chk_val_estado        CHECK (estado_valoracion IN ('PEN','PUB','OCU','REP')),
    CONSTRAINT chk_val_tipo_viaje    CHECK (tipo_viaje IS NULL OR
        tipo_viaje IN ('pareja','familia','negocios','amigos','solo'))
);


-- ============================================================
-- INDICES DE APOYO
-- ============================================================
CREATE INDEX ix_estadia_estado_habitacion
    ON hospedaje.estadia(estado_estadia, habitacion_guid, checkin_utc, checkout_utc);

CREATE INDEX ix_estadia_reserva
    ON hospedaje.estadia(reserva_guid);

CREATE INDEX ix_estadia_cliente
    ON hospedaje.estadia(cliente_guid);

CREATE INDEX ix_estadia_sucursal
    ON hospedaje.estadia(sucursal_guid, estado_estadia);

CREATE INDEX ix_cargo_estadia
    ON hospedaje.cargo_estadia(id_estadia, estado_cargo, fecha_consumo_utc);

CREATE INDEX ix_valoracion_sucursal_estado
    ON hospedaje.valoracion(sucursal_guid, estado_valoracion, publicada_en_portal);

CREATE INDEX ix_valoracion_cliente
    ON hospedaje.valoracion(cliente_guid);


-- ============================================================
-- ============================================================
-- DATOS SEMILLA
-- ============================================================
-- ============================================================


-- ============================================================
-- ESTADIAS (prefijo GUID: aaxxxxxx-...)
--
-- Escenario de cada estadia:
--   aa...002  Ana Lopez   GYE Suite Familiar  ACT  (en curso)
--   aa...003  Pedro Garcia CUE Suite Single   FIN  (completada)
--   aa...005  Carlos Mora  CUE Suite Doble    FIN  (historica)
--
-- Juan Perez  (RES-001, CON, junio)  → sin check-in aun, sin estadia
-- Turismo SA  (RES-004, CON, julio)  → sin check-in aun, sin estadia
--
-- Cross-BD referencias logicas (sin FK):
--   reserva_habitacion_guid : a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1XX
--   reserva_guid            : 99999999-9999-9999-9999-999999999XXX
--   cliente_guid            : 33333333-3333-3333-3333-333333333XXX
--   sucursal_guid           : 44444444-4444-4444-4444-444444444XXX
--   habitacion_guid         : 66666666-6666-6666-6666-66666666600X
-- ============================================================

-- ------------------------------------------------------------
-- Estadia 002: Ana Lopez — Guayaquil Suite Familiar — ACTIVA
--
-- Check-in: 2026-05-09 15:30 (15 min despues del hora oficial).
-- Checkout : NULL — todavia hospedada.
-- Hab GYE 201 quedo en estado OCU en HotelLux_Accommodation.
-- ------------------------------------------------------------
INSERT INTO hospedaje.estadia (
    estadia_guid,
    reserva_habitacion_guid, reserva_guid,
    cliente_guid, sucursal_guid, habitacion_guid,
    checkin_utc, checkout_utc,
    estado_estadia,
    observaciones_checkin,
    requiere_mantenimiento,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002',
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a102',  -- linea reserva 002
    '99999999-9999-9999-9999-999999999002',   -- reserva 002 Ana Lopez
    '33333333-3333-3333-3333-333333333002',   -- cliente Ana Lopez
    '44444444-4444-4444-4444-444444444002',   -- sucursal Guayaquil
    '66666666-6666-6666-6666-666666666007',   -- hab 201 GYE Suite Familiar
    '2026-05-09 15:30:00+00',
    NULL,
    'ACT',
    'Check-in sin novedades. Documento de identidad verificado. Huesped solicito cuna para infante; coordinada con housekeeping. 2 adultos + 1 bebe.',
    FALSE,
    'vendedor', '2026-05-09 15:30:00+00'
);

-- ------------------------------------------------------------
-- Estadia 003: Pedro Garcia — Cuenca Suite Single — FINALIZADA
--
-- Check-in : 2026-04-20 14:35 (walk-in anticipado 25 min,
--            autorizado por recepcion).
-- Checkout : 2026-04-23 11:45 (15 min antes del limite 12:00).
-- Hab CUE 101 volvio a estado DIS en HotelLux_Accommodation.
-- ------------------------------------------------------------
INSERT INTO hospedaje.estadia (
    estadia_guid,
    reserva_habitacion_guid, reserva_guid,
    cliente_guid, sucursal_guid, habitacion_guid,
    checkin_utc, checkout_utc,
    estado_estadia,
    observaciones_checkin,
    observaciones_checkout,
    requiere_mantenimiento,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003',
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a103',  -- linea reserva 003
    '99999999-9999-9999-9999-999999999003',   -- reserva 003 Pedro Garcia
    '33333333-3333-3333-3333-333333333003',   -- cliente Pedro Garcia
    '44444444-4444-4444-4444-444444444003',   -- sucursal Cuenca
    '66666666-6666-6666-6666-666666666008',   -- hab 101 Cuenca Suite Single
    '2026-04-20 14:35:00+00',
    '2026-04-23 11:45:00+00',
    'FIN',
    'Check-in walk-in anticipado autorizado por recepcion. Cedula verificada. Pago en efectivo registrado en caja. 1 adulto.',
    'Checkout sin novedades. Habitacion entregada en perfecto estado. Cliente muy satisfecho con el spa. No requiere mantenimiento adicional.',
    FALSE,
    'vendedor', '2026-04-20 14:35:00+00'
);

-- ------------------------------------------------------------
-- Estadia 005: Carlos Mora — Cuenca Suite Doble — FINALIZADA
--
-- Turista colombiano. Reservo por portal con antelacion.
-- Check-in : 2026-03-15 14:00 (hora exacta de la reserva).
-- Checkout : 2026-03-19 12:00 (puntual).
-- Hab CUE 102 volvio a estado DIS en HotelLux_Accommodation.
-- ------------------------------------------------------------
INSERT INTO hospedaje.estadia (
    estadia_guid,
    reserva_habitacion_guid, reserva_guid,
    cliente_guid, sucursal_guid, habitacion_guid,
    checkin_utc, checkout_utc,
    estado_estadia,
    observaciones_checkin,
    observaciones_checkout,
    requiere_mantenimiento,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa005',
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a105',  -- linea reserva 005
    '99999999-9999-9999-9999-999999999005',   -- reserva 005 Carlos Mora
    '33333333-3333-3333-3333-333333333005',   -- cliente Carlos Mora (PAS)
    '44444444-4444-4444-4444-444444444003',   -- sucursal Cuenca
    '66666666-6666-6666-6666-666666666009',   -- hab 102 Cuenca Suite Doble
    '2026-03-15 14:00:00+00',
    '2026-03-19 12:00:00+00',
    'FIN',
    'Check-in turista extranjero. Pasaporte PA12345678 verificado. Pago registrado (tarjeta Mastercard internacional prepagada en reserva). 2 adultos.',
    'Checkout puntual. Habitacion en perfecto estado. Cliente dejo comentarios muy positivos sobre el centro historico de Cuenca. No requiere mantenimiento.',
    FALSE,
    'vendedor', '2026-03-15 14:00:00+00'
);


-- ============================================================
-- CARGOS DE ESTADIA (prefijo GUID: cccccccc-cccc-cccc-cccc-cccccccc1XXX)
--
-- catalogo_guid apunta logicamente a alojamiento.catalogo_servicios:
--   88...005 = SRV-HAB (Room Service)     $15.00 + IVA → Guayaquil
--   88...006 = SRV-LAV (Lavanderia)       $12.50 + IVA → Guayaquil
--   88...007 = SRV-SPA (Spa Andes)        $45.00 + IVA → Cuenca
--
-- Calculos IVA (15% Ecuador):
--   cc1001: $15.00 × 15% = $2.25   → total $17.25
--   cc1002: $12.50 × 15% = $1.88   → total $14.38  (redondeo bancario)
--   cc1003: $45.00 × 15% = $6.75   → total $51.75
--   cc1004: $45.00 × 15% = $6.75   → total $51.75
-- ============================================================

-- ------------------------------------------------------------
-- Cargo cc1001: Room Service — Ana Lopez (estadia ACT GYE)
-- 1 x $15.00 | IVA $2.25 | Total $17.25 | PEN
-- Cena familiar a la habitacion: noche del check-in.
-- ------------------------------------------------------------
INSERT INTO hospedaje.cargo_estadia (
    cargo_guid,
    id_estadia, catalogo_guid,
    descripcion_cargo,
    cantidad, precio_unitario,
    subtotal, valor_iva, total_cargo,
    fecha_consumo_utc, estado_cargo,
    creado_por_usuario, fecha_registro_utc
) VALUES
(
    'cccccccc-cccc-cccc-cccc-cccccccc1001',
    (SELECT id_estadia FROM hospedaje.estadia
     WHERE estadia_guid = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002'),
    '88888888-8888-8888-8888-888888888005',   -- SRV-HAB Guayaquil
    'Servicio a la habitacion - cena familiar (seco de pollo, arroz con menestra, jugo de naranjilla)',
    1, 15.00,
    15.00, 2.25, 17.25,
    '2026-05-09 20:15:00+00', 'PEN',
    'vendedor', '2026-05-09 20:15:00+00'
),
-- ------------------------------------------------------------
-- Cargo cc1002: Lavanderia — Ana Lopez (estadia ACT GYE)
-- 1 x $12.50 | IVA $1.88 | Total $14.38 | PEN
-- 3 prendas de vestir, solicitado dia 2, entrega dia 3.
-- ------------------------------------------------------------
(
    'cccccccc-cccc-cccc-cccc-cccccccc1002',
    (SELECT id_estadia FROM hospedaje.estadia
     WHERE estadia_guid = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002'),
    '88888888-8888-8888-8888-888888888006',   -- SRV-LAV Guayaquil
    'Lavanderia y planchado - 3 prendas de vestir adulto (2 camisas, 1 pantalon)',
    1, 12.50,
    12.50, 1.88, 14.38,
    '2026-05-10 08:30:00+00', 'PEN',
    'vendedor', '2026-05-10 08:30:00+00'
);

-- ------------------------------------------------------------
-- Cargo cc1003: Spa — Pedro Garcia (estadia FIN Cuenca)
-- 1 x $45.00 | IVA $6.75 | Total $51.75 | FAC
-- Masaje relajante 60 min, tarde del dia 2. Incluido en factura.
-- ------------------------------------------------------------
INSERT INTO hospedaje.cargo_estadia (
    cargo_guid,
    id_estadia, catalogo_guid,
    descripcion_cargo,
    cantidad, precio_unitario,
    subtotal, valor_iva, total_cargo,
    fecha_consumo_utc, estado_cargo,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'cccccccc-cccc-cccc-cccc-cccccccc1003',
    (SELECT id_estadia FROM hospedaje.estadia
     WHERE estadia_guid = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003'),
    '88888888-8888-8888-8888-888888888007',   -- SRV-SPA Cuenca
    'Spa Andes Wellness - masaje relajante 60 min con aceites esenciales de rosas de Quito',
    1, 45.00,
    45.00, 6.75, 51.75,
    '2026-04-22 16:00:00+00', 'FAC',
    'vendedor', '2026-04-22 16:00:00+00'
);

-- ------------------------------------------------------------
-- Cargo cc1004: Spa — Carlos Mora (estadia FIN Cuenca)
-- 1 x $45.00 | IVA $6.75 | Total $51.75 | FAC
-- Masaje de tejido profundo el dia 3. Incluido en factura final.
-- ------------------------------------------------------------
INSERT INTO hospedaje.cargo_estadia (
    cargo_guid,
    id_estadia, catalogo_guid,
    descripcion_cargo,
    cantidad, precio_unitario,
    subtotal, valor_iva, total_cargo,
    fecha_consumo_utc, estado_cargo,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'cccccccc-cccc-cccc-cccc-cccccccc1004',
    (SELECT id_estadia FROM hospedaje.estadia
     WHERE estadia_guid = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa005'),
    '88888888-8888-8888-8888-888888888007',   -- SRV-SPA Cuenca
    'Spa Andes Wellness - masaje de tejido profundo 60 min con barro andino',
    1, 45.00,
    45.00, 6.75, 51.75,
    '2026-03-17 17:00:00+00', 'FAC',
    'vendedor', '2026-03-17 17:00:00+00'
);


-- ============================================================
-- VALORACIONES (prefijo GUID: ddxxxxxx-...)
--
-- Solo estadias FIN pueden tener valoracion.
-- Escala: 0.0 a 10.0.
--
--   dd...1003  Pedro Garcia  CUE Single   PUB  (9.0 general)
--   dd...1005  Carlos Mora   CUE Doble    PUB  (8.5 general)
--
-- NOTA: creado_por_usuario = 'vendedor' en ambos casos porque
-- pedro.garcia y cmora.viajes no tienen cuenta en HotelLux_Auth.
-- El personal de recepcion registra la valoracion recibida
-- por correo tras el checkout.
-- ============================================================

-- ------------------------------------------------------------
-- Valoracion dd1003: Pedro Garcia — Cuenca Suite Single
-- Estadia FIN: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003
-- Puntuacion general: 9.0 | tipo_viaje: solo | estado: PUB
-- ------------------------------------------------------------
INSERT INTO hospedaje.valoracion (
    valoracion_guid,
    id_estadia, estadia_guid,
    cliente_guid, sucursal_guid, habitacion_guid,
    puntuacion_general,
    puntuacion_limpieza, puntuacion_confort,
    puntuacion_ubicacion, puntuacion_instalaciones,
    puntuacion_personal, puntuacion_calidad_precio,
    comentario_positivo, comentario_negativo,
    tipo_viaje, estado_valoracion, publicada_en_portal,
    respuesta_hotel, fecha_respuesta_utc,
    moderada_por_usuario,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'dddddddd-dddd-dddd-dddd-dddddddd1003',
    (SELECT id_estadia FROM hospedaje.estadia
     WHERE estadia_guid = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003'),
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003',
    '33333333-3333-3333-3333-333333333003',  -- Pedro Garcia
    '44444444-4444-4444-4444-444444444003',  -- sucursal Cuenca
    '66666666-6666-6666-6666-666666666008',  -- hab 101 Cuenca Suite Single
    9.0,
    9.5,   -- limpieza: habitacion impecable
    9.0,   -- confort: cama comoda, temperatura agradable
    10.0,  -- ubicacion: centro historico, inmejorable
    8.5,   -- instalaciones: casona hermosa, bano algo pequeno
    9.5,   -- personal: atencion excelente en recepcion y spa
    8.5,   -- calidad/precio: precio justo para la experiencia
    'Ubicacion inmejorable en el centro historico de Cuenca. La casona colonial es preciosa, se siente la historia en cada detalle. El personal de recepcion fue muy atento y el masaje en el Spa Andes Wellness fue lo mejor de la estadia. El desayuno incluso productos locales: pan de yuca, jugos de mora y naranjilla fresca.',
    'El WiFi en la habitacion tuvo intermitencia en las noches. El bano de la Suite Single es algo reducido para una propiedad de esta categoria.',
    'solo',
    'PUB', TRUE,
    'Estimado Sr. Garcia, muchas gracias por su visita y por compartir su experiencia. Nos alegra que haya disfrutado de la casona y del Spa Andes Wellness. Ya trabajamos en mejorar la conectividad WiFi. Lo esperamos pronto en el Hotel Luxemburgo Cuenca.',
    '2026-04-26 09:00:00+00',
    'admin',
    'vendedor', '2026-04-25 09:00:00+00'
);

-- ------------------------------------------------------------
-- Valoracion dd1005: Carlos Mora — Cuenca Suite Doble
-- Estadia FIN: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa005
-- Puntuacion general: 8.5 | tipo_viaje: solo | estado: PUB
-- ------------------------------------------------------------
INSERT INTO hospedaje.valoracion (
    valoracion_guid,
    id_estadia, estadia_guid,
    cliente_guid, sucursal_guid, habitacion_guid,
    puntuacion_general,
    puntuacion_limpieza, puntuacion_confort,
    puntuacion_ubicacion, puntuacion_instalaciones,
    puntuacion_personal, puntuacion_calidad_precio,
    comentario_positivo, comentario_negativo,
    tipo_viaje, estado_valoracion, publicada_en_portal,
    respuesta_hotel, fecha_respuesta_utc,
    moderada_por_usuario,
    creado_por_usuario, fecha_registro_utc
) VALUES (
    'dddddddd-dddd-dddd-dddd-dddddddd1005',
    (SELECT id_estadia FROM hospedaje.estadia
     WHERE estadia_guid = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa005'),
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa005',
    '33333333-3333-3333-3333-333333333005',  -- Carlos Mora (PAS)
    '44444444-4444-4444-4444-444444444003',  -- sucursal Cuenca
    '66666666-6666-6666-6666-666666666009',  -- hab 102 Cuenca Suite Doble
    8.5,
    9.0,   -- limpieza: muy buena
    8.5,   -- confort: dos camas Queen muy comodas
    10.0,  -- ubicacion: centro historico, ideal para recorrer a pie
    8.0,   -- instalaciones: buenas, la casona impresiona
    9.0,   -- personal: amables y con muy buen ingles para turistas
    8.5,   -- calidad/precio: precio competitivo frente a otros hoteles
    'Cuenca es una ciudad encantadora y el hotel encaja perfecto con el ambiente colonial. La habitacion doble fue espaciosa y comoda para dos personas. El personal hablo ingles sin problema, muy util como turista extranjero. El spa fue una experiencia unica con productos andinos. Excelente ubicacion para recorrer la ciudad a pie.',
    'El servicio de desayuno no estaba incluido y el precio del restaurante interno fue un poco elevado. La ventilacion en las noches de marzo puede ser fria, faltaria una frazada adicional.',
    'solo',
    'PUB', TRUE,
    'Dear Mr. Mora, thank you for choosing Hotel Luxemburgo Cuenca during your visit to Ecuador. We are glad you enjoyed the spa and our bilingual team. We have noted your feedback about breakfast and room temperature — we will add an extra blanket kit to all rooms. We hope to welcome you back soon!',
    '2026-03-22 10:00:00+00',
    'admin',
    'vendedor', '2026-03-21 08:00:00+00'
);


-- ============================================================
-- VERIFICACION FINAL
-- Resultados esperados:
--   Estadias          : 3
--   Cargos de estadia : 4
--   Valoraciones      : 2
-- ============================================================
SELECT 'Estadias:           ' || COUNT(*)::text AS resultado FROM hospedaje.estadia;
SELECT 'Cargos de estadia:  ' || COUNT(*)::text AS resultado FROM hospedaje.cargo_estadia;
SELECT 'Valoraciones:       ' || COUNT(*)::text AS resultado FROM hospedaje.valoracion;

-- Vista resumen: estadias con totales de cargos
SELECT
    e.estadia_guid,
    TRIM(e.estado_estadia)                          AS estado,
    e.checkin_utc::date                             AS checkin,
    e.checkout_utc::date                            AS checkout,
    COUNT(c.id_cargo_estadia)                       AS cargos,
    COALESCE(SUM(c.total_cargo), 0)                 AS suma_cargos_usd,
    COUNT(c.id_cargo_estadia)
        FILTER (WHERE c.estado_cargo = 'PEN')       AS cargos_pen,
    COUNT(c.id_cargo_estadia)
        FILTER (WHERE c.estado_cargo = 'FAC')       AS cargos_fac
FROM       hospedaje.estadia e
LEFT JOIN  hospedaje.cargo_estadia c ON c.id_estadia = e.id_estadia
GROUP BY   e.id_estadia, e.estadia_guid, e.estado_estadia,
           e.checkin_utc, e.checkout_utc
ORDER BY   e.checkin_utc;

-- Vista resumen: valoraciones publicadas
SELECT
    v.valoracion_guid,
    TRIM(v.sucursal_guid::text)                     AS sucursal,
    v.puntuacion_general,
    v.puntuacion_limpieza,
    v.puntuacion_confort,
    v.puntuacion_ubicacion,
    v.puntuacion_instalaciones,
    v.puntuacion_personal,
    v.puntuacion_calidad_precio,
    TRIM(v.tipo_viaje)                              AS tipo_viaje,
    TRIM(v.estado_valoracion)                       AS estado,
    v.publicada_en_portal
FROM  hospedaje.valoracion v
ORDER BY v.fecha_registro_utc;