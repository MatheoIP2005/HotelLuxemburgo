-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio ACCOMMODATION
-- Base de datos: HotelLux_Accommodation
-- Motor: PostgreSQL 18
-- Version: 1.0
--
-- DEPENDENCIAS: ninguna (NO necesita ninguna otra BD para ejecutarse)
--
-- CONTENIDO:
--   Schema: alojamiento
--   Tablas: sucursal, sucursal_imagen, tipo_habitacion,
--           tipo_habitacion_imagen, catalogo_servicios,
--           tipo_habitacion_catalogo, habitacion, tarifa
--   Datos semilla: 3 sucursales, 4 tipos, 8 servicios,
--                  10 habitaciones, 8 tarifas, varias imagenes.
--
-- INSTRUCCIONES DE EJECUCION EN pgAdmin:
--   1. Click derecho sobre 'Databases' -> Create -> Database...
--      Database: HotelLux_Accommodation
--      Owner: postgres / Encoding: UTF8
--   2. Click derecho sobre HotelLux_Accommodation -> Query Tool
--   3. File -> Open este archivo -> F5
--   4. Verificar al final los SELECT de conteo.
-- ============================================================


-- ============================================================
-- SCHEMA
-- ============================================================
CREATE SCHEMA IF NOT EXISTS alojamiento;


-- ============================================================
-- TABLA: alojamiento.sucursal
-- ============================================================
CREATE TABLE alojamiento.sucursal (
    id_sucursal              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    sucursal_guid            UUID         NOT NULL DEFAULT gen_random_uuid(),
    codigo_sucursal          VARCHAR(30)  NOT NULL,
    nombre_sucursal          VARCHAR(150) NOT NULL,
    descripcion_sucursal     TEXT         NULL,
    descripcion_corta        VARCHAR(250) NULL,
    tipo_alojamiento         VARCHAR(20)  NOT NULL DEFAULT 'hotel',
    estrellas                INT          NULL,
    categoria_viaje          VARCHAR(30)  NULL,
    pais                     VARCHAR(100) NOT NULL,
    provincia                VARCHAR(100) NULL,
    ciudad                   VARCHAR(100) NOT NULL,
    ubicacion                VARCHAR(200) NOT NULL,
    direccion                VARCHAR(250) NOT NULL,
    codigo_postal            VARCHAR(20)  NULL,
    telefono                 VARCHAR(30)  NOT NULL,
    correo                   VARCHAR(150) NOT NULL,
    latitud                  NUMERIC(10,7) NULL,
    longitud                 NUMERIC(10,7) NULL,
    hora_checkin             VARCHAR(5)   NULL DEFAULT '15:00',
    hora_checkout            VARCHAR(5)   NULL DEFAULT '12:00',
    checkin_anticipado       BOOLEAN      NOT NULL DEFAULT FALSE,
    checkout_tardio          BOOLEAN      NOT NULL DEFAULT FALSE,
    acepta_ninos             BOOLEAN      NOT NULL DEFAULT TRUE,
    edad_minima_huesped      INT          NULL,
    permite_mascotas         BOOLEAN      NOT NULL DEFAULT FALSE,
    se_permite_fumar         BOOLEAN      NOT NULL DEFAULT FALSE,
    estado_sucursal          CHAR(3)      NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    motivo_inhabilitacion    VARCHAR(250) NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100) NOT NULL,
    modificado_por_usuario   VARCHAR(100) NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          VARCHAR(45)  NULL,
    servicio_origen          VARCHAR(50)  NOT NULL DEFAULT 'accommodation-service',
    CONSTRAINT uq_sucursal_guid    UNIQUE (sucursal_guid),
    CONSTRAINT uq_sucursal_codigo  UNIQUE (codigo_sucursal),
    CONSTRAINT uq_sucursal_nombre  UNIQUE (nombre_sucursal),
    CONSTRAINT chk_sucursal_estado CHECK (estado_sucursal IN ('ACT','INA')),
    CONSTRAINT chk_sucursal_tipo_alojamiento CHECK
        (tipo_alojamiento IN ('hotel','hostal','apartamento','resort','villa','cabana','hostel')),
    CONSTRAINT chk_sucursal_estrellas CHECK (estrellas IS NULL OR estrellas BETWEEN 1 AND 5),
    CONSTRAINT chk_sucursal_categoria CHECK
        (categoria_viaje IS NULL OR
         categoria_viaje IN ('playa','ciudad','montana','aventura','cultural','bienestar')),
    CONSTRAINT chk_sucursal_edad_min CHECK
        (edad_minima_huesped IS NULL OR edad_minima_huesped >= 0)
);


