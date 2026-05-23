-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio ACCOMMODATION
-- Base de datos: HotelLux_Accommodation
-- Motor: PostgreSQL 18
-- Version: 2.0
--
-- DEPENDENCIAS: ninguna (independiente)
--
-- CONTENIDO:
--   Schema: alojamiento
--   Tablas: sucursal, sucursal_imagen, tipo_habitacion,
--           tipo_habitacion_imagen, catalogo_servicios,
--           tipo_habitacion_catalogo, habitacion, tarifa
--
--   Datos semilla:
--     3 sucursales   : Quito, Guayaquil, Cuenca
--     6 imagenes     : 2 por sucursal
--     4 tipos hab    : Single, Doble, Familiar, Premium
--     8 imagenes     : 2 por tipo de habitacion
--     8 catalogos    : 4 amenidades (AME) + 4 servicios (SRV)
--     15 relaciones  : tipo_habitacion_catalogo
--     10 habitaciones: 4 Quito, 3 Guayaquil, 3 Cuenca
--     8 tarifas      : vigentes 2026, una por sucursal-tipo disponible
--
-- INSTRUCCIONES EN pgAdmin:
--   1. Create Database: HotelLux_Accommodation / Owner: postgres
--   2. Query Tool -> File -> Open este archivo -> F5
--   3. Verificar los SELECT de conteo al final.
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
    sucursal_guid            UUID          NOT NULL DEFAULT gen_random_uuid(),
    codigo_sucursal          VARCHAR(30)   NOT NULL,
    nombre_sucursal          VARCHAR(150)  NOT NULL,
    descripcion_sucursal     TEXT          NULL,
    descripcion_corta        VARCHAR(250)  NULL,
    tipo_alojamiento         VARCHAR(20)   NOT NULL DEFAULT 'hotel',
    estrellas                INT           NULL,
    categoria_viaje          VARCHAR(30)   NULL,
    pais                     VARCHAR(100)  NOT NULL,
    provincia                VARCHAR(100)  NULL,
    ciudad                   VARCHAR(100)  NOT NULL,
    ubicacion                VARCHAR(200)  NOT NULL,
    direccion                VARCHAR(250)  NOT NULL,
    codigo_postal            VARCHAR(20)   NULL,
    telefono                 VARCHAR(30)   NOT NULL,
    correo                   VARCHAR(150)  NOT NULL,
    latitud                  NUMERIC(10,7) NULL,
    longitud                 NUMERIC(10,7) NULL,
    hora_checkin             VARCHAR(5)    NULL DEFAULT '15:00',
    hora_checkout            VARCHAR(5)    NULL DEFAULT '12:00',
    checkin_anticipado       BOOLEAN       NOT NULL DEFAULT FALSE,
    checkout_tardio          BOOLEAN       NOT NULL DEFAULT FALSE,
    acepta_ninos             BOOLEAN       NOT NULL DEFAULT TRUE,
    edad_minima_huesped      INT           NULL,
    permite_mascotas         BOOLEAN       NOT NULL DEFAULT FALSE,
    se_permite_fumar         BOOLEAN       NOT NULL DEFAULT FALSE,
    estado_sucursal          CHAR(3)       NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN       NOT NULL DEFAULT FALSE,
    fecha_inhabilitacion_utc TIMESTAMPTZ   NULL,
    motivo_inhabilitacion    VARCHAR(250)  NULL,
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100)  NOT NULL,
    modificado_por_usuario   VARCHAR(100)  NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          VARCHAR(45)   NULL,
    servicio_origen          VARCHAR(50)   NOT NULL DEFAULT 'accommodation-service',
    CONSTRAINT uq_sucursal_guid   UNIQUE (sucursal_guid),
    CONSTRAINT uq_sucursal_codigo UNIQUE (codigo_sucursal),
    CONSTRAINT uq_sucursal_nombre UNIQUE (nombre_sucursal),
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
    CONSTRAINT uq_tipo_habitacion_guid   UNIQUE (tipo_habitacion_guid),
    CONSTRAINT uq_tipo_habitacion_codigo UNIQUE (codigo_tipo_habitacion),
    CONSTRAINT uq_tipo_habitacion_nombre UNIQUE (nombre_tipo_habitacion),
    CONSTRAINT chk_tipo_habitacion_estado  CHECK (estado_tipo_habitacion IN ('ACT','INA')),
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
-- ============================================================
CREATE TABLE alojamiento.catalogo_servicios (
    id_catalogo              INT           NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    catalogo_guid            UUID          NOT NULL DEFAULT gen_random_uuid(),
    id_sucursal              INT           NULL,
    codigo_catalogo          VARCHAR(30)   NOT NULL,
    nombre_catalogo          VARCHAR(120)  NOT NULL,
    tipo_catalogo            CHAR(3)       NOT NULL,
    categoria_catalogo       VARCHAR(80)   NOT NULL,
    descripcion_catalogo     VARCHAR(250)  NULL,
    precio_base              NUMERIC(12,2) NOT NULL DEFAULT 0,
    aplica_iva               BOOLEAN       NOT NULL DEFAULT FALSE,
    disponible_24h           BOOLEAN       NOT NULL DEFAULT FALSE,
    hora_inicio              TIME          NULL,
    hora_fin                 TIME          NULL,
    icono_url                VARCHAR(500)  NULL,
    estado_catalogo          CHAR(3)       NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN       NOT NULL DEFAULT FALSE,
    fecha_inhabilitacion_utc TIMESTAMPTZ   NULL,
    motivo_inhabilitacion    VARCHAR(250)  NULL,
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100)  NOT NULL,
    modificado_por_usuario   VARCHAR(100)  NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          VARCHAR(45)   NULL,
    servicio_origen          VARCHAR(50)   NOT NULL DEFAULT 'accommodation-service',
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
-- ============================================================
CREATE TABLE alojamiento.tipo_habitacion_catalogo (
    id_tipo_hab_catalogo INT         NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_tipo_habitacion   INT         NOT NULL,
    id_catalogo          INT         NOT NULL,
    fecha_registro_utc   TIMESTAMPTZ NOT NULL DEFAULT now(),
    creado_por_usuario   VARCHAR(100) NOT NULL,
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
    id_habitacion            INT           NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    habitacion_guid          UUID          NOT NULL DEFAULT gen_random_uuid(),
    id_sucursal              INT           NOT NULL,
    id_tipo_habitacion       INT           NOT NULL,
    numero_habitacion        VARCHAR(20)   NOT NULL,
    piso                     INT           NULL,
    capacidad_habitacion     INT           NOT NULL,
    precio_base              NUMERIC(12,2) NOT NULL,
    descripcion_habitacion   VARCHAR(250)  NULL,
    estado_habitacion        CHAR(3)       NOT NULL DEFAULT 'DIS',
    es_eliminado             BOOLEAN       NOT NULL DEFAULT FALSE,
    fecha_inhabilitacion_utc TIMESTAMPTZ   NULL,
    motivo_inhabilitacion    VARCHAR(250)  NULL,
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100)  NOT NULL,
    modificado_por_usuario   VARCHAR(100)  NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          VARCHAR(45)   NULL,
    servicio_origen          VARCHAR(50)   NOT NULL DEFAULT 'accommodation-service',
    CONSTRAINT uq_habitacion_guid            UNIQUE (habitacion_guid),
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
    id_tarifa                INT           NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tarifa_guid              UUID          NOT NULL DEFAULT gen_random_uuid(),
    codigo_tarifa            VARCHAR(30)   NOT NULL,
    id_sucursal              INT           NOT NULL,
    id_tipo_habitacion       INT           NOT NULL,
    nombre_tarifa            VARCHAR(150)  NOT NULL,
    canal_tarifa             VARCHAR(30)   NOT NULL DEFAULT 'TODOS',
    fecha_inicio             DATE          NOT NULL,
    fecha_fin                DATE          NOT NULL,
    precio_por_noche         NUMERIC(12,2) NOT NULL,
    porcentaje_iva           NUMERIC(5,2)  NOT NULL DEFAULT 15.00,
    min_noches               INT           NOT NULL DEFAULT 1,
    max_noches               INT           NULL,
    permite_portal_publico   BOOLEAN       NOT NULL DEFAULT TRUE,
    prioridad                INT           NOT NULL DEFAULT 1,
    estado_tarifa            CHAR(3)       NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN       NOT NULL DEFAULT FALSE,
    fecha_inhabilitacion_utc TIMESTAMPTZ   NULL,
    motivo_inhabilitacion    VARCHAR(250)  NULL,
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100)  NOT NULL,
    modificado_por_usuario   VARCHAR(100)  NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          VARCHAR(45)   NULL,
    servicio_origen          VARCHAR(50)   NOT NULL DEFAULT 'accommodation-service',
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
-- ============================================================
-- DATOS SEMILLA
-- ============================================================
-- ============================================================


