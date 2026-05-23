-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio STAY
-- Base de datos: HotelLux_Stay
-- Motor: PostgreSQL 18
-- Version: 2.0
--
-- ALCANCE: fusiona stay + rating. Maneja todo lo que ocurre
-- DURANTE y DESPUES de la estadia: check-in, cargos de
-- consumo, check-out y valoraciones del huesped.
--
-- DEPENDENCIAS LOGICAS (sin FK fisica):
--   - HotelLux_Reservation : reserva_guid, reserva_habitacion_guid,
--                            cliente_guid
--   - HotelLux_Accommodation: sucursal_guid, habitacion_guid,
--                             catalogo_guid
--
-- CONTENIDO:
--   Schema: hospedaje
--   Tablas: estadia, cargo_estadia, valoracion
--
--   Datos semilla:
--     2 estadias:
--       aa...002 = Ana Lopez   (GYE, ACT — en curso, sin checkout)
--       aa...003 = Pedro Garcia (Cuenca, FIN — completada)
--     3 cargos de consumo:
--       cc1001 = Room Service Ana     $17.25  PEN
--       cc1002 = Lavanderia Ana       $14.38  PEN
--       cc1003 = Spa Pedro            $51.75  FAC
--     1 valoracion publicada:
--       dd...1003 = Pedro Garcia, 9.0/10, PUB
--
-- NOTA: Reserva Juan Perez (RES-001) esta en estado CON pero
-- no ha realizado check-in todavia => NO tiene estadia aqui.
--
-- VALIDACION MATEMATICA (IVA 15%):
--   cc1001: 1 x $15.00 = $15.00 + $2.25 IVA  = $17.25
--   cc1002: 1 x $12.50 = $12.50 + $1.88 IVA  = $14.38
--   cc1003: 1 x $45.00 = $45.00 + $6.75 IVA  = $51.75
--
-- INSTRUCCIONES EN pgAdmin:
--   1. Create Database: HotelLux_Stay / Owner: postgres
--   2. Query Tool -> File -> Open este archivo -> F5
--   3. Verificar los SELECT de conteo al final.
-- ============================================================


-- ============================================================
-- SCHEMA
-- ============================================================
CREATE SCHEMA IF NOT EXISTS hospedaje;


-- ============================================================
-- TABLA: hospedaje.estadia
-- Denormalizamos reserva_guid y sucursal_guid para evitar
-- viajes constantes a HotelLux_Reservation via gRPC.
-- ============================================================
CREATE TABLE hospedaje.estadia (
    id_estadia                 INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    estadia_guid               UUID         NOT NULL DEFAULT gen_random_uuid(),
    reserva_habitacion_guid    UUID         NOT NULL,
    reserva_guid               UUID         NOT NULL,
    cliente_guid               UUID         NOT NULL,
    sucursal_guid              UUID         NOT NULL,
    habitacion_guid            UUID         NOT NULL,
    checkin_utc                TIMESTAMPTZ  NULL,
    checkout_utc               TIMESTAMPTZ  NULL,
    estado_estadia             CHAR(3)      NOT NULL DEFAULT 'ACT',
    observaciones_checkin      TEXT         NULL,
    observaciones_checkout     TEXT         NULL,
    requiere_mantenimiento     BOOLEAN      NOT NULL DEFAULT FALSE,
    fecha_registro_utc         TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario         VARCHAR(100) NOT NULL,
    modificado_por_usuario     VARCHAR(100) NULL,
    fecha_modificacion_utc     TIMESTAMPTZ  NULL,
    modificacion_ip            VARCHAR(45)  NULL,
    servicio_origen            VARCHAR(50)  NOT NULL DEFAULT 'stay-service',
    CONSTRAINT uq_estadia_guid        UNIQUE (estadia_guid),
    CONSTRAINT uq_estadia_reserva_hab UNIQUE (reserva_habitacion_guid),
    -- ACT=Activa (check-in hecho, sin checkout) | FIN=Finalizada | CAN=Cancelada
    CONSTRAINT chk_estadia_estado CHECK (estado_estadia IN ('ACT','FIN','CAN')),
    CONSTRAINT chk_estadia_fechas CHECK (
        checkout_utc IS NULL OR checkin_utc IS NULL OR checkout_utc >= checkin_utc
    )
);


