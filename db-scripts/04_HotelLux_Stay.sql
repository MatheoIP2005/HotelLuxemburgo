-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio STAY
-- Base de datos: HotelLux_Stay
-- Motor: PostgreSQL 18
-- Version: 1.0
--
-- ALCANCE: este microservicio FUSIONA stay + rating del plan
-- original (decision: 6 microservicios). Maneja todo lo que pasa
-- DURANTE y DESPUES de la estadia del huesped: check-in, cargos
-- de consumo, check-out y valoraciones.
--
-- DEPENDENCIAS LOGICAS (sin FK fisica):
--   - HotelLux_Reservation:    reserva_guid, reserva_habitacion_guid
--   - HotelLux_Reservation:    cliente_guid
--   - HotelLux_Accommodation:  sucursal_guid, habitacion_guid, catalogo_guid
--
-- CONTENIDO:
--   Schema: hospedaje
--   Tablas: estadia, cargo_estadia, valoracion
--   Datos semilla: 2 estadias (1 en curso, 1 finalizada),
--                  3 cargos, 1 valoracion publicada
--
-- INSTRUCCIONES EN pgAdmin:
--   1. Create Database: HotelLux_Stay
--   2. Query Tool -> Open file -> F5
-- ============================================================


-- ============================================================
-- SCHEMA
-- ============================================================
CREATE SCHEMA IF NOT EXISTS hospedaje;


-- ============================================================
-- TABLA: hospedaje.estadia
--
-- Denormalizamos reserva_guid (no estaba en el monolito) para
-- evitar viajes constantes a HotelLux_Reservation via gRPC.
-- Tambien guardamos sucursal_guid por la misma razon (consulta
-- frecuente "estadias por sucursal").
-- ============================================================
CREATE TABLE hospedaje.estadia (
    id_estadia                 INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    estadia_guid               UUID         NOT NULL DEFAULT gen_random_uuid(),
    reserva_habitacion_guid    UUID         NOT NULL,                       -- ref logica
    reserva_guid               UUID         NOT NULL,                       -- ref logica (denormalizada)
    cliente_guid               UUID         NOT NULL,                       -- ref logica
    sucursal_guid              UUID         NOT NULL,                       -- ref logica (denormalizada)
    habitacion_guid            UUID         NOT NULL,                       -- ref logica
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
    es_eliminado               BOOLEAN      NOT NULL DEFAULT FALSE,
    CONSTRAINT uq_estadia_guid           UNIQUE (estadia_guid),
    CONSTRAINT uq_estadia_reserva_hab    UNIQUE (reserva_habitacion_guid),
    -- ACT=Activa (check-in hecho, sin checkout) | FIN=Finalizada | CAN=Cancelada
    CONSTRAINT chk_estadia_estado        CHECK (estado_estadia IN ('ACT','FIN','CAN')),
    CONSTRAINT chk_estadia_fechas        CHECK (checkout_utc IS NULL OR checkin_utc IS NULL OR checkout_utc >= checkin_utc)
);