-- ============================================================
-- TABLA: alojamiento.sucursal_imagen
-- Nueva tabla: el endpoint GET /sucursales/{guid}/imagenes la requiere
-- y no existia en el modelo original.
-- ============================================================
CREATE TABLE alojamiento.sucursal_imagen (
    id_sucursal_imagen       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    sucursal_imagen_guid     UUID         NOT NULL DEFAULT gen_random_uuid(),
    id_sucursal              INT          NOT NULL,
    url_imagen               VARCHAR(500) NOT NULL,
    descripcion_imagen       VARCHAR(255) NULL,
    orden_visualizacion      INT          NOT NULL DEFAULT 1,
    es_principal             BOOLEAN      NOT NULL DEFAULT FALSE,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100) NOT NULL,
    CONSTRAINT uq_sucursal_imagen_guid UNIQUE (sucursal_imagen_guid),
    CONSTRAINT fk_sucursal_imagen_sucursal FOREIGN KEY (id_sucursal)
        REFERENCES alojamiento.sucursal(id_sucursal) ON DELETE CASCADE,
    CONSTRAINT chk_sucursal_imagen_orden CHECK (orden_visualizacion > 0)
);


-- ============================================================
-- TABLA: alojamiento.tipo_habitacion
-- ============================================================
CREATE TABLE alojamiento.tipo_habitacion (
    id_tipo_habitacion       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tipo_habitacion_guid     UUID         NOT NULL DEFAULT gen_random_uuid(),
    codigo_tipo_habitacion   VARCHAR(30)  NOT NULL,
    nombre_tipo_habitacion   VARCHAR(120) NOT NULL,
    descripcion              TEXT         NULL,
    capacidad_adultos        INT          NOT NULL,
    capacidad_ninos          INT          NOT NULL DEFAULT 0,
    capacidad_total          INT          NOT NULL,
    tipo_cama                VARCHAR(60)  NULL,
    area_m2                  NUMERIC(6,2) NULL,
    permite_eventos          BOOLEAN      NOT NULL DEFAULT FALSE,
    permite_reserva_publica  BOOLEAN      NOT NULL DEFAULT TRUE,
    estado_tipo_habitacion   CHAR(3)      NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    motivo_inhabilitacion    VARCHAR(250) NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100) NOT NULL,
    modificado_por_usuario   VARCHAR(100) NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          VARCHAR(45)  NULL,
    servicio_origen          VARCHAR(50)  NOT NULL DEFAULT 'accommodation-service',
    CONSTRAINT uq_tipo_habitacion_guid    UNIQUE (tipo_habitacion_guid),
    CONSTRAINT uq_tipo_habitacion_codigo  UNIQUE (codigo_tipo_habitacion),
    CONSTRAINT uq_tipo_habitacion_nombre  UNIQUE (nombre_tipo_habitacion),
    CONSTRAINT chk_tipo_habitacion_estado CHECK (estado_tipo_habitacion IN ('ACT','INA')),
    CONSTRAINT chk_tipo_habitacion_adultos CHECK (capacidad_adultos > 0),
    CONSTRAINT chk_tipo_habitacion_ninos   CHECK (capacidad_ninos >= 0),
    CONSTRAINT chk_tipo_habitacion_total   CHECK (capacidad_total > 0),
    CONSTRAINT chk_tipo_habitacion_area    CHECK (area_m2 IS NULL OR area_m2 > 0)
);


-- ============================================================
-- TABLA: alojamiento.tipo_habitacion_imagen
-- ============================================================
CREATE TABLE alojamiento.tipo_habitacion_imagen (
    id_tipo_habitacion_imagen INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tipo_hab_imagen_guid      UUID         NOT NULL DEFAULT gen_random_uuid(),
    id_tipo_habitacion        INT          NOT NULL,
    url_imagen                VARCHAR(500) NOT NULL,
    descripcion_imagen        VARCHAR(255) NULL,
    orden_visualizacion       INT          NOT NULL DEFAULT 1,
    es_principal              BOOLEAN      NOT NULL DEFAULT FALSE,
    fecha_registro_utc        TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario        VARCHAR(100) NOT NULL,
    CONSTRAINT uq_tipo_hab_imagen_guid UNIQUE (tipo_hab_imagen_guid),
    CONSTRAINT fk_tipo_hab_imagen_tipo FOREIGN KEY (id_tipo_habitacion)
        REFERENCES alojamiento.tipo_habitacion(id_tipo_habitacion) ON DELETE CASCADE,
    CONSTRAINT chk_tipo_hab_imagen_orden CHECK (orden_visualizacion > 0)
);