-- ============================================================
-- SUCURSALES (prefijo GUID: 44xxxxxx-...)
--
--   44...001 = Hotel Luxemburgo Quito     (Pichincha, 5 estrellas)
--   44...002 = Hotel Luxemburgo Guayaquil (Guayas,    5 estrellas)
--   44...003 = Hotel Luxemburgo Cuenca    (Azuay,     4 estrellas)
-- ============================================================
INSERT INTO alojamiento.sucursal (
    sucursal_guid, codigo_sucursal, nombre_sucursal,
    descripcion_sucursal, descripcion_corta,
    tipo_alojamiento, estrellas, categoria_viaje,
    pais, provincia, ciudad, ubicacion, direccion, codigo_postal,
    telefono, correo, latitud, longitud,
    hora_checkin, hora_checkout,
    checkin_anticipado, checkout_tardio,
    acepta_ninos, permite_mascotas, se_permite_fumar,
    creado_por_usuario
) VALUES
-- Sucursal 1: Quito
(
    '44444444-4444-4444-4444-444444444001',
    'LUX-UIO',
    'Hotel Luxemburgo Quito',
    'Ubicado en el corazon de La Mariscal, el Hotel Luxemburgo Quito es un hotel boutique de 5 estrellas que combina la elegancia europea con la calidez andina. Ofrece vistas privilegiadas al Pichincha y al centro historico de Quito, Patrimonio de la Humanidad. Cada detalle ha sido pensado para brindar una experiencia de lujo autentica en la capital ecuatoriana.',
    'Hotel boutique 5 estrellas en La Mariscal, Quito. Vista al Pichincha y al centro historico.',
    'hotel', 5, 'ciudad',
    'Ecuador', 'Pichincha', 'Quito',
    'Sector La Mariscal, norte de Quito',
    'Av. 6 de Diciembre N24-65 y Calle Foch',
    '170135',
    '+593 2 222-1100',
    'quito@hotelluxemburgo.com',
    -0.2058543, -78.4929210,
    '15:00', '12:00',
    TRUE, TRUE,
    TRUE, FALSE, FALSE,
    'admin'
),
-- Sucursal 2: Guayaquil
(
    '44444444-4444-4444-4444-444444444002',
    'LUX-GYE',
    'Hotel Luxemburgo Guayaquil',
    'Frente al icónico Malecon 2000 y con vista panoramica al rio Guayas, el Hotel Luxemburgo Guayaquil es el destino de lujo por excelencia en la ciudad portuaria. Su arquitectura contemporanea contrasta con la vibrante vida urbana de Guayaquil, ofreciendo piscina exterior climatizada, restaurante gourmet y acceso directo al malecon renovado.',
    'Hotel premium 5 estrellas frente al Malecon 2000 con vista al rio Guayas, Guayaquil.',
    'hotel', 5, 'ciudad',
    'Ecuador', 'Guayas', 'Guayaquil',
    'Malecon 2000, centro de Guayaquil',
    'Malecon Simon Bolivar 1100 y Calle Sucre',
    '090313',
    '+593 4 256-7800',
    'guayaquil@hotelluxemburgo.com',
    -2.1894128, -79.8807552,
    '15:00', '12:00',
    FALSE, FALSE,
    TRUE, TRUE, FALSE,
    'admin'
),
-- Sucursal 3: Cuenca
(
    '44444444-4444-4444-4444-444444444003',
    'LUX-CUE',
    'Hotel Luxemburgo Cuenca',
    'Instalado en una casona colonial restaurada del siglo XIX, el Hotel Luxemburgo Cuenca esta ubicado en el Centro Historico declarado Patrimonio Cultural de la Humanidad por la UNESCO. Sus habitaciones conservan elementos arquitectonicos originales como pisos de madera, artesonados y balcones de hierro forjado, fusionados con comodidades de lujo contemporaneas.',
    'Hotel colonial 4 estrellas en el Centro Historico de Cuenca, Patrimonio UNESCO.',
    'hotel', 4, 'cultural',
    'Ecuador', 'Azuay', 'Cuenca',
    'Centro Historico de Cuenca',
    'Calle Larga 7-25 y Calle Borrero',
    '010103',
    '+593 7 283-4500',
    'cuenca@hotelluxemburgo.com',
    -2.8974217, -79.0058736,
    '14:00', '12:00',
    FALSE, FALSE,
    TRUE, FALSE, FALSE,
    'admin'
);