-- ============================================================
-- TABLA: hospedaje.cargo_estadia
-- Cargos de consumo generados durante la estadia.
-- catalogo_guid es referencia logica a HotelLux_Accommodation.
-- ============================================================
CREATE TABLE hospedaje.cargo_estadia (
    id_cargo_estadia           INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cargo_guid                 UUID          NOT NULL DEFAULT gen_random_uuid(),
    id_estadia                 INT           NOT NULL,
    catalogo_guid              UUID          NULL,
    descripcion_cargo          VARCHAR(250)  NOT NULL,
    cantidad                   INT           NOT NULL DEFAULT 1,
    precio_unitario            NUMERIC(12,2) NOT NULL,
    subtotal                   NUMERIC(12,2) NOT NULL,
    valor_iva                  NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_cargo                NUMERIC(12,2) NOT NULL,
    fecha_consumo_utc          TIMESTAMPTZ   NOT NULL DEFAULT now(),
    -- PEN=Pendiente de facturar | FAC=Facturado | ANU=Anulado
    estado_cargo               CHAR(3)       NOT NULL DEFAULT 'PEN',
    fecha_registro_utc         TIMESTAMPTZ   NOT NULL DEFAULT now(),
    creado_por_usuario         VARCHAR(100)  NOT NULL,
    modificado_por_usuario     VARCHAR(100)  NULL,
    fecha_modificacion_utc     TIMESTAMPTZ   NULL,
    modificacion_ip            VARCHAR(45)   NULL,
    servicio_origen            VARCHAR(50)   NOT NULL DEFAULT 'stay-service',
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
-- Toda valoracion requiere una estadia FIN local.
-- Fusionada aqui (antes era su propio microservicio Rating).
-- ============================================================
CREATE TABLE hospedaje.valoracion (
    id_valoracion              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    valoracion_guid            UUID          NOT NULL DEFAULT gen_random_uuid(),
    id_estadia                 INT           NOT NULL,
    estadia_guid               UUID          NOT NULL,                        -- denormalizado para evitar join
    cliente_guid               UUID          NOT NULL,
    sucursal_guid              UUID          NOT NULL,
    habitacion_guid            UUID          NULL,
    puntuacion_general         NUMERIC(3,1)  NOT NULL,
    puntuacion_limpieza        NUMERIC(3,1)  NULL,
    puntuacion_confort         NUMERIC(3,1)  NULL,
    puntuacion_ubicacion       NUMERIC(3,1)  NULL,
    puntuacion_instalaciones   NUMERIC(3,1)  NULL,
    puntuacion_personal        NUMERIC(3,1)  NULL,
    puntuacion_calidad_precio  NUMERIC(3,1)  NULL,
    comentario_positivo        TEXT          NULL,
    comentario_negativo        TEXT          NULL,
    tipo_viaje                 VARCHAR(20)   NULL,
    estado_valoracion          CHAR(3)       NOT NULL DEFAULT 'PEN',
    publicada_en_portal        BOOLEAN       NOT NULL DEFAULT FALSE,
    respuesta_hotel            TEXT          NULL,
    fecha_respuesta_utc        TIMESTAMPTZ   NULL,
    moderada_por_usuario       VARCHAR(100)  NULL,
    motivo_moderacion          VARCHAR(250)  NULL,
    fecha_registro_utc         TIMESTAMPTZ   NOT NULL DEFAULT now(),
    creado_por_usuario         VARCHAR(100)  NOT NULL,
    modificado_por_usuario     VARCHAR(100)  NULL,
    fecha_modificacion_utc     TIMESTAMPTZ   NULL,
    modificacion_ip            VARCHAR(45)   NULL,
    servicio_origen            VARCHAR(50)   NOT NULL DEFAULT 'stay-service',
    CONSTRAINT uq_valoracion_guid         UNIQUE (valoracion_guid),
    CONSTRAINT uq_valoracion_estadia_clte UNIQUE (id_estadia, cliente_guid),
    CONSTRAINT fk_valoracion_estadia      FOREIGN KEY (id_estadia)
        REFERENCES hospedaje.estadia(id_estadia),
    CONSTRAINT chk_val_puntuacion      CHECK (puntuacion_general          BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_limp       CHECK (puntuacion_limpieza         IS NULL OR puntuacion_limpieza        BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_conf       CHECK (puntuacion_confort          IS NULL OR puntuacion_confort         BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_ubic       CHECK (puntuacion_ubicacion        IS NULL OR puntuacion_ubicacion       BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_inst       CHECK (puntuacion_instalaciones    IS NULL OR puntuacion_instalaciones   BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_pers       CHECK (puntuacion_personal         IS NULL OR puntuacion_personal        BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_calp       CHECK (puntuacion_calidad_precio   IS NULL OR puntuacion_calidad_precio  BETWEEN 0 AND 10),
    -- PEN | PUB | OCU | REP
    CONSTRAINT chk_val_estado          CHECK (estado_valoracion IN ('PEN','PUB','OCU','REP')),
    CONSTRAINT chk_val_tipo_viaje      CHECK (tipo_viaje IS NULL OR
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
--   aa...002 = Ana Lopez   (GYE)    -- ACT, sin checkout
--   aa...003 = Pedro Garcia (Cuenca) -- FIN, con checkout
--
-- Juan Perez (RES-001) esta en CON pero sin check-in => sin estadia.
-- ============================================================

-- ----------------------------------------------------------
-- Estadia 002: Ana Lopez — Guayaquil — ACTIVA
--
-- Check-in: 2026-05-09 a las 15:30 (15 min despues del
-- horario oficial de checkin 15:00).
-- Checkout: NULL (todavia hospedada).
-- Habitacion GYE 201 queda en estado OCU en Accommodation.
-- ----------------------------------------------------------
INSERT INTO hospedaje.estadia (
    estadia_guid,
    reserva_habitacion_guid,
    reserva_guid,
    cliente_guid,
    sucursal_guid,
    habitacion_guid,
    checkin_utc,
    checkout_utc,
    estado_estadia,
    observaciones_checkin,
    requiere_mantenimiento,
    creado_por_usuario
) VALUES
(
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002',
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a102',  -- linea reserva 002
    '99999999-9999-9999-9999-999999999002',   -- reserva 002 Ana Lopez
    '33333333-3333-3333-3333-333333333002',   -- cliente Ana Lopez
    '44444444-4444-4444-4444-444444444002',   -- sucursal Guayaquil
    '66666666-6666-6666-6666-666666666007',   -- hab 201 GYE Suite Familiar
    '2026-05-09 15:30:00+00',
    NULL,
    'ACT',
    'Check-in realizado sin novedades. Documento de identidad verificado. Huesped solicito cuna para infante, coordinada con housekeeping. 2 adultos + 1 bebe.',
    FALSE,
    'vendedor'
);

-- ----------------------------------------------------------
-- Estadia 003: Pedro Garcia — Cuenca — FINALIZADA
--
-- Check-in : 2026-04-20 a las 14:35 (walk-in, 25 min antes
--            del horario oficial 15:00, se acepto en recepcion).
-- Checkout : 2026-04-23 a las 11:45 (15 min antes del limite 12:00).
-- Habitacion Cuenca 101 volvio a estado DIS en Accommodation.
-- ----------------------------------------------------------
INSERT INTO hospedaje.estadia (
    estadia_guid,
    reserva_habitacion_guid,
    reserva_guid,
    cliente_guid,
    sucursal_guid,
    habitacion_guid,
    checkin_utc,
    checkout_utc,
    estado_estadia,
    observaciones_checkin,
    observaciones_checkout,
    requiere_mantenimiento,
    creado_por_usuario
) VALUES
(
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003',
    'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a103',  -- linea reserva 003
    '99999999-9999-9999-9999-999999999003',   -- reserva 003 Pedro Garcia
    '33333333-3333-3333-3333-333333333003',   -- cliente Pedro Garcia
    '44444444-4444-4444-4444-444444444003',   -- sucursal Cuenca
    '66666666-6666-6666-6666-666666666008',   -- hab 101 Cuenca Suite Single
    '2026-04-20 14:35:00+00',
    '2026-04-23 11:45:00+00',
    'FIN',
    'Check-in walk-in anticipado autorizado por recepcion. Cedula verificada. Pago en efectivo registrado en caja. 1 adulto, sin acompanantes.',
    'Checkout sin novedades. Habitacion entregada en perfecto estado. Cliente satisfecho. No requiere mantenimiento adicional.',
    FALSE,
    'vendedor'
);


-- ============================================================
-- CARGOS DE ESTADIA (prefijo GUID: cc...1xxx)
--
-- Ana Lopez (estadia ACT): 2 cargos pendientes de facturar.
-- Pedro Garcia (estadia FIN): 1 cargo ya facturado al checkout.
--
-- catalogo_guid apunta logicamente a HotelLux_Accommodation:
--   88...005 = SRV-HAB (Room Service)  $15.00 + IVA
--   88...006 = SRV-LAV (Lavanderia)    $12.50 + IVA
--   88...007 = SRV-SPA (Spa)           $45.00 + IVA
-- ============================================================

-- ----------------------------------------------------------
-- Cargo cc1001: Room Service — Ana Lopez
-- 1 x $15.00 = $15.00 + $2.25 IVA = $17.25  |  PEN
-- Consumo: noche del check-in, cena familiar a la habitacion.
-- ----------------------------------------------------------
INSERT INTO hospedaje.cargo_estadia (
    cargo_guid,
    id_estadia,
    catalogo_guid,
    descripcion_cargo,
    cantidad, precio_unitario,
    subtotal, valor_iva, total_cargo,
    fecha_consumo_utc,
    estado_cargo,
    creado_por_usuario
) VALUES
(
    'cccccccc-cccc-cccc-cccc-cccccccc1001',
    (SELECT id_estadia FROM hospedaje.estadia WHERE estadia_guid = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002'),
    '88888888-8888-8888-8888-888888888005',   -- SRV-HAB catalogo
    'Servicio a la habitacion - cena familiar (seco de pollo, arroz con menestra)',
    1, 15.00,
    15.00, 2.25, 17.25,
    '2026-05-09 20:15:00+00',
    'PEN',
    'vendedor'
),
-- ----------------------------------------------------------
-- Cargo cc1002: Lavanderia — Ana Lopez
-- 1 x $12.50 = $12.50 + $1.88 IVA = $14.38  |  PEN
-- Consumo: manana del segundo dia, entrega solicitada para el dia siguiente.
-- ----------------------------------------------------------
(
    'cccccccc-cccc-cccc-cccc-cccccccc1002',
    (SELECT id_estadia FROM hospedaje.estadia WHERE estadia_guid = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002'),
    '88888888-8888-8888-8888-888888888006',   -- SRV-LAV catalogo
    'Lavanderia y planchado - 3 prendas de vestir adulto',
    1, 12.50,
    12.50, 1.88, 14.38,
    '2026-05-10 08:30:00+00',
    'PEN',
    'vendedor'
);

-- ----------------------------------------------------------
-- Cargo cc1003: Spa — Pedro Garcia
-- 1 x $45.00 = $45.00 + $6.75 IVA = $51.75  |  FAC
-- Consumo: tarde del segundo dia, masaje relajante 60 min.
-- Ya fue incluido en la factura final al checkout.
-- ----------------------------------------------------------
INSERT INTO hospedaje.cargo_estadia (
    cargo_guid,
    id_estadia,
    catalogo_guid,
    descripcion_cargo,
    cantidad, precio_unitario,
    subtotal, valor_iva, total_cargo,
    fecha_consumo_utc,
    estado_cargo,
    creado_por_usuario
) VALUES
(
    'cccccccc-cccc-cccc-cccc-cccccccc1003',
    (SELECT id_estadia FROM hospedaje.estadia WHERE estadia_guid = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003'),
    '88888888-8888-8888-8888-888888888007',   -- SRV-SPA catalogo
    'Spa Andes Wellness - masaje relajante 60 min con aceites esenciales de rosas de Quito',
    1, 45.00,
    45.00, 6.75, 51.75,
    '2026-04-22 16:00:00+00',
    'FAC',
    'vendedor'
);


-- ============================================================
-- VALORACIONES (prefijo GUID: ddxxxxxx-...)
--
-- Solo Pedro Garcia tiene valoracion porque es el unico con
-- estadia FIN. Ana Lopez (ACT) no puede valorar aun.
--
-- Escala de puntuacion: 0.0 a 10.0
-- tipo_viaje: 'solo' (viajo sin acompanantes)
-- estado: PUB (moderada y publicada en portal)
-- ============================================================
INSERT INTO hospedaje.valoracion (
    valoracion_guid,
    id_estadia,
    estadia_guid,
    cliente_guid,
    sucursal_guid,
    habitacion_guid,
    puntuacion_general,
    puntuacion_limpieza,
    puntuacion_confort,
    puntuacion_ubicacion,
    puntuacion_instalaciones,
    puntuacion_personal,
    puntuacion_calidad_precio,
    comentario_positivo,
    comentario_negativo,
    tipo_viaje,
    estado_valoracion,
    publicada_en_portal,
    respuesta_hotel,
    fecha_respuesta_utc,
    moderada_por_usuario,
    creado_por_usuario
) VALUES
(
    'dddddddd-dddd-dddd-dddd-dddddddd1003',
    (SELECT id_estadia FROM hospedaje.estadia WHERE estadia_guid = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003'),
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003',   -- estadia_guid denormalizado
    '33333333-3333-3333-3333-333333333003',   -- Pedro Garcia
    '44444444-4444-4444-4444-444444444003',   -- sucursal Cuenca
    '66666666-6666-6666-6666-666666666008',   -- hab 101 Cuenca Single
    9.0,
    9.5,   -- limpieza: habitacion impecable
    9.0,   -- confort: cama comoda, temperatura agradable
    10.0,  -- ubicacion: centro historico, perfecto
    8.5,   -- instalaciones: casona hermosa, bano algo pequeno
    9.5,   -- personal: atencion excelente en recepcion y spa
    8.5,   -- calidad/precio: precio justo para la experiencia
    'Ubicacion inmejorable en pleno centro historico de Cuenca. La casona colonial es preciosa, se siente la historia en cada detalle. El personal de recepcion fue muy atento y el masaje en el Spa Andes Wellness fue lo mejor de la estadia. El desayuno incluyo productos locales como pan de yuca y jugos de mora y naranjilla.',
    'El WiFi en la habitacion tuvo intermitencia en las noches. El bano de la Suite Single es algo reducido para una propiedad de esta categoria.',
    'solo',
    'PUB',
    TRUE,
    'Estimado Sr. Garcia, muchas gracias por su visita y por compartir su experiencia. Nos alegra que haya disfrutado de nuestra casona y del Spa Andes Wellness. Ya estamos trabajando en mejorar la conectividad WiFi en todas las habitaciones. Lo esperamos pronto en el Hotel Luxemburgo Cuenca.',
    '2026-04-26 09:00:00+00',
    'admin',
    'pedro.garcia'
);


-- ============================================================
-- VERIFICACION FINAL
-- Resultados esperados:
--   Estadias          : 2
--   Cargos de estadia : 3
--   Valoraciones      : 1
-- ============================================================
SELECT 'Estadias:           ' || COUNT(*)::text AS resultado FROM hospedaje.estadia;
SELECT 'Cargos de estadia:  ' || COUNT(*)::text AS resultado FROM hospedaje.cargo_estadia;
SELECT 'Valoraciones:       ' || COUNT(*)::text AS resultado FROM hospedaje.valoracion;

-- Vista resumen: estadias con sus cargos
SELECT
    e.estadia_guid,
    e.cliente_guid,
    e.estado_estadia                          AS estado,
    e.checkin_utc::date                       AS checkin,
    e.checkout_utc::date                      AS checkout,
    COUNT(c.id_cargo_estadia)                 AS total_cargos,
    COALESCE(SUM(c.total_cargo), 0)           AS suma_cargos_usd,
    COUNT(c.id_cargo_estadia)
        FILTER (WHERE c.estado_cargo = 'PEN') AS cargos_pendientes,
    COUNT(c.id_cargo_estadia)
        FILTER (WHERE c.estado_cargo = 'FAC') AS cargos_facturados
FROM       hospedaje.estadia e
LEFT JOIN  hospedaje.cargo_estadia c ON c.id_estadia = e.id_estadia
GROUP BY   e.id_estadia
ORDER BY   e.checkin_utc;

-- Vista de la valoracion publicada
SELECT
    v.valoracion_guid,
    v.puntuacion_general,
    v.puntuacion_limpieza,
    v.puntuacion_confort,
    v.puntuacion_ubicacion,
    v.puntuacion_instalaciones,
    v.puntuacion_personal,
    v.puntuacion_calidad_precio,
    v.tipo_viaje,
    v.estado_valoracion,
    v.publicada_en_portal
FROM hospedaje.valoracion v;