-- ============================================================
-- TABLA: alojamiento.catalogo_servicios
-- Unifica amenidades (AME) y servicios adicionales (SRV).
-- ============================================================
CREATE TABLE alojamiento.catalogo_servicios (
    id_catalogo              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    catalogo_guid            UUID         NOT NULL DEFAULT gen_random_uuid(),
    id_sucursal              INT          NULL,             -- NULL = aplica a todas las sucursales
    codigo_catalogo          VARCHAR(30)  NOT NULL,
    nombre_catalogo          VARCHAR(120) NOT NULL,
    tipo_catalogo            CHAR(3)      NOT NULL,         -- AME | SRV
    categoria_catalogo       VARCHAR(80)  NOT NULL,
    descripcion_catalogo     VARCHAR(250) NULL,
    precio_base              NUMERIC(12,2) NOT NULL DEFAULT 0,
    aplica_iva               BOOLEAN      NOT NULL DEFAULT FALSE,
    disponible_24h           BOOLEAN      NOT NULL DEFAULT FALSE,
    hora_inicio              TIME         NULL,
    hora_fin                 TIME         NULL,
    icono_url                VARCHAR(500) NULL,
    estado_catalogo          CHAR(3)      NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    motivo_inhabilitacion    VARCHAR(250) NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100) NOT NULL,
    modificado_por_usuario   VARCHAR(100) NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          VARCHAR(45)  NULL,
    servicio_origen          VARCHAR(50)  NOT NULL DEFAULT 'accommodation-service',
    CONSTRAINT uq_catalogo_guid   UNIQUE (catalogo_guid),
    CONSTRAINT uq_catalogo_codigo UNIQUE (codigo_catalogo),
    CONSTRAINT fk_catalogo_sucursal FOREIGN KEY (id_sucursal)
        REFERENCES alojamiento.sucursal(id_sucursal),
    CONSTRAINT chk_catalogo_tipo   CHECK (tipo_catalogo IN ('AME','SRV')),
    CONSTRAINT chk_catalogo_estado CHECK (estado_catalogo IN ('ACT','INA')),
    CONSTRAINT chk_catalogo_precio CHECK (
        (tipo_catalogo = 'AME' AND precio_base = 0) OR
        (tipo_catalogo = 'SRV' AND precio_base >= 0)
    )
);


-- ============================================================
-- TABLA: alojamiento.tipo_habitacion_catalogo (N:M)
-- Que amenidades incluye cada tipo de habitacion
-- ============================================================
CREATE TABLE alojamiento.tipo_habitacion_catalogo (
    id_tipo_hab_catalogo     INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_tipo_habitacion       INT          NOT NULL,
    id_catalogo              INT          NOT NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100) NOT NULL,
    CONSTRAINT uq_tipo_hab_catalogo UNIQUE (id_tipo_habitacion, id_catalogo),
    CONSTRAINT fk_tipo_hab_cat_tipo FOREIGN KEY (id_tipo_habitacion)
        REFERENCES alojamiento.tipo_habitacion(id_tipo_habitacion) ON DELETE CASCADE,
    CONSTRAINT fk_tipo_hab_cat_cat FOREIGN KEY (id_catalogo)
        REFERENCES alojamiento.catalogo_servicios(id_catalogo)
);