-- ============================================================
-- IMAGENES DE SUCURSALES (2 por sucursal)
-- ============================================================
INSERT INTO alojamiento.sucursal_imagen (
    id_sucursal, url_imagen, descripcion_imagen,
    es_principal, orden_visualizacion, creado_por_usuario
) VALUES
-- Quito: fachada + lobby
(
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    'https://cdn.hotelluxemburgo.com/sucursales/lux-uio/fachada-principal.jpg',
    'Fachada principal del Hotel Luxemburgo Quito con vista a la Av. 6 de Diciembre',
    TRUE, 1, 'admin'
),
(
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    'https://cdn.hotelluxemburgo.com/sucursales/lux-uio/lobby-recepcion.jpg',
    'Lobby y area de recepcion con decoracion andina contemporanea',
    FALSE, 2, 'admin'
),
-- Guayaquil: vista al malecon + piscina
(
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    'https://cdn.hotelluxemburgo.com/sucursales/lux-gye/vista-malecon.jpg',
    'Vista panoramica al Malecon 2000 y al rio Guayas desde el Hotel Luxemburgo Guayaquil',
    TRUE, 1, 'admin'
),
(
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    'https://cdn.hotelluxemburgo.com/sucursales/lux-gye/piscina-exterior.jpg',
    'Piscina exterior climatizada con terraza y bar de piscina',
    FALSE, 2, 'admin'
),
-- Cuenca: patio colonial + fachada
(
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    'https://cdn.hotelluxemburgo.com/sucursales/lux-cue/fachada-colonial.jpg',
    'Fachada colonial restaurada del Hotel Luxemburgo Cuenca en la Calle Larga',
    TRUE, 1, 'admin'
),
(
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    'https://cdn.hotelluxemburgo.com/sucursales/lux-cue/patio-interior.jpg',
    'Patio interior con jardin y fuente de piedra tipica de las casonas cuencanas',
    FALSE, 2, 'admin'
);