-- ============================================================
-- TABLA: hospedaje.cargo_estadia
-- Cargos generados durante la estadia (room service, spa, etc.).
-- Referencia a catalogo_servicios es GUID logico.
-- ============================================================
CREATE TABLE hospedaje.cargo_estadia (
    id_cargo_estadia           INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cargo_guid                 UUID         NOT NULL DEFAULT gen_random_uuid(),
    id_estadia                 INT          NOT NULL,                       -- FK local
    catalogo_guid              UUID         NULL,                           -- ref logica (NULL = cargo manual libre)
    descripcion_cargo          VARCHAR(250) NOT NULL,
    cantidad                   INT          NOT NULL DEFAULT 1,
    precio_unitario            NUMERIC(12,2) NOT NULL,
    subtotal                   NUMERIC(12,2) NOT NULL,
    valor_iva                  NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_cargo                NUMERIC(12,2) NOT NULL,
    fecha_consumo_utc          TIMESTAMPTZ  NOT NULL DEFAULT now(),
    -- PEN=Pendiente de facturar | FAC=Facturado | ANU=Anulado
    estado_cargo               CHAR(3)      NOT NULL DEFAULT 'PEN',
    fecha_registro_utc         TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario         VARCHAR(100) NOT NULL,
    modificado_por_usuario     VARCHAR(100) NULL,
    fecha_modificacion_utc     TIMESTAMPTZ  NULL,
    modificacion_ip            VARCHAR(45)  NULL,
    servicio_origen            VARCHAR(50)  NOT NULL DEFAULT 'stay-service',
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
-- Antes en su propio microservicio Rating. Ahora vive en Stay
-- porque toda valoracion REQUIERE una estadia finalizada local.
-- ============================================================
CREATE TABLE hospedaje.valoracion (
    id_valoracion              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    valoracion_guid            UUID         NOT NULL DEFAULT gen_random_uuid(),
    id_estadia                 INT          NOT NULL,                        -- FK local
    estadia_guid               UUID         NOT NULL,                        -- ref denormalizada (la usa EF)
    cliente_guid               UUID         NOT NULL,                        -- ref logica
    sucursal_guid              UUID         NOT NULL,                        -- ref logica
    habitacion_guid            UUID         NULL,                            -- ref logica
    -- Puntuacion general escala 0-10 (alineado contrato API)
    puntuacion_general         NUMERIC(3,1) NOT NULL,
    -- Subpuntuaciones requeridas por el objeto Rating del contrato
    puntuacion_limpieza        NUMERIC(3,1) NULL,
    puntuacion_confort         NUMERIC(3,1) NULL,
    puntuacion_ubicacion       NUMERIC(3,1) NULL,
    puntuacion_instalaciones   NUMERIC(3,1) NULL,
    puntuacion_personal        NUMERIC(3,1) NULL,
    puntuacion_calidad_precio  NUMERIC(3,1) NULL,
    -- Comentario separado en positivo/negativo segun contrato
    comentario_positivo        TEXT         NULL,
    comentario_negativo        TEXT         NULL,
    tipo_viaje                 VARCHAR(20)  NULL,
    estado_valoracion          CHAR(3)      NOT NULL DEFAULT 'PEN',
    publicada_en_portal        BOOLEAN      NOT NULL DEFAULT FALSE,
    respuesta_hotel            TEXT         NULL,
    fecha_respuesta_utc        TIMESTAMPTZ  NULL,
    moderada_por_usuario       VARCHAR(100) NULL,
    motivo_moderacion          VARCHAR(250) NULL,
    fecha_registro_utc         TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario         VARCHAR(100) NOT NULL,
    modificado_por_usuario     VARCHAR(100) NULL,
    fecha_modificacion_utc     TIMESTAMPTZ  NULL,
    modificacion_ip            VARCHAR(45)  NULL,
    servicio_origen            VARCHAR(50)  NOT NULL DEFAULT 'stay-service',
    es_eliminado               BOOLEAN      NOT NULL DEFAULT FALSE,
    nombre_visible_cliente     VARCHAR(150) NULL,
    fecha_publicacion_utc      TIMESTAMPTZ  NOT NULL DEFAULT now(),
    CONSTRAINT uq_valoracion_guid          UNIQUE (valoracion_guid),
    CONSTRAINT uq_valoracion_estadia_clte  UNIQUE (id_estadia, cliente_guid),
    CONSTRAINT fk_valoracion_estadia       FOREIGN KEY (id_estadia)
        REFERENCES hospedaje.estadia(id_estadia),
    CONSTRAINT chk_val_puntuacion       CHECK (puntuacion_general BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_limp        CHECK (puntuacion_limpieza        IS NULL OR puntuacion_limpieza        BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_conf        CHECK (puntuacion_confort         IS NULL OR puntuacion_confort         BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_ubic        CHECK (puntuacion_ubicacion       IS NULL OR puntuacion_ubicacion       BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_inst        CHECK (puntuacion_instalaciones   IS NULL OR puntuacion_instalaciones   BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_pers        CHECK (puntuacion_personal        IS NULL OR puntuacion_personal        BETWEEN 0 AND 10),
    CONSTRAINT chk_val_punt_calp        CHECK (puntuacion_calidad_precio  IS NULL OR puntuacion_calidad_precio  BETWEEN 0 AND 10),
    CONSTRAINT chk_val_tipo_viaje       CHECK (tipo_viaje IS NULL OR tipo_viaje IN ('pareja','familia','negocios','amigos','solo')),
    -- PEN=Pendiente | PUB=Publicada | OCU=Oculta | REP=Reportada
    CONSTRAINT chk_val_estado           CHECK (estado_valoracion IN ('PEN','PUB','OCU','REP'))
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
-- DATOS SEMILLA
--
-- ESTADIAS (aaxxxxxx-...):
--   aaa...002 = ACT (en curso) -- corresponde a reserva 99...002 (Ana Lopez)
--   aaa...003 = FIN (completa) -- corresponde a reserva 99...003 (Pedro Garcia)
-- (reserva 99...001 esta en CON pero AUN sin check-in => no tiene estadia)
-- ============================================================

-- Estadia 1: Ana Lopez en Guayaquil, check-in hoy, sin checkout
INSERT INTO hospedaje.estadia (
    estadia_guid,
    reserva_habitacion_guid, reserva_guid,
    cliente_guid, sucursal_guid, habitacion_guid,
    checkin_utc, checkout_utc, estado_estadia,
    observaciones_checkin, creado_por_usuario
) VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002',
 'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a102',                  -- linea reserva 002
 '99999999-9999-9999-9999-999999999002',                  -- reserva 002
 '33333333-3333-3333-3333-333333333002',                  -- Ana Lopez
 '44444444-4444-4444-4444-444444444002',                  -- sucursal GYE
 '66666666-6666-6666-6666-666666666007',                  -- hab familiar GYE 201
 '2026-05-09 15:30:00+00', NULL, 'ACT',
 'Check-in normal. Documento verificado. Sin equipaje extra.',
 'vendedor1');

-- Estadia 2: Pedro Garcia en Cuenca, check-in y checkout completos
INSERT INTO hospedaje.estadia (
    estadia_guid,
    reserva_habitacion_guid, reserva_guid,
    cliente_guid, sucursal_guid, habitacion_guid,
    checkin_utc, checkout_utc, estado_estadia,
    observaciones_checkin, observaciones_checkout, creado_por_usuario
) VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003',
 'a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a103',                  -- linea reserva 003
 '99999999-9999-9999-9999-999999999003',                  -- reserva 003
 '33333333-3333-3333-3333-333333333003',                  -- Pedro Garcia
 '44444444-4444-4444-4444-444444444003',                  -- sucursal Cuenca
 '66666666-6666-6666-6666-666666666008',                  -- hab single Cuenca 101
 '2026-04-20 14:35:00+00', '2026-04-23 11:45:00+00', 'FIN',
 'Check-in walk-in. Pago efectivo.',
 'Checkout sin novedad. Habitacion en buen estado.',
 'vendedor1');


-- ============================================================
-- CARGOS DE ESTADIA
-- Para la estadia EN CURSO de Ana Lopez: 2 cargos pendientes.
-- Para la estadia FINALIZADA de Pedro: 1 cargo ya facturado.
-- ============================================================

-- Cargos para Ana Lopez (estadia ACT)
INSERT INTO hospedaje.cargo_estadia (
    cargo_guid, id_estadia, catalogo_guid,
    descripcion_cargo, cantidad, precio_unitario,
    subtotal, valor_iva, total_cargo,
    fecha_consumo_utc, estado_cargo, creado_por_usuario
) VALUES
('cccccccc-cccc-cccc-cccc-cccccccc1001',
 (SELECT id_estadia FROM hospedaje.estadia WHERE estadia_guid='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002'),
 '88888888-8888-8888-8888-888888888005',                  -- SRV-RS (Room Service)
 'Servicio a la habitacion - cena familiar', 1, 15.00,
 15.00, 2.25, 17.25,
 '2026-05-09 20:00:00+00', 'PEN', 'vendedor1'),

('cccccccc-cccc-cccc-cccc-cccccccc1002',
 (SELECT id_estadia FROM hospedaje.estadia WHERE estadia_guid='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002'),
 '88888888-8888-8888-8888-888888888006',                  -- SRV-LAV (Lavanderia)
 'Lavanderia - 2 mudas de ropa', 1, 12.50,
 12.50, 1.88, 14.38,
 '2026-05-10 09:30:00+00', 'PEN', 'vendedor1');

-- Cargo para Pedro Garcia (estadia FIN, ya facturado)
INSERT INTO hospedaje.cargo_estadia (
    cargo_guid, id_estadia, catalogo_guid,
    descripcion_cargo, cantidad, precio_unitario,
    subtotal, valor_iva, total_cargo,
    fecha_consumo_utc, estado_cargo, creado_por_usuario
) VALUES
('cccccccc-cccc-cccc-cccc-cccccccc1003',
 (SELECT id_estadia FROM hospedaje.estadia WHERE estadia_guid='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003'),
 '88888888-8888-8888-8888-888888888007',                  -- SRV-SPA
 'Spa - masaje relajante 60 min', 1, 45.00,
 45.00, 6.75, 51.75,
 '2026-04-22 16:00:00+00', 'FAC', 'vendedor1');


-- ============================================================
-- VALORACIONES (ddxxxxxx-...)
-- Solo Pedro Garcia (estadia FIN) tiene valoracion publicada.
-- ============================================================

INSERT INTO hospedaje.valoracion (
    valoracion_guid, id_estadia,
    cliente_guid, sucursal_guid, habitacion_guid,
    puntuacion_general,
    puntuacion_limpieza, puntuacion_confort, puntuacion_ubicacion,
    puntuacion_instalaciones, puntuacion_personal, puntuacion_calidad_precio,
    comentario_positivo, comentario_negativo, tipo_viaje,
    estado_valoracion, publicada_en_portal,
    respuesta_hotel, fecha_respuesta_utc, moderada_por_usuario,
    creado_por_usuario
) VALUES
('dddddddd-dddd-dddd-dddd-dddddddd1003',
 (SELECT id_estadia FROM hospedaje.estadia WHERE estadia_guid='aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003'),
 '33333333-3333-3333-3333-333333333003',                  -- Pedro Garcia
 '44444444-4444-4444-4444-444444444003',                  -- sucursal Cuenca
 '66666666-6666-6666-6666-666666666008',                  -- hab Cuenca 101
 9.0,
 9.5, 9.0, 10.0, 8.5, 9.5, 8.5,
 'Ubicacion excelente en pleno centro historico. Personal muy atento, habitacion impecable y desayuno delicioso.',
 'El WiFi en la habitacion fue un poco lento en las noches.',
 'solo',
 'PUB', TRUE,
 'Gracias por su visita Sr. Garcia. Ya estamos mejorando la conectividad WiFi. Lo esperamos pronto.',
 '2026-04-25 10:00:00+00', 'gerente1',
 'pedro.garcia');


-- ============================================================
-- VERIFICACION FINAL
-- ============================================================
SELECT 'Estadias:           ' || COUNT(*)::text FROM hospedaje.estadia;
SELECT 'Cargos de estadia:  ' || COUNT(*)::text FROM hospedaje.cargo_estadia;
SELECT 'Valoraciones:       ' || COUNT(*)::text FROM hospedaje.valoracion;

-- Resumen de estadias con cargos
SELECT
    e.estadia_guid,
    e.cliente_guid,
    e.estado_estadia,
    e.checkin_utc::date  AS checkin,
    e.checkout_utc::date AS checkout,
    COUNT(c.id_cargo_estadia) AS total_cargos,
    COALESCE(SUM(c.total_cargo), 0) AS suma_cargos
FROM   hospedaje.estadia e
LEFT   JOIN hospedaje.cargo_estadia c ON c.id_estadia = e.id_estadia
GROUP  BY e.id_estadia
ORDER  BY e.checkin_utc;