-- ============================================================
-- TABLA: alojamiento.habitacion
-- ============================================================
CREATE TABLE alojamiento.habitacion (
    id_habitacion            INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    habitacion_guid          UUID         NOT NULL DEFAULT gen_random_uuid(),
    id_sucursal              INT          NOT NULL,
    id_tipo_habitacion       INT          NOT NULL,
    numero_habitacion        VARCHAR(20)  NOT NULL,
    piso                     INT          NULL,
    capacidad_habitacion     INT          NOT NULL,
    precio_base              NUMERIC(12,2) NOT NULL,
    descripcion_habitacion   VARCHAR(250) NULL,
    estado_habitacion        CHAR(3)      NOT NULL DEFAULT 'DIS',
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    motivo_inhabilitacion    VARCHAR(250) NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100) NOT NULL,
    modificado_por_usuario   VARCHAR(100) NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          VARCHAR(45)  NULL,
    servicio_origen          VARCHAR(50)  NOT NULL DEFAULT 'accommodation-service',
    CONSTRAINT uq_habitacion_guid           UNIQUE (habitacion_guid),
    CONSTRAINT uq_habitacion_sucursal_numero UNIQUE (id_sucursal, numero_habitacion),
    CONSTRAINT fk_habitacion_sucursal FOREIGN KEY (id_sucursal)
        REFERENCES alojamiento.sucursal(id_sucursal),
    CONSTRAINT fk_habitacion_tipo FOREIGN KEY (id_tipo_habitacion)
        REFERENCES alojamiento.tipo_habitacion(id_tipo_habitacion),
    -- DIS=Disponible | OCU=Ocupada | MNT=Mantenimiento | FDS=Fuera de servicio | INA=Inactiva
    CONSTRAINT chk_habitacion_estado    CHECK (estado_habitacion IN ('DIS','OCU','MNT','FDS','INA')),
    CONSTRAINT chk_habitacion_capacidad CHECK (capacidad_habitacion > 0),
    CONSTRAINT chk_habitacion_precio    CHECK (precio_base > 0)
);


-- ============================================================
-- TABLA: alojamiento.tarifa
-- ============================================================
CREATE TABLE alojamiento.tarifa (
    id_tarifa                INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tarifa_guid              UUID         NOT NULL DEFAULT gen_random_uuid(),
    codigo_tarifa            VARCHAR(30)  NOT NULL,
    id_sucursal              INT          NOT NULL,
    id_tipo_habitacion       INT          NOT NULL,
    nombre_tarifa            VARCHAR(150) NOT NULL,
    canal_tarifa             VARCHAR(30)  NOT NULL DEFAULT 'TODOS',
    fecha_inicio             DATE         NOT NULL,
    fecha_fin                DATE         NOT NULL,
    precio_por_noche         NUMERIC(12,2) NOT NULL,
    porcentaje_iva           NUMERIC(5,2) NOT NULL DEFAULT 15.00,
    min_noches               INT          NOT NULL DEFAULT 1,
    max_noches               INT          NULL,
    permite_portal_publico   BOOLEAN      NOT NULL DEFAULT TRUE,
    prioridad                INT          NOT NULL DEFAULT 1,
    estado_tarifa            CHAR(3)      NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    motivo_inhabilitacion    VARCHAR(250) NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100) NOT NULL,
    modificado_por_usuario   VARCHAR(100) NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          VARCHAR(45)  NULL,
    servicio_origen          VARCHAR(50)  NOT NULL DEFAULT 'accommodation-service',
    CONSTRAINT uq_tarifa_guid   UNIQUE (tarifa_guid),
    CONSTRAINT uq_tarifa_codigo UNIQUE (codigo_tarifa),
    CONSTRAINT fk_tarifa_sucursal FOREIGN KEY (id_sucursal)
        REFERENCES alojamiento.sucursal(id_sucursal),
    CONSTRAINT fk_tarifa_tipo FOREIGN KEY (id_tipo_habitacion)
        REFERENCES alojamiento.tipo_habitacion(id_tipo_habitacion),
    CONSTRAINT chk_tarifa_canal      CHECK (canal_tarifa IN ('TODOS','PORTAL','ADMIN','API','WALKIN')),
    CONSTRAINT chk_tarifa_estado     CHECK (estado_tarifa IN ('ACT','INA')),
    CONSTRAINT chk_tarifa_fechas     CHECK (fecha_fin >= fecha_inicio),
    CONSTRAINT chk_tarifa_precio     CHECK (precio_por_noche > 0),
    CONSTRAINT chk_tarifa_iva        CHECK (porcentaje_iva >= 0),
    CONSTRAINT chk_tarifa_min_noches CHECK (min_noches > 0),
    CONSTRAINT chk_tarifa_max_noches CHECK (max_noches IS NULL OR max_noches >= min_noches),
    CONSTRAINT chk_tarifa_prioridad  CHECK (prioridad > 0)
);


-- ============================================================
-- INDICES DE APOYO
-- ============================================================
CREATE INDEX ix_habitacion_sucursal_estado
    ON alojamiento.habitacion(id_sucursal, estado_habitacion, id_tipo_habitacion);

CREATE INDEX ix_tarifa_tipo_fechas
    ON alojamiento.tarifa(id_sucursal, id_tipo_habitacion, fecha_inicio, fecha_fin, estado_tarifa);