-- ============================================================
-- TIPOS DE HABITACION (prefijo GUID: 55xxxxxx-...)
--
--   55...001 = Suite Single   (1 adulto,  22 m2, King size)
--   55...002 = Suite Doble    (2 adultos, 32 m2, 2 Queen)
--   55...003 = Suite Familiar (4 personas,48 m2, King + sofa-cama)
--   55...004 = Suite Premium  (2 adultos, 65 m2, King lujo + jacuzzi)
-- ============================================================
INSERT INTO alojamiento.tipo_habitacion (
    tipo_habitacion_guid, codigo_tipo_habitacion, nombre_tipo_habitacion,
    descripcion, capacidad_adultos, capacidad_ninos, capacidad_total,
    tipo_cama, area_m2, permite_eventos, permite_reserva_publica,
    creado_por_usuario
) VALUES
(
    '55555555-5555-5555-5555-555555555001',
    'TH-SINGLE',
    'Suite Single',
    'Habitacion individual con cama King size, escritorio ejecutivo, minibar y bano privado con ducha de lluvia. Diseno moderno con iluminacion calida. Ideal para viajeros de negocios o turistas que buscan confort y privacidad. Incluye caja fuerte digital, smart TV 43 pulgadas y amenities de bano de la linea Hermes.',
    1, 0, 1,
    'King size 200x200 cm',
    22.00, FALSE, TRUE, 'admin'
),
(
    '55555555-5555-5555-5555-555555555002',
    'TH-DOBLE',
    'Suite Doble',
    'Suite para dos personas con dos camas Queen size independientes, sala de estar compacta, escritorio y bano privado con tina. Perfecta para parejas o companeros de viaje que prefieren camas separadas. Decoracion con toques ecuatorianos en textiles y artesanias de Otavalo. Incluye minibar, smart TV 50 pulgadas y vista parcial a la ciudad.',
    2, 0, 2,
    '2 Queen size 160x200 cm',
    32.00, FALSE, TRUE, 'admin'
),
(
    '55555555-5555-5555-5555-555555555003',
    'TH-FAMILIAR',
    'Suite Familiar',
    'Suite de dos ambientes diseñada para familias: dormitorio principal con cama King size y sala de estar con sofa-cama doble. Bano principal completo con tina y bano auxiliar con ducha. Espacio amplio para ninos con entretenimiento adicional. Caja fuerte de talla maleta, frigobar doble y acceso a servicio de canguro bajo coordinacion con recepcion.',
    2, 2, 4,
    '1 King size + 1 Sofa-cama doble',
    48.00, FALSE, TRUE, 'admin'
),
(
    '55555555-5555-5555-5555-555555555004',
    'TH-PREMIUM',
    'Suite Premium',
    'La maxima expresion de lujo del Hotel Luxemburgo. Ubicada en los pisos superiores con vista panoramica a la ciudad. Sala de estar independiente, comedor para dos personas, jacuzzi privado y ducha de vapor en el bano principal. Incluye servicio de mayordomo personalizado de 06h00 a 22h00, amenities exclusivos Bulgari, champagne de bienvenida y acceso al Club Lounge.',
    2, 0, 2,
    '1 King size 200x200 cm de lujo con somier articulado',
    65.00, TRUE, TRUE, 'admin'
);