CREATE INDEX ix_catalogo_tipo_estado
    ON alojamiento.catalogo_servicios(tipo_catalogo, estado_catalogo, id_sucursal);

CREATE INDEX ix_sucursal_estado_ciudad
    ON alojamiento.sucursal(estado_sucursal, ciudad, tipo_alojamiento);


-- ============================================================
-- DATOS SEMILLA
--
-- SUCURSALES (44xxxxxx-...):
--   001 = Hotel Luxemburgo Quito
--   002 = Hotel Luxemburgo Guayaquil
--   003 = Hotel Luxemburgo Cuenca
-- ============================================================

INSERT INTO alojamiento.sucursal (
    sucursal_guid, codigo_sucursal, nombre_sucursal,
    descripcion_sucursal, descripcion_corta, tipo_alojamiento, estrellas, categoria_viaje,
    pais, provincia, ciudad, ubicacion, direccion, codigo_postal,
    telefono, correo, latitud, longitud,
    hora_checkin, hora_checkout,
    acepta_ninos, permite_mascotas, se_permite_fumar,
    creado_por_usuario
) VALUES
(
 '44444444-4444-4444-4444-444444444001',
 'LUX-UIO', 'Hotel Luxemburgo Quito',
 'Hotel boutique en el corazon de Quito con vista a la ciudad colonial y servicios de lujo.',
 'Hotel boutique 5 estrellas en Quito centro.',
 'hotel', 5, 'ciudad',
 'Ecuador', 'Pichincha', 'Quito',
 'Sector La Mariscal', 'Av. 6 de Diciembre N24-65 y Foch', '170135',
 '+593 2 222 1100', 'quito@hotelluxemburgo.com',
 -0.2058543, -78.4929210,
 '15:00', '12:00',
 TRUE, FALSE, FALSE,
 'system'
),
(
 '44444444-4444-4444-4444-444444444002',
 'LUX-GYE', 'Hotel Luxemburgo Guayaquil',
 'Hotel premium frente al Malecon 2000 con vista al rio Guayas y experiencia tropical.',
 'Hotel premium en Guayaquil con vista al Malecon 2000.',
 'hotel', 5, 'ciudad',
 'Ecuador', 'Guayas', 'Guayaquil',
 'Malecon 2000', 'Malecon Simon Bolivar 1100 y Sucre', '090313',
 '+593 4 256 7800', 'guayaquil@hotelluxemburgo.com',
 -2.1894128, -79.8807552,
 '15:00', '12:00',
 TRUE, TRUE, FALSE,
 'system'
),
(
 '44444444-4444-4444-4444-444444444003',
 'LUX-CUE', 'Hotel Luxemburgo Cuenca',
 'Hotel colonial en el centro historico de Cuenca, declarado Patrimonio de la Humanidad.',
 'Hotel colonial en Cuenca patrimonial.',
 'hotel', 4, 'cultural',
 'Ecuador', 'Azuay', 'Cuenca',
 'Centro Historico', 'Calle Larga 7-25 y Borrero', '010103',
 '+593 7 283 4500', 'cuenca@hotelluxemburgo.com',
 -2.8974217, -79.0058736,
 '14:00', '12:00',
 TRUE, FALSE, FALSE,
 'system'
);


-- Imagenes de sucursales (1 por sucursal)
INSERT INTO alojamiento.sucursal_imagen (id_sucursal, url_imagen, descripcion_imagen, es_principal, orden_visualizacion, creado_por_usuario)
SELECT s.id_sucursal,
       'https://cdn.hotelluxemburgo.com/sucursales/' || lower(s.codigo_sucursal) || '/principal.jpg',
       'Vista principal del ' || s.nombre_sucursal,
       TRUE, 1, 'system'
FROM alojamiento.sucursal s;


-- ============================================================
-- TIPOS DE HABITACION (55xxxxxx-...):
--   001 = Suite Single
--   002 = Suite Doble
--   003 = Suite Familiar
--   004 = Suite Premium
-- ============================================================
INSERT INTO alojamiento.tipo_habitacion (
    tipo_habitacion_guid, codigo_tipo_habitacion, nombre_tipo_habitacion,
    descripcion, capacidad_adultos, capacidad_ninos, capacidad_total,
    tipo_cama, area_m2, permite_eventos, permite_reserva_publica,
    creado_por_usuario
) VALUES
('55555555-5555-5555-5555-555555555001', 'TH-SINGLE', 'Suite Single',
 'Habitacion individual con cama king y bano privado. Ideal para viajeros de negocios.',
 1, 0, 1, 'King size', 22.00, FALSE, TRUE, 'system'),