-- ============================================================
-- IMAGENES DE TIPOS DE HABITACION (2 por tipo)
-- ============================================================
INSERT INTO alojamiento.tipo_habitacion_imagen (
    id_tipo_habitacion, url_imagen, descripcion_imagen,
    es_principal, orden_visualizacion, creado_por_usuario
) VALUES
-- Suite Single
(
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'https://cdn.hotelluxemburgo.com/habitaciones/th-single/dormitorio.jpg',
    'Dormitorio de la Suite Single con cama King size y vista a la ciudad',
    TRUE, 1, 'admin'
),
(
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'https://cdn.hotelluxemburgo.com/habitaciones/th-single/bano.jpg',
    'Bano privado con ducha de lluvia y amenities de lujo',
    FALSE, 2, 'admin'
),
-- Suite Doble
(
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'https://cdn.hotelluxemburgo.com/habitaciones/th-doble/dormitorio.jpg',
    'Dormitorio de la Suite Doble con dos camas Queen y decoracion con textiles otavalenos',
    TRUE, 1, 'admin'
),
(
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'https://cdn.hotelluxemburgo.com/habitaciones/th-doble/sala.jpg',
    'Sala de estar compacta con escritorio ejecutivo y smart TV',
    FALSE, 2, 'admin'
),
-- Suite Familiar
(
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'https://cdn.hotelluxemburgo.com/habitaciones/th-familiar/dormitorio-principal.jpg',
    'Dormitorio principal de la Suite Familiar con cama King size',
    TRUE, 1, 'admin'
),
(
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'https://cdn.hotelluxemburgo.com/habitaciones/th-familiar/sala-estar.jpg',
    'Sala de estar con sofa-cama doble y area de entretenimiento para ninos',
    FALSE, 2, 'admin'
),
-- Suite Premium
(
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    'https://cdn.hotelluxemburgo.com/habitaciones/th-premium/suite-completa.jpg',
    'Vista completa de la Suite Premium con sala independiente y acceso al jacuzzi privado',
    TRUE, 1, 'admin'
),
(
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    'https://cdn.hotelluxemburgo.com/habitaciones/th-premium/jacuzzi-vista.jpg',
    'Jacuzzi privado con vista panoramica a la ciudad desde los pisos superiores',
    FALSE, 2, 'admin'
);