('55555555-5555-5555-5555-555555555002', 'TH-DOBLE', 'Suite Doble',
 'Habitacion doble con dos camas queen, escritorio y sala de estar. Ideal para parejas.',
 2, 0, 2, '2 Queen size', 32.00, FALSE, TRUE, 'system'),

('55555555-5555-5555-5555-555555555003', 'TH-FAMILIAR', 'Suite Familiar',
 'Suite amplia con dos ambientes, cama king y sofa-cama doble. Ideal para familias.',
 2, 2, 4, '1 King + 1 Sofa-cama doble', 48.00, FALSE, TRUE, 'system'),

('55555555-5555-5555-5555-555555555004', 'TH-PREMIUM', 'Suite Premium',
 'Suite de lujo en piso alto con vista panoramica, jacuzzi privado y servicio de mayordomo.',
 2, 0, 2, '1 King size de lujo', 65.00, TRUE, TRUE, 'system');


-- Imagenes de tipos de habitacion (2 por tipo)
INSERT INTO alojamiento.tipo_habitacion_imagen (id_tipo_habitacion, url_imagen, descripcion_imagen, es_principal, orden_visualizacion, creado_por_usuario)
SELECT th.id_tipo_habitacion,
       'https://cdn.hotelluxemburgo.com/habitaciones/' || lower(th.codigo_tipo_habitacion) || '/01.jpg',
       'Vista principal de la ' || th.nombre_tipo_habitacion,
       TRUE, 1, 'system'
FROM alojamiento.tipo_habitacion th;

INSERT INTO alojamiento.tipo_habitacion_imagen (id_tipo_habitacion, url_imagen, descripcion_imagen, es_principal, orden_visualizacion, creado_por_usuario)
SELECT th.id_tipo_habitacion,
       'https://cdn.hotelluxemburgo.com/habitaciones/' || lower(th.codigo_tipo_habitacion) || '/02.jpg',
       'Vista secundaria de la ' || th.nombre_tipo_habitacion,
       FALSE, 2, 'system'
FROM alojamiento.tipo_habitacion th;


-- ============================================================
-- CATALOGO DE SERVICIOS (88xxxxxx-...):
--   001-004 = Amenidades (AME, precio 0)
--   005-008 = Servicios adicionales (SRV, con precio)
-- ============================================================
INSERT INTO alojamiento.catalogo_servicios (
    catalogo_guid, id_sucursal, codigo_catalogo, nombre_catalogo,
    tipo_catalogo, categoria_catalogo, descripcion_catalogo,
    precio_base, aplica_iva, disponible_24h, icono_url,
    creado_por_usuario
) VALUES
-- Amenidades (precio = 0)
('88888888-8888-8888-8888-888888888001', NULL, 'AME-WIFI',   'WiFi de alta velocidad',
 'AME', 'Conectividad', 'WiFi gratuito en toda la habitacion y areas comunes',
 0, FALSE, TRUE, 'wifi.svg', 'system'),

('88888888-8888-8888-8888-888888888002', NULL, 'AME-AC',     'Aire Acondicionado',
 'AME', 'Confort', 'Aire acondicionado con control de temperatura individual',
 0, FALSE, TRUE, 'ac.svg', 'system'),

('88888888-8888-8888-8888-888888888003', NULL, 'AME-TV',     'TV Cable HD',
 'AME', 'Entretenimiento', 'TV LED 43 pulgadas con cable HD y servicios de streaming',
 0, FALSE, TRUE, 'tv.svg', 'system'),

('88888888-8888-8888-8888-888888888004', NULL, 'AME-MINIBAR','Mini Bar',
 'AME', 'Alimentos', 'Mini bar con bebidas y snacks de cortesia (al ingreso)',
 0, FALSE, TRUE, 'minibar.svg', 'system'),

-- Servicios adicionales (con precio)
('88888888-8888-8888-8888-888888888005', NULL, 'SRV-RS',     'Servicio a la habitacion',
 'SRV', 'Alimentos', 'Servicio de comida a la habitacion 24 horas',
 15.00, TRUE, TRUE, 'roomservice.svg', 'system'),