-- ============================================================
-- CATALOGO DE SERVICIOS (prefijo GUID: 88xxxxxx-...)
--
-- AME = Amenidad incluida en la tarifa (precio_base SIEMPRE 0)
--   88...001 = WiFi de alta velocidad
--   88...002 = Aire acondicionado
--   88...003 = Smart TV con streaming
--   88...004 = Minibar de cortesia
--
-- SRV = Servicio adicional con costo
--   88...005 = Servicio a la habitacion 24h
--   88...006 = Lavanderia y planchado
--   88...007 = Spa y masajes
--   88...008 = Transporte al aeropuerto
-- ============================================================
INSERT INTO alojamiento.catalogo_servicios (
    catalogo_guid, id_sucursal, codigo_catalogo, nombre_catalogo,
    tipo_catalogo, categoria_catalogo, descripcion_catalogo,
    precio_base, aplica_iva, disponible_24h, hora_inicio, hora_fin,
    icono_url, creado_por_usuario
) VALUES
-- ---- AMENIDADES (AME) ----------------------------------------
(
    '88888888-8888-8888-8888-888888888001',
    NULL,
    'AME-WIFI',
    'WiFi de alta velocidad',
    'AME', 'Conectividad',
    'Acceso WiFi gratuito de 100 Mbps en habitacion y areas comunes. Red dedicada para huespedes con soporte tecnico disponible en recepcion.',
    0.00, FALSE, TRUE, NULL, NULL,
    'icons/wifi.svg', 'admin'
),
(
    '88888888-8888-8888-8888-888888888002',
    NULL,
    'AME-AC',
    'Aire acondicionado y calefaccion',
    'AME', 'Confort',
    'Sistema de climatizacion individual con control de temperatura entre 16°C y 28°C. Incluye calefaccion para las noches frias andinas en Quito y Cuenca.',
    0.00, FALSE, TRUE, NULL, NULL,
    'icons/ac.svg', 'admin'
),
(
    '88888888-8888-8888-8888-888888888003',
    NULL,
    'AME-TV',
    'Smart TV 4K con cable y streaming',
    'AME', 'Entretenimiento',
    'Televisor LED 4K de 50 pulgadas con cable HD, Netflix, Amazon Prime Video y YouTube preconfigurados. Control de voz disponible en Suite Premium.',
    0.00, FALSE, TRUE, NULL, NULL,
    'icons/tv.svg', 'admin'
),
(
    '88888888-8888-8888-8888-888888888004',
    NULL,
    'AME-MINIBAR',
    'Minibar de cortesia',
    'AME', 'Alimentos y bebidas',
    'Minibar reabastecido diariamente con agua mineral San Luis, refrescos, cerveza Pilsener, snacks locales y chocolates artesanales de Pacari. Solo disponible en Suite Doble, Suite Familiar y Suite Premium.',
    0.00, FALSE, TRUE, NULL, NULL,
    'icons/minibar.svg', 'admin'
),
-- ---- SERVICIOS ADICIONALES (SRV) ----------------------------
(
    '88888888-8888-8888-8888-888888888005',
    NULL,
    'SRV-HAB',
    'Servicio a la habitacion',
    'SRV', 'Alimentos y bebidas',
    'Servicio de alimentos y bebidas a la habitacion disponible las 24 horas. Menu completo con opciones de desayuno, almuerzo, merienda y cena, incluyendo platos tipicos ecuatorianos y carta internacional. Tiempo de entrega estimado: 30 minutos.',
    15.00, TRUE, TRUE, NULL, NULL,
    'icons/room-service.svg', 'admin'
),
(
    '88888888-8888-8888-8888-888888888006',
    NULL,
    'SRV-LAV',
    'Lavanderia y planchado',
    'SRV', 'Limpieza y cuidado',
    'Servicio de lavanderia, secado y planchado con entrega en 24 horas habiles. Recogida antes de las 09h00 garantiza entrega el mismo dia. Incluye bolsa de lavanderia en cada habitacion.',
    12.50, TRUE, FALSE, '08:00', '18:00',
    'icons/laundry.svg', 'admin'
),
(
    '88888888-8888-8888-8888-888888888007',
    NULL,
    'SRV-SPA',
    'Spa Andes Wellness',
    'SRV', 'Bienestar',
    'Sesion de 60 min en el Spa Andes Wellness. Masaje relajante, tejido profundo, envoltura de barro andino o tratamiento facial con productos nativos del Ecuador. Requiere reserva con minimo 2 horas de anticipacion.',
    45.00, TRUE, FALSE, '09:00', '20:00',
    'icons/spa.svg', 'admin'
),
(
    '88888888-8888-8888-8888-888888888008',
    NULL,
    'SRV-TRANS',
    'Traslado aeropuerto - hotel',
    'SRV', 'Transporte',
    'Traslado privado en vehiculo ejecutivo al aeropuerto local. Conductor bilingue, seguimiento de vuelo incluido. Cubre aeropuertos Mariscal Sucre (UIO), Jose J. de Olmedo (GYE) y Mariscal Lamar (CUE).',
    25.00, TRUE, TRUE, NULL, NULL,
    'icons/transfer.svg', 'admin'
);


-- ============================================================
-- RELACION N:M: tipo_habitacion <-> catalogo_servicios
--
-- Distribucion de amenidades por tipo:
--   TH-SINGLE  : WiFi, AC, TV              (sin minibar)
--   TH-DOBLE   : WiFi, AC, TV, Minibar
--   TH-FAMILIAR: WiFi, AC, TV, Minibar
--   TH-PREMIUM : WiFi, AC, TV, Minibar
-- ============================================================
INSERT INTO alojamiento.tipo_habitacion_catalogo (
    id_tipo_habitacion, id_catalogo, creado_por_usuario
)
-- WiFi, AC y Smart TV para TODOS los tipos
SELECT th.id_tipo_habitacion, c.id_catalogo, 'admin'
FROM   alojamiento.tipo_habitacion th
CROSS  JOIN alojamiento.catalogo_servicios c
WHERE  c.codigo_catalogo IN ('AME-WIFI', 'AME-AC', 'AME-TV')