('88888888-8888-8888-8888-888888888006', NULL, 'SRV-LAV',    'Lavanderia',
 'SRV', 'Limpieza', 'Servicio de lavanderia con entrega en 24 horas',
 12.50, TRUE, FALSE, 'laundry.svg', 'system'),

('88888888-8888-8888-8888-888888888007', NULL, 'SRV-SPA',    'Spa & Masajes',
 'SRV', 'Bienestar', 'Servicio de spa con masajes relajantes y tratamientos faciales',
 45.00, TRUE, FALSE, 'spa.svg', 'system'),

('88888888-8888-8888-8888-888888888008', NULL, 'SRV-TRANS',  'Transporte al aeropuerto',
 'SRV', 'Transporte', 'Traslado privado al aeropuerto local en vehiculo ejecutivo',
 25.00, TRUE, TRUE, 'transport.svg', 'system');


-- Relacion N:M tipo_habitacion <-> catalogo (amenidades incluidas en cada tipo)
INSERT INTO alojamiento.tipo_habitacion_catalogo (id_tipo_habitacion, id_catalogo, creado_por_usuario)
SELECT th.id_tipo_habitacion, c.id_catalogo, 'system'
FROM   alojamiento.tipo_habitacion th
CROSS  JOIN alojamiento.catalogo_servicios c
WHERE  c.tipo_catalogo = 'AME'
   -- Todas las habitaciones tienen WiFi, AC y TV
   AND c.codigo_catalogo IN ('AME-WIFI','AME-AC','AME-TV')

UNION ALL

-- Solo Suite Doble, Familiar y Premium tienen Mini Bar
SELECT th.id_tipo_habitacion, c.id_catalogo, 'system'
FROM   alojamiento.tipo_habitacion th
CROSS  JOIN alojamiento.catalogo_servicios c
WHERE  c.codigo_catalogo = 'AME-MINIBAR'
   AND th.codigo_tipo_habitacion IN ('TH-DOBLE','TH-FAMILIAR','TH-PREMIUM');


-- ============================================================
-- HABITACIONES (66xxxxxx-...):
-- Distribuidas entre las 3 sucursales.
-- ============================================================
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, creado_por_usuario
) VALUES
-- Quito (id_sucursal = 1) -- 4 habitaciones
('66666666-6666-6666-6666-666666666001',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-UIO'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-SINGLE'),
 '101', 1, 1,  80.00, 'Habitacion 101 - Quito - Suite Single', 'system'),

('66666666-6666-6666-6666-666666666002',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-UIO'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-DOBLE'),
 '102', 1, 2, 120.00, 'Habitacion 102 - Quito - Suite Doble', 'system'),

('66666666-6666-6666-6666-666666666003',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-UIO'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-FAMILIAR'),
 '201', 2, 4, 180.00, 'Habitacion 201 - Quito - Suite Familiar', 'system'),

('66666666-6666-6666-6666-666666666004',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-UIO'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-PREMIUM'),
 '301', 3, 2, 250.00, 'Habitacion 301 - Quito - Suite Premium (vista a la ciudad)', 'system'),

-- Guayaquil -- 3 habitaciones
('66666666-6666-6666-6666-666666666005',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-GYE'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-DOBLE'),
 '101', 1, 2, 130.00, 'Habitacion 101 - GYE - Suite Doble vista al malecon', 'system'),

('66666666-6666-6666-6666-666666666006',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-GYE'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-DOBLE'),
 '102', 1, 2, 130.00, 'Habitacion 102 - GYE - Suite Doble vista al malecon', 'system'),

('66666666-6666-6666-6666-666666666007',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-GYE'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-FAMILIAR'),
 '201', 2, 4, 190.00, 'Habitacion 201 - GYE - Suite Familiar', 'system'),

-- Cuenca -- 3 habitaciones
('66666666-6666-6666-6666-666666666008',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-CUE'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-SINGLE'),
 '101', 1, 1,  75.00, 'Habitacion 101 - Cuenca - Suite Single', 'system'),

('66666666-6666-6666-6666-666666666009',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-CUE'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-DOBLE'),
 '102', 1, 2, 115.00, 'Habitacion 102 - Cuenca - Suite Doble', 'system'),

('66666666-6666-6666-6666-666666666010',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-CUE'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-PREMIUM'),
 '201', 2, 2, 230.00, 'Habitacion 201 - Cuenca - Suite Premium colonial', 'system');