UNION ALL

-- Minibar solo para Suite Doble, Familiar y Premium
SELECT th.id_tipo_habitacion, c.id_catalogo, 'admin'
FROM   alojamiento.tipo_habitacion th
CROSS  JOIN alojamiento.catalogo_servicios c
WHERE  c.codigo_catalogo = 'AME-MINIBAR'
  AND  th.codigo_tipo_habitacion IN ('TH-DOBLE', 'TH-FAMILIAR', 'TH-PREMIUM');


-- ============================================================
-- HABITACIONES (prefijo GUID: 66xxxxxx-...)
--
-- Quito (4 hab):
--   66...001 = 101 Single  piso 1  $80
--   66...002 = 102 Doble   piso 1  $120
--   66...003 = 201 Familiar piso 2 $180
--   66...004 = 301 Premium  piso 3 $250
--
-- Guayaquil (3 hab):
--   66...005 = 101 Doble   piso 1  $130  (vista malecon A)
--   66...006 = 102 Doble   piso 1  $130  (vista malecon B)
--   66...007 = 201 Familiar piso 2 $190
--
-- Cuenca (3 hab):
--   66...008 = 101 Single  piso 1  $75
--   66...009 = 102 Doble   piso 1  $115
--   66...010 = 201 Premium  piso 2 $230
-- ============================================================
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES
-- ---- QUITO --------------------------------------------------
(
    '66666666-6666-6666-6666-666666666001',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 80.00,
    'Suite Single en primer piso, orientacion norte. Vista a los jardines interiores del hotel. Tranquila y alejada de la calle principal.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666002',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 120.00,
    'Suite Doble en primer piso, orientacion sur. Vista parcial a la Av. 6 de Diciembre. Ideal para parejas en visita de negocios o turismo.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666003',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '201', 2, 4, 180.00,
    'Suite Familiar en segundo piso, orientacion este. Vista al barrio de La Mariscal y hacia los valles de Quito. Espacio amplio con dos ambientes completamente separados.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666004',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    '301', 3, 2, 250.00,
    'Suite Premium en tercer piso, orientacion oeste. Vista panoramica al volcan Pichincha y al atardecer sobre Quito. La habitacion mas exclusiva de la sucursal.',
    'DIS', 'admin'
),
-- ---- GUAYAQUIL ----------------------------------------------
(
    '66666666-6666-6666-6666-666666666005',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 130.00,
    'Suite Doble en primer piso, orientacion oeste. Vista directa al Malecon 2000 y al rio Guayas. Una de las habitaciones con mejor panoramica de Guayaquil.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666006',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 130.00,
    'Suite Doble en primer piso, orientacion este. Vista al centro urbano de Guayaquil y al cerro Santa Ana. Gemela a la habitacion 101 con identica distribucion.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666007',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '201', 2, 4, 190.00,
    'Suite Familiar en segundo piso, orientacion oeste. Amplia vista al Malecon y al estero Salado. Sala de estar con acceso a balcon privado, ideal para familias que visitan Guayaquil.',
    'DIS', 'admin'
),
-- ---- CUENCA -------------------------------------------------
(
    '66666666-6666-6666-6666-666666666008',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 75.00,
    'Suite Single en planta baja de la casona colonial, orientacion al patio interior. Piso de madera original del siglo XIX, techo artesonado y ventanas con vidrio emplomado. La habitacion con mas caracter historico de la sucursal.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666009',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 115.00,
    'Suite Doble en planta baja, orientacion a la Calle Larga. Dos camas Queen, balcon de hierro forjado con vista al rio Tomebamba y a los bohios de Las Herrerias. Una de las vistas mas romanticas de Cuenca.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666010',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    '201', 2, 2, 230.00,
    'Suite Premium en segundo piso de la casona, orientacion al rio Tomebamba y las Catedral Nueva. Jacuzzi privado con vista al casco historico, vigas de madera originales y decoracion con ceramica azuay. La suite mas exclusiva de Cuenca.',
    'DIS', 'admin'
);