-- ============================================================
-- TARIFAS (77xxxxxx-...):
-- Una tarifa activa por combinacion sucursal-tipo (vigentes 2026).
-- ============================================================
INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa, id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES
-- Quito
('77777777-7777-7777-7777-777777777001', 'TAR-UIO-SINGLE-2026',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-UIO'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-SINGLE'),
 'Tarifa Single Quito 2026', 'TODOS',
 '2026-01-01', '2026-12-31',
 80.00, 15.00, 1, TRUE, 1, 'system'),

('77777777-7777-7777-7777-777777777002', 'TAR-UIO-DOBLE-2026',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-UIO'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-DOBLE'),
 'Tarifa Doble Quito 2026', 'TODOS',
 '2026-01-01', '2026-12-31',
 120.00, 15.00, 1, TRUE, 1, 'system'),

('77777777-7777-7777-7777-777777777003', 'TAR-UIO-FAMILIAR-2026',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-UIO'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-FAMILIAR'),
 'Tarifa Familiar Quito 2026', 'TODOS',
 '2026-01-01', '2026-12-31',
 180.00, 15.00, 2, TRUE, 1, 'system'),

('77777777-7777-7777-7777-777777777004', 'TAR-UIO-PREMIUM-2026',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-UIO'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-PREMIUM'),
 'Tarifa Premium Quito 2026', 'TODOS',
 '2026-01-01', '2026-12-31',
 250.00, 15.00, 1, TRUE, 1, 'system'),

-- Guayaquil
('77777777-7777-7777-7777-777777777005', 'TAR-GYE-DOBLE-2026',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-GYE'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-DOBLE'),
 'Tarifa Doble GYE 2026', 'TODOS',
 '2026-01-01', '2026-12-31',
 130.00, 15.00, 1, TRUE, 1, 'system'),

('77777777-7777-7777-7777-777777777006', 'TAR-GYE-FAMILIAR-2026',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-GYE'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-FAMILIAR'),
 'Tarifa Familiar GYE 2026', 'TODOS',
 '2026-01-01', '2026-12-31',
 190.00, 15.00, 2, TRUE, 1, 'system'),

-- Cuenca
('77777777-7777-7777-7777-777777777007', 'TAR-CUE-SINGLE-2026',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-CUE'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-SINGLE'),
 'Tarifa Single Cuenca 2026', 'TODOS',
 '2026-01-01', '2026-12-31',
 75.00, 15.00, 1, TRUE, 1, 'system'),

('77777777-7777-7777-7777-777777777008', 'TAR-CUE-PREMIUM-2026',
 (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal='LUX-CUE'),
 (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion='TH-PREMIUM'),
 'Tarifa Premium Cuenca 2026', 'TODOS',
 '2026-01-01', '2026-12-31',
 230.00, 15.00, 1, TRUE, 1, 'system');


-- ============================================================
-- VERIFICACION FINAL
-- ============================================================
SELECT 'Sucursales:                ' || COUNT(*)::text FROM alojamiento.sucursal;
SELECT 'Imagenes sucursal:         ' || COUNT(*)::text FROM alojamiento.sucursal_imagen;
SELECT 'Tipos de habitacion:       ' || COUNT(*)::text FROM alojamiento.tipo_habitacion;
SELECT 'Imagenes tipo habitacion:  ' || COUNT(*)::text FROM alojamiento.tipo_habitacion_imagen;
SELECT 'Catalogo de servicios:     ' || COUNT(*)::text FROM alojamiento.catalogo_servicios;
SELECT 'Tipo-hab catalogo (N:M):   ' || COUNT(*)::text FROM alojamiento.tipo_habitacion_catalogo;
SELECT 'Habitaciones fisicas:      ' || COUNT(*)::text FROM alojamiento.habitacion;
SELECT 'Tarifas vigentes:          ' || COUNT(*)::text FROM alojamiento.tarifa;

-- Vista resumen: habitaciones agrupadas por sucursal y tipo
SELECT
    s.nombre_sucursal,
    th.nombre_tipo_habitacion,
    COUNT(h.id_habitacion) AS habitaciones_disponibles,
    MIN(h.precio_base)     AS precio_base
FROM   alojamiento.habitacion h
JOIN   alojamiento.sucursal s        ON s.id_sucursal       = h.id_sucursal
JOIN   alojamiento.tipo_habitacion th ON th.id_tipo_habitacion = h.id_tipo_habitacion
GROUP  BY s.nombre_sucursal, th.nombre_tipo_habitacion
ORDER  BY s.nombre_sucursal, th.nombre_tipo_habitacion;