-- ============================================================
-- TARIFAS (prefijo GUID: 77xxxxxx-...)
--
-- Una tarifa activa por combinacion sucursal + tipo_habitacion.
-- Vigencia: 2026-01-01 al 2026-12-31. IVA: 15% (Ecuador).
-- Canal: TODOS (aplica para portal, admin, API y walk-in).
-- min_noches: 1 excepto Suite Familiar (min 2 noches).
--
-- Cuenca no tiene Suite Familiar fisica => no tiene tarifa familiar.
-- ============================================================
INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa, id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES
-- ---- QUITO (4 tarifas) --------------------------------------
(
    '77777777-7777-7777-7777-777777777001',
    'TAR-UIO-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Quito 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    80.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777002',
    'TAR-UIO-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Quito 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    120.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777003',
    'TAR-UIO-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Quito 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    180.00, 15.00, 2, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777004',
    'TAR-UIO-PREMIUM-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    'Tarifa Estandar Suite Premium Quito 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    250.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
-- ---- GUAYAQUIL (3 tarifas: Doble A/B comparten tipo, Familiar)
(
    '77777777-7777-7777-7777-777777777005',
    'TAR-GYE-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Guayaquil 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    130.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777006',
    'TAR-GYE-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Guayaquil 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    190.00, 15.00, 2, NULL, TRUE, 1, 'admin'
),
-- ---- CUENCA (2 tarifas: Single y Doble; no hay Familiar fisica)
(
    '77777777-7777-7777-7777-777777777007',
    'TAR-CUE-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Cuenca 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    75.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777008',
    'TAR-CUE-PREMIUM-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    'Tarifa Estandar Suite Premium Cuenca 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    230.00, 15.00, 1, NULL, TRUE, 1, 'admin'
);


-- ============================================================
-- VERIFICACION FINAL
-- Resultados esperados:
--   Sucursales             : 3
--   Imagenes de sucursal   : 6
--   Tipos de habitacion    : 4
--   Imagenes tipo hab      : 8
--   Catalogo de servicios  : 8
--   Tipo-hab catalogo (N:M): 15  (4x3 AME-sin-minibar + 3x1 AME-minibar)
--   Habitaciones fisicas   : 10
--   Tarifas vigentes       : 8
-- ============================================================
SELECT 'Sucursales:                 ' || COUNT(*)::text AS resultado FROM alojamiento.sucursal;
SELECT 'Imagenes sucursal:          ' || COUNT(*)::text AS resultado FROM alojamiento.sucursal_imagen;
SELECT 'Tipos de habitacion:        ' || COUNT(*)::text AS resultado FROM alojamiento.tipo_habitacion;
SELECT 'Imagenes tipo habitacion:   ' || COUNT(*)::text AS resultado FROM alojamiento.tipo_habitacion_imagen;
SELECT 'Catalogo de servicios:      ' || COUNT(*)::text AS resultado FROM alojamiento.catalogo_servicios;
SELECT 'Tipo-hab catalogo (N:M):    ' || COUNT(*)::text AS resultado FROM alojamiento.tipo_habitacion_catalogo;
SELECT 'Habitaciones fisicas:       ' || COUNT(*)::text AS resultado FROM alojamiento.habitacion;
SELECT 'Tarifas vigentes:           ' || COUNT(*)::text AS resultado FROM alojamiento.tarifa;

-- Vista resumen: habitaciones por sucursal y tipo con tarifa
SELECT
    s.nombre_sucursal,
    th.nombre_tipo_habitacion,
    COUNT(h.id_habitacion)          AS habitaciones,
    MIN(h.precio_base)              AS precio_base_usd,
    t.precio_por_noche              AS tarifa_noche_usd,
    ROUND(t.precio_por_noche * 1.15, 2) AS tarifa_con_iva_usd,
    t.min_noches
FROM       alojamiento.habitacion h
JOIN       alojamiento.sucursal          s  ON s.id_sucursal        = h.id_sucursal
JOIN       alojamiento.tipo_habitacion   th ON th.id_tipo_habitacion = h.id_tipo_habitacion
LEFT JOIN  alojamiento.tarifa            t  ON t.id_sucursal         = h.id_sucursal
                                           AND t.id_tipo_habitacion  = h.id_tipo_habitacion
                                           AND t.estado_tarifa       = 'ACT'
GROUP BY   s.nombre_sucursal, th.nombre_tipo_habitacion,
           t.precio_por_noche, t.min_noches
ORDER BY   s.nombre_sucursal, th.nombre_tipo_habitacion;