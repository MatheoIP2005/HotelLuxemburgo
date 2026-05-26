-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio ACCOMMODATION
-- Base de datos: HotelLux_Accommodation
-- Motor: PostgreSQL 18
-- Version: 3.0
--
-- DEPENDENCIAS: ninguna (independiente)
--
-- CONTENIDO:
--   Schema: alojamiento
--   Tablas: sucursal, sucursal_imagen, tipo_habitacion,
--           tipo_habitacion_imagen, catalogo_servicios,
--           tipo_habitacion_catalogo, habitacion, tarifa
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
    codigo_sucursal          CHAR(10)      NOT NULL,
    nombre_sucursal          VARCHAR(100)  NOT NULL,
    descripcion_sucursal     VARCHAR(350)  NULL,
    descripcion_corta        VARCHAR(250)  NULL,
    tipo_alojamiento         VARCHAR(20)   NOT NULL DEFAULT 'hotel',
    estrellas                INT           NULL,
    categoria_viaje          CHAR(30)      NULL,
    pais                     CHAR(15)      NOT NULL,
    provincia                CHAR(30)      NULL,
    ciudad                   CHAR(25)      NOT NULL,
    ubicacion                VARCHAR(200)  NOT NULL,
    direccion                VARCHAR(250)  NOT NULL,
    codigo_postal            VARCHAR(20)   NULL,
    telefono                 CHAR(9)       NOT NULL,
    correo                   CHAR(50)      NOT NULL,
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
    motivo_inhabilitacion    VARCHAR(150)  NULL,
    fecha_registro_utc       TIMESTAMPTZ   NOT NULL DEFAULT now(),
    creado_por_usuario       CHAR(30)      NOT NULL,
    modificado_por_usuario   CHAR(30)      NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          CHAR(25)      NULL,
    servicio_origen          CHAR(50)      NOT NULL DEFAULT 'accommodation-service',
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
    creado_por_usuario       CHAR(30)     NOT NULL,
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
    codigo_tipo_habitacion   CHAR(30)     NOT NULL,
    nombre_tipo_habitacion   CHAR(60)     NOT NULL,
    descripcion              TEXT         NULL,
    capacidad_adultos        INT          NOT NULL,
    capacidad_ninos          INT          NOT NULL DEFAULT 0,
    capacidad_total          INT          NOT NULL,
    tipo_cama                CHAR(60)     NULL,
    area_m2                  NUMERIC(6,2) NULL,
    permite_eventos          BOOLEAN      NOT NULL DEFAULT FALSE,
    permite_reserva_publica  BOOLEAN      NOT NULL DEFAULT TRUE,
    estado_tipo_habitacion   CHAR(3)      NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    motivo_inhabilitacion    VARCHAR(250) NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       CHAR(30)     NOT NULL,
    modificado_por_usuario   CHAR(30)     NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          CHAR(25)     NULL,
    servicio_origen          CHAR(50)     NOT NULL DEFAULT 'accommodation-service',
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
    creado_por_usuario        CHAR(50)     NOT NULL,
    CONSTRAINT uq_tipo_hab_imagen_guid UNIQUE (tipo_hab_imagen_guid),
    CONSTRAINT fk_tipo_hab_imagen_tipo FOREIGN KEY (id_tipo_habitacion)
        REFERENCES alojamiento.tipo_habitacion(id_tipo_habitacion) ON DELETE CASCADE,
    CONSTRAINT chk_tipo_hab_imagen_orden CHECK (orden_visualizacion > 0)
);


-- ============================================================
-- TABLA: alojamiento.catalogo_servicios
-- id_sucursal NOT NULL: cada servicio/amenidad pertenece a
-- una sucursal especifica. El microservicio filtra por
-- sucursal_guid al consultar servicios disponibles.
-- ============================================================
CREATE TABLE alojamiento.catalogo_servicios (
    id_catalogo              INT           NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    catalogo_guid            UUID          NOT NULL DEFAULT gen_random_uuid(),
    id_sucursal              INT           NOT NULL,
    codigo_catalogo          CHAR(10)      NOT NULL,
    nombre_catalogo          CHAR(60)      NOT NULL,
    tipo_catalogo            CHAR(3)       NOT NULL,
    categoria_catalogo       CHAR(80)      NOT NULL,
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
    creado_por_usuario       CHAR(30)      NOT NULL,
    modificado_por_usuario   CHAR(30)      NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          CHAR(25)      NULL,
    servicio_origen          CHAR(50)      NOT NULL DEFAULT 'accommodation-service',
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
    id_tipo_hab_catalogo INT          NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_tipo_habitacion   INT          NOT NULL,
    id_catalogo          INT          NOT NULL,
    fecha_registro_utc   TIMESTAMPTZ  NOT NULL DEFAULT now(),
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
    creado_por_usuario       CHAR(30)      NOT NULL,
    modificado_por_usuario   CHAR(30)      NULL,
    fecha_modificacion_utc   TIMESTAMPTZ   NULL,
    modificacion_ip          CHAR(25)      NULL,
    servicio_origen          CHAR(50)      NOT NULL DEFAULT 'accommodation-service',
    CONSTRAINT uq_habitacion_guid            UNIQUE (habitacion_guid),
    CONSTRAINT uq_habitacion_sucursal_numero UNIQUE (id_sucursal, numero_habitacion),
    CONSTRAINT fk_habitacion_sucursal FOREIGN KEY (id_sucursal)
        REFERENCES alojamiento.sucursal(id_sucursal),
    CONSTRAINT fk_habitacion_tipo FOREIGN KEY (id_tipo_habitacion)
        REFERENCES alojamiento.tipo_habitacion(id_tipo_habitacion),
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
-- HOTEL LUXEMBURGO -- Microservicio ACCOMMODATION
-- Base de datos: HotelLux_Accommodation
-- Motor: PostgreSQL 18
--
-- DATOS SEMILLA UNICAMENTE
-- Version: 3.1 — Todas las sucursales con prefijo LUX-
--
-- CAMBIOS v3.1:
--   Los 27 codigos no-LUX (BUL-UIO, KEN-GYE, COL-IBA, etc.)
--   fueron reemplazados por codigos LUX-xxx. Todos los nombres
--   incluyen "Luxemburgo". Los GUIDs 44...001/002/003 y todas
--   las referencias cruzadas (catalogo, habitaciones, tarifas)
--   permanecen identicos.
--
-- DISTRIBUCION SUCURSALES (30 total = 6 categorias x 5):
--   CIUDAD   : LUX-UIO   LUX-GYE   LUX-UIOH  LUX-GYEA  LUX-UIOR
--   CULTURAL : LUX-CUE   LUX-IBR   LUX-CUEH  LUX-MIT   LUX-CUEA
--   PLAYA    : LUX-MAN   LUX-CAN   LUX-PLA   LUX-SAM   LUX-SAL
--   MONTANA  : LUX-BAN   LUX-COT   LUX-BANV  LUX-QUI   LUX-RIO
--   AVENTURA : LUX-TEN   LUX-YAS   LUX-MND   LUX-NAP   LUX-PUY
--   BIENESTAR: LUX-PAP   LUX-YUN   LUX-BANT  LUX-CUEW  LUX-MNDW
-- ============================================================


-- ============================================================
-- 1. SUCURSALES (30 registros — prefijo GUID: 44xxxxxx-...)
--
-- REGLA: GUIDs 44...001, 44...002 y 44...003 son referencias
-- cruzadas en HotelLux_Reservation, Stay, Finance y Audit.
-- NO cambiar. Todos los demas GUIDs (44...004 al 44...030)
-- son internos de este microservicio.
-- ============================================================
INSERT INTO alojamiento.sucursal (
    sucursal_guid, codigo_sucursal, nombre_sucursal,
    descripcion_sucursal, descripcion_corta,
    tipo_alojamiento, estrellas, categoria_viaje,
    pais, provincia, ciudad,
    ubicacion, direccion, codigo_postal,
    telefono, correo, latitud, longitud,
    hora_checkin, hora_checkout,
    checkin_anticipado, checkout_tardio,
    acepta_ninos, permite_mascotas, se_permite_fumar,
    creado_por_usuario
) VALUES

-- ============================================================
-- CATEGORIA: CIUDAD (5)
-- ============================================================
(
    '44444444-4444-4444-4444-444444444001',
    'LUX-UIO',
    'Hotel Luxemburgo Quito',
    'Hotel boutique 5 estrellas en La Mariscal, Quito. Combina elegancia europea con calidez andina. Vista al Pichincha y al centro historico Patrimonio de la Humanidad. Cada detalle pensado para una experiencia de lujo autentica.',
    'Hotel boutique 5 estrellas en La Mariscal. Vista al Pichincha y centro historico.',
    'hotel', 5, 'ciudad',
    'Ecuador', 'Pichincha', 'Quito',
    'Sector La Mariscal, norte de Quito',
    'Av. 6 de Diciembre N24-65 y Calle Foch', '170135',
    '023222001', 'quito@hotelluxemburgo.com',
    -0.2058543, -78.4929210,
    '15:00', '12:00', TRUE, TRUE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444002',
    'LUX-GYE',
    'Hotel Luxemburgo Guayaquil',
    'Hotel 5 estrellas frente al Malecon 2000, Guayaquil. Vista panoramica al rio Guayas. Arquitectura contemporanea, piscina exterior climatizada y restaurante gourmet. Acceso directo al malecon renovado.',
    'Hotel premium 5 estrellas frente al Malecon 2000 con vista al rio Guayas.',
    'hotel', 5, 'ciudad',
    'Ecuador', 'Guayas', 'Guayaquil',
    'Malecon 2000, centro de Guayaquil',
    'Malecon Simon Bolivar 1100 y Calle Sucre', '090313',
    '042567800', 'guayaquil@hotelluxemburgo.com',
    -2.1894128, -79.8807552,
    '15:00', '12:00', FALSE, FALSE, TRUE, TRUE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444004',
    'LUX-UIOH',
    'Hostal Luxemburgo La Mariscal',
    'Hostal 3 estrellas de la cadena Hotel Luxemburgo en el corazon de La Mariscal, Quito. Ambiente familiar y acogedor con habitaciones confortables. A pasos de restaurantes, bares y puntos turisticos. Ideal para viajeros que buscan comodidad a buen precio.',
    'Hostal Luxemburgo 3 estrellas en La Mariscal, Quito. Ambiente acogedor y ubicacion central.',
    'hostal', 3, 'ciudad',
    'Ecuador', 'Pichincha', 'Quito',
    'La Mariscal, Quito',
    'Calle Reina Victoria N24-20 y Calle Lizardo Garcia', '170136',
    '022345678', 'mariscal@hotelluxemburgo.com',
    -0.2070000, -78.4890000,
    '14:00', '12:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444005',
    'LUX-GYEA',
    'Apartamentos Luxemburgo Kennedy Norte',
    'Apartamentos de la cadena Hotel Luxemburgo totalmente equipados en Kennedy Norte, Guayaquil. Opciones de estudio y suites con cocina completa, ideal para estadias prolongadas de negocios o turismo. A minutos de los principales centros comerciales.',
    'Apartamentos Luxemburgo equipados en Kennedy Norte, Guayaquil. Ideal para estadias largas.',
    'apartamento', NULL, 'ciudad',
    'Ecuador', 'Guayas', 'Guayaquil',
    'Cdla. Kennedy Norte, Guayaquil',
    'Av. Francisco de Orellana y Av. Miguel H. Alcivar', '090501',
    '042889900', 'kennedy@hotelluxemburgo.com',
    -2.1540000, -79.9020000,
    '14:00', '11:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444006',
    'LUX-UIOR',
    'Resort Luxemburgo Mariscal Quito',
    'Resort urbano 5 estrellas Hotel Luxemburgo con spa completo en La Mariscal, Quito. Piscina temperada, circuito de hidroterapia, gimnasio, restaurante de autor y bar con vista a la ciudad. El unico resort de ciudad de la capital de la cadena Luxemburgo.',
    'Resort Luxemburgo 5 estrellas con spa y piscina temperada en La Mariscal, Quito.',
    'resort', 5, 'ciudad',
    'Ecuador', 'Pichincha', 'Quito',
    'La Mariscal, Quito',
    'Calle Foch E4-127 y Av. Amazonas', '170135',
    '022456789', 'resort.uio@hotelluxemburgo.com',
    -0.2065000, -78.4935000,
    '15:00', '12:00', TRUE, TRUE, TRUE, FALSE, FALSE, 'admin'
),

-- ============================================================
-- CATEGORIA: CULTURAL (5)
-- ============================================================
(
    '44444444-4444-4444-4444-444444444003',
    'LUX-CUE',
    'Hotel Luxemburgo Cuenca',
    'Casona colonial restaurada del siglo XIX en el Centro Historico de Cuenca, Patrimonio UNESCO. Habitaciones con pisos de madera, artesonados y balcones de hierro forjado. Fusion de historia y lujo contemporaneo de la cadena Hotel Luxemburgo.',
    'Hotel Luxemburgo colonial 4 estrellas en el Centro Historico de Cuenca, Patrimonio UNESCO.',
    'hotel', 4, 'cultural',
    'Ecuador', 'Azuay', 'Cuenca',
    'Centro Historico de Cuenca',
    'Calle Larga 7-25 y Calle Borrero', '010103',
    '072834500', 'cuenca@hotelluxemburgo.com',
    -2.8974217, -79.0058736,
    '14:00', '12:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444007',
    'LUX-IBR',
    'Villa Luxemburgo Colonial Ibarra',
    'Villa colonial Hotel Luxemburgo en el centro historico de Ibarra, Ciudad Blanca del Ecuador. Hacienda restaurada del siglo XVIII con jardin, piscina de agua termal y cocina de autor con productos del norte andino.',
    'Villa Luxemburgo 4 estrellas en el centro historico de Ibarra, Ciudad Blanca.',
    'villa', 4, 'cultural',
    'Ecuador', 'Imbabura', 'Ibarra',
    'Centro historico de Ibarra',
    'Calle Garcia Moreno 3-21 y Calle Olmedo', '100101',
    '062956123', 'ibarra@hotelluxemburgo.com',
    0.3517200, -78.1222400,
    '15:00', '12:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444008',
    'LUX-CUEH',
    'Hostal Luxemburgo Patrimonial Cuenca',
    'Hostal 3 estrellas Hotel Luxemburgo en casona republicana del Centro Historico de Cuenca. Habitaciones con baldosas artesanales, ventanas de madera y patio interior con fuente. Desayuno incluido con productos locales de la Sierra Sur.',
    'Hostal Luxemburgo 3 estrellas en casona republicana del Centro Historico de Cuenca.',
    'hostal', 3, 'cultural',
    'Ecuador', 'Azuay', 'Cuenca',
    'Centro Historico de Cuenca',
    'Calle Presidente Borrero 5-18 y Calle Sucre', '010101',
    '072834678', 'patrimonio@hotelluxemburgo.com',
    -2.8980000, -79.0040000,
    '13:00', '11:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444009',
    'LUX-MIT',
    'Cabana Luxemburgo Mitad del Mundo',
    'Cabana 3 estrellas Hotel Luxemburgo junto al Monumento Mitad del Mundo, San Antonio de Pichincha. Construccion en madera y bahareque con vista al valle de Quito. Visita guiada al monumento y experiencias culturales indigenas incluidas.',
    'Cabana Luxemburgo junto al Monumento Mitad del Mundo. Experiencias culturales unicas.',
    'cabana', 3, 'cultural',
    'Ecuador', 'Pichincha', 'Quito',
    'San Antonio de Pichincha, 23 km norte de Quito',
    'Av. Manuel Cordova Galarza Km 23, San Antonio', '170350',
    '022394567', 'mitad@hotelluxemburgo.com',
    0.0022100, -78.4556800,
    '14:00', '11:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444010',
    'LUX-CUEA',
    'Apartamentos Luxemburgo El Barranco',
    'Apartamentos Hotel Luxemburgo con vista al Barranco del rio Tomebamba y las iglesias del Centro Historico de Cuenca. Suites con cocina completa, sala y balcon privado. A pasos de los mejores restaurantes y galerias de Cuenca.',
    'Apartamentos Luxemburgo con vista al Barranco y el rio Tomebamba, Cuenca.',
    'apartamento', NULL, 'cultural',
    'Ecuador', 'Azuay', 'Cuenca',
    'Sector El Barranco, Cuenca',
    'Calle Larga 8-44 y Calle Hermano Miguel', '010103',
    '072834901', 'barranco@hotelluxemburgo.com',
    -2.8991000, -79.0072000,
    '14:00', '11:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),

-- ============================================================
-- CATEGORIA: PLAYA (5)
-- ============================================================
(
    '44444444-4444-4444-4444-444444444011',
    'LUX-MAN',
    'Resort Luxemburgo Cocos Manta',
    'Resort 5 estrellas Hotel Luxemburgo frente al mar en Manta, la capital atunera del Ecuador. Playa privada, piscinas con agua de mar, spa de talasoterapia y restaurante de mariscos frescos. A 20 minutos del aeropuerto Eloy Alfaro.',
    'Resort Luxemburgo 5 estrellas frente al mar con playa privada en Manta, Manabi.',
    'resort', 5, 'playa',
    'Ecuador', 'Manabi', 'Manta',
    'Playa Murciélago, Manta',
    'Av. Flavio Reyes y Calle 18, sector Murciélago', '130101',
    '052622001', 'manta@hotelluxemburgo.com',
    -0.9463400, -80.7226700,
    '15:00', '12:00', FALSE, FALSE, TRUE, TRUE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444012',
    'LUX-CAN',
    'Villa Luxemburgo Canoa del Pacifico',
    'Villa boutique 3 estrellas Hotel Luxemburgo en la tranquila playa de Canoa, norte de Manabi. Cabanas de madera con hamacas y vista directa al Pacifico. Ideal para surf, parapente y observacion de cetaceos entre junio y septiembre.',
    'Villa Luxemburgo en Canoa, Manabi. Playa tranquila, surf y vida marina.',
    'villa', 3, 'playa',
    'Ecuador', 'Manabi', 'Canoa',
    'Playa de Canoa, Sucre, Manabi',
    'Calle Principal de Canoa y Av. del Pacifico', '130350',
    '052689001', 'canoa@hotelluxemburgo.com',
    0.4740000, -80.4536000,
    '14:00', '11:00', FALSE, FALSE, TRUE, TRUE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444013',
    'LUX-PLA',
    'Hostal Luxemburgo Playas del Sur',
    'Hostal 2 estrellas Hotel Luxemburgo en General Villamil Playas, el balneario mas popular de Guayas. Habitaciones sencillas a 50 metros de la playa. Ambiente familiar, alquiler de sombrillas y atardeceres frente al Pacifico.',
    'Hostal Luxemburgo 2 estrellas a 50 metros de la playa en General Villamil, Guayas.',
    'hostal', 2, 'playa',
    'Ecuador', 'Guayas', 'General Villamil',
    'Malecon de General Villamil Playas',
    'Av. Jaime Roldos Aguilera y Calle 5 Sur', '092350',
    '042846001', 'playas@hotelluxemburgo.com',
    -2.6264000, -80.3930000,
    '13:00', '11:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444014',
    'LUX-SAM',
    'Cabana Luxemburgo Same del Mar',
    'Cabana 3 estrellas Hotel Luxemburgo en Same, paraiso escondido de Esmeraldas. Playa de arena oscura volcanica, aguas tranquilas y palmeras. Construccion en madera de chonta, jardin tropical y kayaks incluidos para los huespedes.',
    'Cabana Luxemburgo en Same, Esmeraldas. Playa negra volcanica, naturaleza y tranquilidad.',
    'cabana', 3, 'playa',
    'Ecuador', 'Esmeraldas', 'Same',
    'Playa de Same, Muisne, Esmeraldas',
    'Via a Same Km 2, frente al mar', '080150',
    '062734001', 'same@hotelluxemburgo.com',
    0.5348000, -80.0526000,
    '14:00', '11:00', FALSE, FALSE, TRUE, TRUE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444015',
    'LUX-SAL',
    'Apartamentos Luxemburgo Salinas Bay',
    'Apartamentos Hotel Luxemburgo frente al mar en Salinas, la ciudad de playa mas exclusiva de Ecuador. Suites con vista directa a la bahia, balcon privado y cocina equipada. Acceso al club de playa y servicios de marina.',
    'Apartamentos Luxemburgo frente al mar en Salinas, la playa exclusiva de Santa Elena.',
    'apartamento', NULL, 'playa',
    'Ecuador', 'Santa Elena', 'Salinas',
    'Malecon de Salinas, Santa Elena',
    'Av. Enrique Gallo y Calle 34 NO', '240350',
    '042773001', 'salinas@hotelluxemburgo.com',
    -2.2148000, -80.9589000,
    '15:00', '12:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),

-- ============================================================
-- CATEGORIA: MONTANA (5)
-- ============================================================
(
    '44444444-4444-4444-4444-444444444016',
    'LUX-BAN',
    'Resort Luxemburgo Andes Retiro Banos',
    'Resort 4 estrellas Hotel Luxemburgo en Banos de Agua Santa, la puerta de la Amazonia. Vista al volcan Tungurahua, piscinas termales naturales, tirolesa sobre el rio Pastaza y deportes de aventura. Gastronomia de la Sierra central.',
    'Resort Luxemburgo 4 estrellas en Banos, Tungurahua. Aguas termales y vista al Tungurahua.',
    'resort', 4, 'montana',
    'Ecuador', 'Tungurahua', 'Banos',
    'Sector Runtun, sobre Banos de Agua Santa',
    'Via Runtun Km 4, Banos de Agua Santa', '180250',
    '032741001', 'banos@hotelluxemburgo.com',
    -1.3933000, -78.4241000,
    '15:00', '12:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444017',
    'LUX-COT',
    'Cabana Luxemburgo Cotopaxi Lodge',
    'Cabana 3 estrellas Hotel Luxemburgo al pie del volcan Cotopaxi, el mas alto del mundo en actividad. Construccion en piedra volcanica y madera andina a 3600 msnm. Trekking al refugio, avistamiento de condores y cabalgatas incluidas.',
    'Cabana Luxemburgo al pie del Cotopaxi a 3600 msnm. Trekking y avistamiento de condores.',
    'cabana', 3, 'montana',
    'Ecuador', 'Cotopaxi', 'Latacunga',
    'Parque Nacional Cotopaxi, acceso norte',
    'Via al Cotopaxi Km 35, ingreso Caspi', '050150',
    '032812001', 'cotopaxi@hotelluxemburgo.com',
    -0.6773000, -78.4369000,
    '14:00', '11:00', FALSE, FALSE, FALSE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444018',
    'LUX-BANV',
    'Villa Luxemburgo El Volcan Banos',
    'Villa 4 estrellas Hotel Luxemburgo con vista directa al Tungurahua desde Banos de Agua Santa. Terraza panoramica, banos de cajita con hierbas medicinales locales y mesa redonda con productos de la huerta propia. Maxima 10 personas.',
    'Villa Luxemburgo 4 estrellas con terraza panoramica al Tungurahua en Banos, Tungurahua.',
    'villa', 4, 'montana',
    'Ecuador', 'Tungurahua', 'Banos',
    'Cerro Ulba, Banos de Agua Santa',
    'Via a Ulba Km 2, Banos de Agua Santa', '180251',
    '032741002', 'volcan@hotelluxemburgo.com',
    -1.4012000, -78.4153000,
    '15:00', '12:00', FALSE, FALSE, FALSE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444019',
    'LUX-QUI',
    'Hostal Luxemburgo Quilotoa View',
    'Hostal 2 estrellas Hotel Luxemburgo con vista directa a la laguna craterica del Quilotoa a 3800 msnm. Construccion en adobe y paja toquilla. Punto de partida ideal para la Ruta del Quilotoa y trekking de multiples dias.',
    'Hostal Luxemburgo con vista a la laguna del Quilotoa a 3800 msnm, Cotopaxi.',
    'hostal', 2, 'montana',
    'Ecuador', 'Cotopaxi', 'Zumbahua',
    'Borde de la laguna Quilotoa, Pujili',
    'Comunidad Quilotoa, Pujili, Cotopaxi', '050350',
    '032814001', 'quilotoa@hotelluxemburgo.com',
    -0.8606000, -78.9000000,
    '13:00', '10:00', FALSE, FALSE, FALSE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444020',
    'LUX-RIO',
    'Hotel Luxemburgo Summit Riobamba',
    'Hotel 3 estrellas Hotel Luxemburgo en Riobamba con vista al Chimborazo, el punto mas cercano al sol. Base ideal para la ascension al volcan y expediciones en la sierra central. Cocina riobambena, quinua, cuy y chicha de jora.',
    'Hotel Luxemburgo 3 estrellas en Riobamba con vista al Chimborazo, puerta al coloso.',
    'hotel', 3, 'montana',
    'Ecuador', 'Chimborazo', 'Riobamba',
    'Norte de Riobamba, vista al Chimborazo',
    'Av. Daniel Leon Borja 44-15 y Uruguay', '060101',
    '032961001', 'riobamba@hotelluxemburgo.com',
    -1.6635000, -78.6543000,
    '14:00', '12:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),

-- ============================================================
-- CATEGORIA: AVENTURA (5)
-- ============================================================
(
    '44444444-4444-4444-4444-444444444021',
    'LUX-TEN',
    'Resort Luxemburgo Tena Jungle',
    'Resort 4 estrellas Hotel Luxemburgo a orillas del rio Tena, capital del rafting en Ecuador. Bungalows sobre el rio con mosquiteros, piscina natural, canopy y expediciones a comunidades kichwas. Gastronomia amazonica contemporanea.',
    'Resort Luxemburgo 4 estrellas en Tena, capital del rafting. Bungalows sobre el rio Napo.',
    'resort', 4, 'aventura',
    'Ecuador', 'Napo', 'Tena',
    'Orillas del rio Tena, Napo',
    'Via Tena - Misahualli Km 3', '150101',
    '062886001', 'tena@hotelluxemburgo.com',
    -0.9992000, -77.8101000,
    '15:00', '12:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444022',
    'LUX-YAS',
    'Cabana Luxemburgo Yasuni Wildlife',
    'Cabana 3 estrellas Hotel Luxemburgo en la reserva de biosfera Yasuni, Orellana. Plataforma elevada sobre la selva amazonica con avistamiento de mas de 600 especies de aves. Canoa por los rios, pesca artesanal y guias nativos waorani.',
    'Cabana Luxemburgo en el Yasuni, Orellana. Biodiversidad amazonica y guias nativos waorani.',
    'cabana', 3, 'aventura',
    'Ecuador', 'Orellana', 'Francisco de Orellana',
    'Reserva Biosfera Yasuni, Orellana',
    'Via fluvial desde Coca, 4h rio abajo', '220101',
    '062881001', 'yasuni@hotelluxemburgo.com',
    -0.4550000, -76.9870000,
    '15:00', '11:00', FALSE, FALSE, FALSE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444023',
    'LUX-MND',
    'Hostal Luxemburgo Mindo Cloud Forest',
    'Hostal 3 estrellas Hotel Luxemburgo en Mindo, la capital del avistamiento de aves de Ecuador. A 1200 msnm, en el bosque nublado con 500+ especies de aves. Tubing, mariposas, chocolate artesanal y senderos autoguiados.',
    'Hostal Luxemburgo en Mindo, capital del birdwatching. Bosque nublado con 500 especies.',
    'hostal', 3, 'aventura',
    'Ecuador', 'Pichincha', 'Mindo',
    'Sector El Descanso, Mindo',
    'Via Mindo Km 1, ingreso al pueblo', '170350',
    '022177001', 'mindo@hotelluxemburgo.com',
    0.0515000, -78.7726000,
    '13:00', '11:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444024',
    'LUX-NAP',
    'Apartamentos Luxemburgo Puerto Napo',
    'Apartamentos Hotel Luxemburgo a orillas del rio Napo, puerta a la Amazonia ecuatoriana. Suites equipadas con terraza sobre el rio, ideal para mochileros activos y familias que planean expediciones a Yasuni, Cuyabeno y comunidades kichwa.',
    'Apartamentos Luxemburgo a orillas del rio Napo, Tena. Puerta a la Amazonia ecuatoriana.',
    'apartamento', NULL, 'aventura',
    'Ecuador', 'Napo', 'Tena',
    'Puerto Napo, orillas del rio Napo',
    'Via Puerto Napo Km 0.5, Tena', '150102',
    '062887001', 'napo@hotelluxemburgo.com',
    -1.0372000, -77.7894000,
    '14:00', '11:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444025',
    'LUX-PUY',
    'Villa Luxemburgo Amazonica Puyo',
    'Villa 4 estrellas Hotel Luxemburgo en la ciudad de Puyo, corazon de la region Amazonica. Jardin de 2 hectareas con fauna silvestre, salon de eventos para turismo de grupos y conexion directa con comunidades shuar y achuar del Pastaza.',
    'Villa Luxemburgo 4 estrellas en Puyo, Pastaza. Jardin amazonico con fauna y cultura shuar.',
    'villa', 4, 'aventura',
    'Ecuador', 'Pastaza', 'Puyo',
    'Sector Puyo Garden, Pastaza',
    'Av. Alberto Zambrano Palacios y Calle Oriente', '160101',
    '032885001', 'puyo@hotelluxemburgo.com',
    -1.4920000, -77.9970000,
    '15:00', '12:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),

-- ============================================================
-- CATEGORIA: BIENESTAR (5)
-- ============================================================
(
    '44444444-4444-4444-4444-444444444026',
    'LUX-PAP',
    'Resort Luxemburgo Termas Papallacta',
    'Resort 5 estrellas Hotel Luxemburgo en Papallacta, el balneario termal de altura mas famoso de Ecuador a 3300 msnm. 23 piscinas termales de origen volcanico, spa de lodo andino, cromoterapia y dieta de desintoxicacion con superalimentos ecuatorianos.',
    'Resort Luxemburgo termal 5 estrellas en Papallacta a 3300 msnm. 23 piscinas de agua volcanica.',
    'resort', 5, 'bienestar',
    'Ecuador', 'Napo', 'Papallacta',
    'Papallacta, a 60 km de Quito via E45',
    'Via Interoceanica E45 Km 60, Papallacta', '150150',
    '062319001', 'papallacta@hotelluxemburgo.com',
    -0.3633000, -78.1481000,
    '16:00', '13:00', TRUE, TRUE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444027',
    'LUX-YUN',
    'Villa Luxemburgo Spa Yunguilla',
    'Villa 4 estrellas Hotel Luxemburgo en el microclima subtropical de Yunguilla, valle perdido de Azuay a 1800 msnm. Reconocida como destino de bienestar y sanacion. Yoga, meditacion, hidromasaje, tratamientos con plantas medicinales locales.',
    'Villa Luxemburgo spa en Yunguilla, Azuay. Valle subtropical, yoga y plantas medicinales.',
    'villa', 4, 'bienestar',
    'Ecuador', 'Azuay', 'Yunguilla',
    'Valle de Yunguilla, Santa Isabel, Azuay',
    'Via Yunguilla Km 5, Santa Isabel', '010350',
    '072289001', 'yunguilla@hotelluxemburgo.com',
    -3.2741000, -79.1793000,
    '15:00', '12:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444028',
    'LUX-BANT',
    'Hotel Luxemburgo Termal Banos',
    'Hotel 3 estrellas Hotel Luxemburgo especializado en bienestar termal en Banos de Agua Santa. Aguas sulfurosas directas del Tungurahua, bancos de vapor, fangoterapia y masajes con esencias de naranjilla y maracuya de la region.',
    'Hotel Luxemburgo termal 3 estrellas en Banos. Aguas sulfurosas del Tungurahua y fangoterapia.',
    'hotel', 3, 'bienestar',
    'Ecuador', 'Tungurahua', 'Banos',
    'Centro de Banos de Agua Santa',
    'Calle Thomas Halflants y Av. Ambato', '180250',
    '032741003', 'termal@hotelluxemburgo.com',
    -1.3966000, -78.4253000,
    '14:00', '12:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444029',
    'LUX-CUEW',
    'Apartamentos Luxemburgo Wellness Cuenca',
    'Apartamentos Hotel Luxemburgo de larga estadia enfocados en bienestar integral en Cuenca, ciudad con el mejor clima del mundo segun Forbes. Cocina funcional para dietas terapeuticas, acceso a parques, spa y medicos holisticos.',
    'Apartamentos Luxemburgo wellness en Cuenca, la ciudad con mejor clima del mundo.',
    'apartamento', NULL, 'bienestar',
    'Ecuador', 'Azuay', 'Cuenca',
    'Sector El Vergel, Cuenca',
    'Av. Fray Vicente Solano 12-43 y Calle Vargas Machuca', '010106',
    '072834902', 'wellness@hotelluxemburgo.com',
    -2.9094000, -79.0212000,
    '14:00', '11:00', FALSE, FALSE, TRUE, FALSE, FALSE, 'admin'
),
(
    '44444444-4444-4444-4444-444444444030',
    'LUX-MNDW',
    'Cabana Luxemburgo Mindo Wellness',
    'Cabana de bienestar 3 estrellas Hotel Luxemburgo en el bosque nublado de Mindo, Pichincha. Retiro de desconexion digital con yoga en plataforma sobre el rio, meditacion guiada, rituales de limpia ancestral y chocolate ceremonial.',
    'Cabana Luxemburgo de bienestar en Mindo. Retiro de desconexion, yoga y rituales ancestrales.',
    'cabana', 3, 'bienestar',
    'Ecuador', 'Pichincha', 'Mindo',
    'Sector Rio Mindo, bosque nublado',
    'Via Rio Nambillo Km 3, Mindo', '170351',
    '022177002', 'mindo.spa@hotelluxemburgo.com',
    0.0480000, -78.7814000,
    '15:00', '12:00', FALSE, FALSE, FALSE, FALSE, FALSE, 'admin'
);

-- ============================================================
-- 3. TIPOS DE HABITACION (prefijo GUID: 55xxxxxx-...)
--
--   55...001 = Suite Single   (1 adulto,   22 m2, King size)
--   55...002 = Suite Doble    (2 adultos,  32 m2, 2 Queen)
--   55...003 = Suite Familiar (2 ad+2 nin, 48 m2, King + sofa-cama)
--   55...004 = Suite Premium  (2 adultos,  65 m2, King lujo + jacuzzi)
--   55...005 = Suite Triple   (3 adultos,  40 m2, 3 camas individual)
--
-- Tipos globales: aplican a todas las sucursales LUX.
-- ============================================================
INSERT INTO alojamiento.tipo_habitacion (
    tipo_habitacion_guid, codigo_tipo_habitacion, nombre_tipo_habitacion,
    descripcion, capacidad_adultos, capacidad_ninos, capacidad_total,
    tipo_cama, area_m2, permite_eventos, permite_reserva_publica,
    creado_por_usuario
) VALUES
(
    '55555555-5555-5555-5555-555555555001',
    'TH-SINGLE', 'Suite Single',
    'Habitacion individual con cama King size, escritorio ejecutivo, minibar y bano privado con ducha de lluvia. Disenada por Hotel Luxemburgo para viajeros de negocios o turistas que buscan confort y privacidad. Incluye caja fuerte digital, Smart TV 43" y amenities de lujo.',
    1, 0, 1, '1 King size 200x200 cm', 22.00, FALSE, TRUE, 'admin'
),
(
    '55555555-5555-5555-5555-555555555002',
    'TH-DOBLE', 'Suite Doble',
    'Suite para dos personas con dos camas Queen size, sala compacta, escritorio y bano privado con tina. Decoracion con textiles y artesanias de Otavalo. Incluye minibar, Smart TV 50" y vista parcial a la ciudad. Perfecta para parejas o companeros de viaje de la cadena Hotel Luxemburgo.',
    2, 0, 2, '2 Queen size 160x200 cm', 32.00, FALSE, TRUE, 'admin'
),
(
    '55555555-5555-5555-5555-555555555003',
    'TH-FAMILIAR', 'Suite Familiar',
    'Suite de dos ambientes Hotel Luxemburgo para familias: dormitorio principal King size y sala con sofa-cama doble. Bano principal con tina y bano auxiliar con ducha. Frigobar doble y acceso a servicio de canguro coordinado en recepcion.',
    2, 2, 4, '1 King size + 1 Sofa-cama doble', 48.00, FALSE, TRUE, 'admin'
),
(
    '55555555-5555-5555-5555-555555555004',
    'TH-PREMIUM', 'Suite Premium',
    'La maxima expresion de lujo Hotel Luxemburgo. Pisos superiores con vista panoramica. Sala independiente, comedor, jacuzzi privado y ducha de vapor. Servicio de mayordomo 06h-22h, amenities premium, champagne de bienvenida y acceso al Club Lounge.',
    2, 0, 2, '1 King size 200x200 cm somier articulado', 65.00, TRUE, TRUE, 'admin'
),
(
    '55555555-5555-5555-5555-555555555005',
    'TH-TRIPLE', 'Suite Triple',
    'Suite Hotel Luxemburgo disenada para tres huespedes adultos con tres camas individuales articuladas, escritorios individuales y bano compartido amplio con ducha doble. Ideal para grupos de trabajo, amigos o familia sin ninos. Minibar compartido y Smart TV 50".',
    3, 0, 3, '3 camas individuales 90x200 cm', 40.00, FALSE, TRUE, 'admin'
);

-- ============================================================
-- 5. CATALOGO DE SERVICIOS (prefijo GUID: 88xxxxxx-...)
--
-- id_sucursal NOT NULL: cada servicio pertenece a una sucursal.
-- GUIDs 88...001 a 88...008 son referenciados por HotelLux_Stay.
--   88...001-004 (AME) --> LUX-UIO  (Quito)
--   88...005 SRV-HAB   --> LUX-GYE  (Guayaquil / cargo cc1001, cc1002 Ana)
--   88...006 SRV-LAV   --> LUX-GYE  (Guayaquil)
--   88...007 SRV-SPA   --> LUX-CUE  (Cuenca / cargo cc1003 Pedro, cc1004 Carlos)
--   88...008 SRV-TRANS --> LUX-UIO  (Quito)
--
-- Codigos CHAR(10): todos <= 10 caracteres.
-- ============================================================
INSERT INTO alojamiento.catalogo_servicios (
    catalogo_guid, id_sucursal, codigo_catalogo, nombre_catalogo,
    tipo_catalogo, categoria_catalogo, descripcion_catalogo,
    precio_base, aplica_iva, disponible_24h, hora_inicio, hora_fin,
    icono_url, creado_por_usuario
) VALUES

-- ---- LUX-UIO (Quito): 4 AME + 1 SRV -----------------------
(
    '88888888-8888-8888-8888-888888888001',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    'AME-WIFI', 'WiFi alta velocidad',
    'AME', 'Conectividad',
    'WiFi 100 Mbps gratuito en habitacion y areas comunes del Hotel Luxemburgo Quito. Red dedicada con soporte tecnico en recepcion las 24h.',
    0.00, FALSE, TRUE, NULL, NULL, 'icons/wifi.svg', 'admin'
),
(
    '88888888-8888-8888-8888-888888888002',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    'AME-AC', 'Aire acondicionado',
    'AME', 'Confort',
    'Climatizacion individual 16-28 C en el Hotel Luxemburgo Quito. Calefaccion para noches andinas frias de La Mariscal.',
    0.00, FALSE, TRUE, NULL, NULL, 'icons/ac.svg', 'admin'
),
(
    '88888888-8888-8888-8888-888888888003',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    'AME-TV', 'Smart TV 4K streaming',
    'AME', 'Entretenimiento',
    'TV LED 4K 50" con cable HD, Netflix, Amazon Prime y YouTube preconfigurados. Complemento digital de la experiencia Hotel Luxemburgo Quito.',
    0.00, FALSE, TRUE, NULL, NULL, 'icons/tv.svg', 'admin'
),
(
    '88888888-8888-8888-8888-888888888004',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    'AME-MINI', 'Minibar de cortesia',
    'AME', 'Alimentos y bebidas',
    'Minibar diario Hotel Luxemburgo Quito con agua San Luis, refrescos, Pilsener, snacks y chocolates Pacari. Disponible en Suite Doble, Familiar, Premium y Triple.',
    0.00, FALSE, TRUE, NULL, NULL, 'icons/minibar.svg', 'admin'
),
(
    '88888888-8888-8888-8888-888888888008',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    'SRV-TRANS', 'Traslado aeropuerto UIO',
    'SRV', 'Transporte',
    'Traslado privado ejecutivo Hotel Luxemburgo al aeropuerto Mariscal Sucre (UIO). Conductor bilingue y seguimiento de vuelo incluido.',
    25.00, TRUE, TRUE, NULL, NULL, 'icons/transfer.svg', 'admin'
),

-- ---- LUX-GYE (Guayaquil): 4 AME + 4 SRV ------------------
-- 88...005 y 88...006 conservan GUID (referenciados en Stay)
(
    '88888888-8888-8888-8888-888888888005',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    'SRV-HAB', 'Servicio a la habitacion',
    'SRV', 'Alimentos y bebidas',
    'Alimentos y bebidas 24h en el Hotel Luxemburgo Guayaquil. Menu completo con opciones ecuatorianas e internacionales. Entrega en 30 minutos garantizada.',
    15.00, TRUE, TRUE, NULL, NULL, 'icons/room-service.svg', 'admin'
),
(
    '88888888-8888-8888-8888-888888888006',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    'SRV-LAV', 'Lavanderia y planchado',
    'SRV', 'Limpieza y cuidado',
    'Lavanderia, secado y planchado Hotel Luxemburgo Guayaquil. Recogida antes 09h garantiza entrega el mismo dia habiles. Especialidad en prendas delicadas.',
    12.50, TRUE, FALSE, '08:00', '18:00', 'icons/laundry.svg', 'admin'
),
(
    gen_random_uuid(),
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    'GYE-WIFI', 'WiFi fibra optica GYE',
    'AME', 'Conectividad',
    'WiFi 200 Mbps fibra optica en habitacion y areas comunes del Hotel Luxemburgo Guayaquil. Optimo para streaming y videollamadas desde el Malecon 2000.',
    0.00, FALSE, TRUE, NULL, NULL, 'icons/wifi.svg', 'admin'
),
(
    gen_random_uuid(),
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    'GYE-AC', 'Climatizacion tropical GYE',
    'AME', 'Confort',
    'Sistema de aire acondicionado de alta eficiencia para el clima tropical del Hotel Luxemburgo Guayaquil. Control individual 18-26 C.',
    0.00, FALSE, TRUE, NULL, NULL, 'icons/ac.svg', 'admin'
),
(
    gen_random_uuid(),
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    'GYE-TV', 'Smart TV 4K GYE',
    'AME', 'Entretenimiento',
    'TV LED 4K 55" con cable HD y plataformas de streaming en el Hotel Luxemburgo Guayaquil. Vista al Malecon desde el mejor angulo.',
    0.00, FALSE, TRUE, NULL, NULL, 'icons/tv.svg', 'admin'
),
(
    gen_random_uuid(),
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    'GYE-MINI', 'Minibar tropical GYE',
    'AME', 'Alimentos y bebidas',
    'Minibar Hotel Luxemburgo Guayaquil con agua, jugos tropicales, cervezas artesanales de la Costa, snacks y frutas locales como cacao y maracuya.',
    0.00, FALSE, TRUE, NULL, NULL, 'icons/minibar.svg', 'admin'
),
(
    gen_random_uuid(),
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    'GYE-SPA', 'Spa Pacifico GYE',
    'SRV', 'Bienestar',
    'Sesion 60 min en el Spa Pacifico del Hotel Luxemburgo Guayaquil. Masaje relajante, envolturas de aloe vera costeno o exfoliacion de sal del Pacifico. Reserva con 2h de anticipacion.',
    48.00, TRUE, FALSE, '09:00', '20:00', 'icons/spa.svg', 'admin'
),
(
    gen_random_uuid(),
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    'GYE-TRAN', 'Traslado aeropuerto GYE',
    'SRV', 'Transporte',
    'Traslado privado Hotel Luxemburgo al aeropuerto Jose Joaquin de Olmedo (GYE). Vehiculo climatizado y conductor bilingue.',
    28.00, TRUE, TRUE, NULL, NULL, 'icons/transfer.svg', 'admin'
),

-- ---- LUX-CUE (Cuenca): 4 AME + 4 SRV ----------------------
-- 88...007 conserva GUID (referenciado en HotelLux_Stay)
(
    '88888888-8888-8888-8888-888888888007',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    'SRV-SPA', 'Spa Andes Wellness',
    'SRV', 'Bienestar',
    'Sesion 60 min en el Spa Andes Wellness del Hotel Luxemburgo Cuenca. Masaje relajante, barro andino o tratamiento facial con productos nativos de la Sierra Sur. Reserva con 2h minimo.',
    45.00, TRUE, FALSE, '09:00', '20:00', 'icons/spa.svg', 'admin'
),
(
    gen_random_uuid(),
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    'CUE-WIFI', 'WiFi alta velocidad CUE',
    'AME', 'Conectividad',
    'WiFi 100 Mbps gratuito en habitacion y areas comunes del Hotel Luxemburgo Cuenca. Senial reforzada en todos los pisos de la casona colonial.',
    0.00, FALSE, TRUE, NULL, NULL, 'icons/wifi.svg', 'admin'
),
(
    gen_random_uuid(),
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    'CUE-AC', 'Calefaccion andina CUE',
    'AME', 'Confort',
    'Calefaccion de piso radiante en habitaciones del Hotel Luxemburgo Cuenca. Ideal para las noches frias del Centro Historico Patrimonio UNESCO.',
    0.00, FALSE, TRUE, NULL, NULL, 'icons/ac.svg', 'admin'
),
(
    gen_random_uuid(),
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    'CUE-TV', 'Smart TV 4K CUE',
    'AME', 'Entretenimiento',
    'TV LED 4K 50" integrada en el mobiliario colonial del Hotel Luxemburgo Cuenca. Plataformas de streaming y canales locales e internacionales.',
    0.00, FALSE, TRUE, NULL, NULL, 'icons/tv.svg', 'admin'
),
(
    gen_random_uuid(),
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    'CUE-MINI', 'Minibar serrano CUE',
    'AME', 'Alimentos y bebidas',
    'Minibar Hotel Luxemburgo Cuenca con agua, jugos de mora y naranjilla, canelazo preparado, chocolates cuencanos y queso de hoja local.',
    0.00, FALSE, TRUE, NULL, NULL, 'icons/minibar.svg', 'admin'
),
(
    gen_random_uuid(),
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    'CUE-HAB', 'Servicio habitacion CUE',
    'SRV', 'Alimentos y bebidas',
    'Servicio a la habitacion Hotel Luxemburgo Cuenca con menu de la Sierra Sur. Mote pillo, caldo de gallina criolla, seco de pato y dulces cuencanos. Entrega en 35 minutos.',
    14.00, TRUE, FALSE, '07:00', '22:00', 'icons/room-service.svg', 'admin'
),
(
    gen_random_uuid(),
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    'CUE-LAV', 'Lavanderia CUE',
    'SRV', 'Limpieza y cuidado',
    'Lavado, secado y planchado artesanal Hotel Luxemburgo Cuenca. Especialidad en prendas delicadas. Entrega en 24h habiles.',
    11.00, TRUE, FALSE, '08:00', '17:00', 'icons/laundry.svg', 'admin'
),
(
    gen_random_uuid(),
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    'CUE-TRAN', 'Traslado aeropuerto CUE',
    'SRV', 'Transporte',
    'Traslado privado Hotel Luxemburgo Cuenca al aeropuerto Mariscal Lamar (CUE). Vehiculo ejecutivo y conductor con conocimiento del centro historico.',
    20.00, TRUE, TRUE, NULL, NULL, 'icons/transfer.svg', 'admin'
);


-- ============================================================
-- 6. RELACION N:M: tipo_habitacion <-> catalogo_servicios
--
-- TH-SINGLE  : WiFi, AC, TV              (sin minibar)
-- TH-DOBLE   : WiFi, AC, TV, Minibar
-- TH-FAMILIAR: WiFi, AC, TV, Minibar
-- TH-PREMIUM : WiFi, AC, TV, Minibar
-- TH-TRIPLE  : WiFi, AC, TV, Minibar
-- ============================================================

-- QUITO: AME-WIFI, AME-AC, AME-TV para TODOS los tipos
INSERT INTO alojamiento.tipo_habitacion_catalogo (id_tipo_habitacion, id_catalogo, creado_por_usuario)
SELECT th.id_tipo_habitacion, c.id_catalogo, 'admin'
FROM   alojamiento.tipo_habitacion th
CROSS  JOIN alojamiento.catalogo_servicios c
WHERE  c.codigo_catalogo IN ('AME-WIFI','AME-AC','AME-TV')
UNION ALL
-- QUITO: AME-MINI para Doble, Familiar, Premium y Triple
SELECT th.id_tipo_habitacion, c.id_catalogo, 'admin'
FROM   alojamiento.tipo_habitacion th
CROSS  JOIN alojamiento.catalogo_servicios c
WHERE  c.codigo_catalogo = 'AME-MINI'
  AND  th.codigo_tipo_habitacion IN ('TH-DOBLE','TH-FAMILIAR','TH-PREMIUM','TH-TRIPLE')
UNION ALL
-- GUAYAQUIL: GYE-WIFI, GYE-AC, GYE-TV para TODOS los tipos
SELECT th.id_tipo_habitacion, c.id_catalogo, 'admin'
FROM   alojamiento.tipo_habitacion th
CROSS  JOIN alojamiento.catalogo_servicios c
WHERE  c.codigo_catalogo IN ('GYE-WIFI','GYE-AC','GYE-TV')
UNION ALL
-- GUAYAQUIL: GYE-MINI para Doble, Familiar, Premium y Triple
SELECT th.id_tipo_habitacion, c.id_catalogo, 'admin'
FROM   alojamiento.tipo_habitacion th
CROSS  JOIN alojamiento.catalogo_servicios c
WHERE  c.codigo_catalogo = 'GYE-MINI'
  AND  th.codigo_tipo_habitacion IN ('TH-DOBLE','TH-FAMILIAR','TH-PREMIUM','TH-TRIPLE')
UNION ALL
-- CUENCA: CUE-WIFI, CUE-AC, CUE-TV para TODOS los tipos
SELECT th.id_tipo_habitacion, c.id_catalogo, 'admin'
FROM   alojamiento.tipo_habitacion th
CROSS  JOIN alojamiento.catalogo_servicios c
WHERE  c.codigo_catalogo IN ('CUE-WIFI','CUE-AC','CUE-TV')
UNION ALL
-- CUENCA: CUE-MINI para Doble, Familiar, Premium y Triple
SELECT th.id_tipo_habitacion, c.id_catalogo, 'admin'
FROM   alojamiento.tipo_habitacion th
CROSS  JOIN alojamiento.catalogo_servicios c
WHERE  c.codigo_catalogo = 'CUE-MINI'
  AND  th.codigo_tipo_habitacion IN ('TH-DOBLE','TH-FAMILIAR','TH-PREMIUM','TH-TRIPLE');


-- ============================================================
-- 7. HABITACIONES (prefijo GUID: 66xxxxxx-...)
--
-- Solo las 3 sucursales LUX principales tienen habitaciones
-- fisicas definidas en este seed. El resto puede agregarse
-- progresivamente via la API de accommodation-service.
--
-- GUIDs 66...001 a 66...010: referenciados por otras BDs.
-- GUIDs 66...011, 66...012, 66...013: Suite Triple nuevas.
--
-- LUX-UIO (Quito):
--   66...001 = 101 Single   p1  $80    DIS
--   66...002 = 102 Doble    p1  $120   DIS  <-- ref Reservation/Stay
--   66...003 = 201 Familiar p2  $180   DIS
--   66...004 = 301 Premium  p3  $250   DIS
--   66...011 = 103 Triple   p1  $150   DIS
--
-- LUX-GYE (Guayaquil):
--   66...005 = 101 Doble    p1  $130   DIS
--   66...006 = 102 Doble    p1  $130   DIS
--   66...007 = 201 Familiar p2  $190   OCU  <-- ref Stay (Ana Lopez)
--   66...012 = 103 Triple   p1  $160   DIS
--
-- LUX-CUE (Cuenca):
--   66...008 = 101 Single   p1  $75    DIS  <-- ref Stay (Pedro, Carlos)
--   66...009 = 102 Doble    p1  $115   DIS
--   66...010 = 201 Premium  p2  $230   DIS
--   66...013 = 103 Triple   p1  $130   DIS
-- ============================================================
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES
-- ---- LUX-UIO (Quito) ----------------------------------------
(
    '66666666-6666-6666-6666-666666666001',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 80.00,
    'Suite Single Hotel Luxemburgo Quito - p1, orientacion norte. Vista a jardines interiores. Tranquila y alejada de la calle.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666002',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 120.00,
    'Suite Doble Hotel Luxemburgo Quito - p1, orientacion sur. Vista parcial Av. 6 de Diciembre. Ideal parejas en negocios o turismo.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666003',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '201', 2, 4, 180.00,
    'Suite Familiar Hotel Luxemburgo Quito - p2, orientacion este. Vista a La Mariscal y valles de Quito. Dos ambientes completamente separados.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666004',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    '301', 3, 2, 250.00,
    'Suite Premium Hotel Luxemburgo Quito - p3, orientacion oeste. Vista panoramica al Pichincha y atardecer sobre Quito. La mas exclusiva.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666011',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-TRIPLE'),
    '103', 1, 3, 150.00,
    'Suite Triple Hotel Luxemburgo Quito - p1, orientacion norte. Tres camas individuales con escritorios separados. Ideal para equipos de trabajo o amigos.',
    'DIS', 'admin'
),
-- ---- LUX-GYE (Guayaquil) ------------------------------------
(
    '66666666-6666-6666-6666-666666666005',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 130.00,
    'Suite Doble Hotel Luxemburgo Guayaquil - p1, orientacion oeste. Vista directa al Malecon 2000 y rio Guayas. Mejor panoramica de la sucursal.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666006',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 130.00,
    'Suite Doble Hotel Luxemburgo Guayaquil - p1, orientacion este. Vista al centro urbano y cerro Santa Ana. Gemela a habitacion 101.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666007',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '201', 2, 4, 190.00,
    'Suite Familiar Hotel Luxemburgo Guayaquil - p2, orientacion oeste. Vista al Malecon y estero Salado. Balcon privado ideal para familias.',
    'OCU', 'admin'
),
(
    '66666666-6666-6666-6666-666666666012',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-TRIPLE'),
    '103', 1, 3, 160.00,
    'Suite Triple Hotel Luxemburgo Guayaquil - p1, orientacion este. Tres camas individuales con vista al cerro Santa Ana.',
    'DIS', 'admin'
),
-- ---- LUX-CUE (Cuenca) ----------------------------------------
(
    '66666666-6666-6666-6666-666666666008',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 75.00,
    'Suite Single Hotel Luxemburgo Cuenca - planta baja, patio interior. Piso de madera siglo XIX, techo artesonado y ventanas emplomadas.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666009',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 115.00,
    'Suite Doble Hotel Luxemburgo Cuenca - balcon de hierro forjado con vista al rio Tomebamba y bohios de Las Herrerias.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666010',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    '201', 2, 2, 230.00,
    'Suite Premium Hotel Luxemburgo Cuenca - p2, vista al Tomebamba y Catedral Nueva. Jacuzzi con vista historica, vigas y ceramica azuay.',
    'DIS', 'admin'
),
(
    '66666666-6666-6666-6666-666666666013',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-TRIPLE'),
    '103', 1, 3, 130.00,
    'Suite Triple Hotel Luxemburgo Cuenca - planta baja, patio colonial. Tres camas en caoba con elementos de la casona original.',
    'DIS', 'admin'
);


-- ============================================================
-- 8. TARIFAS (prefijo GUID: 77xxxxxx-...)
--
-- GUIDs 77...001 a 77...008 son referenciados por otras BDs.
-- GUIDs adicionales:
--   77...009 = TAR-UIO-TRIPLE-2026 (Quito   TH-TRIPLE  $150)
--   77...010 = TAR-GYE-TRIPLE-2026 (GYE     TH-TRIPLE  $160)
--   77...011 = TAR-CUE-DOBLE-2026  (Cuenca  TH-DOBLE   $115)
--   77...012 = TAR-CUE-TRIPLE-2026 (Cuenca  TH-TRIPLE  $130)
--
-- IVA 15% aplicado en todas las tarifas (Ecuador 2026).
-- ============================================================
INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES
-- ---- LUX-UIO Quito (5 tarifas) ------------------------------
(
    '77777777-7777-7777-7777-777777777001', 'TAR-UIO-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Hotel Luxemburgo Quito 2026', 'TODOS',
    '2026-01-01', '2026-12-31', 80.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777002', 'TAR-UIO-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Hotel Luxemburgo Quito 2026', 'TODOS',
    '2026-01-01', '2026-12-31', 120.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777003', 'TAR-UIO-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Hotel Luxemburgo Quito 2026', 'TODOS',
    '2026-01-01', '2026-12-31', 180.00, 15.00, 2, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777004', 'TAR-UIO-PREMIUM-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    'Tarifa Estandar Suite Premium Hotel Luxemburgo Quito 2026', 'TODOS',
    '2026-01-01', '2026-12-31', 250.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777009', 'TAR-UIO-TRIPLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-TRIPLE'),
    'Tarifa Estandar Suite Triple Hotel Luxemburgo Quito 2026', 'TODOS',
    '2026-01-01', '2026-12-31', 150.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
-- ---- LUX-GYE Guayaquil (3 tarifas) --------------------------
(
    '77777777-7777-7777-7777-777777777005', 'TAR-GYE-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Hotel Luxemburgo Guayaquil 2026', 'TODOS',
    '2026-01-01', '2026-12-31', 130.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777006', 'TAR-GYE-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Hotel Luxemburgo Guayaquil 2026', 'TODOS',
    '2026-01-01', '2026-12-31', 190.00, 15.00, 2, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777010', 'TAR-GYE-TRIPLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-TRIPLE'),
    'Tarifa Estandar Suite Triple Hotel Luxemburgo Guayaquil 2026', 'TODOS',
    '2026-01-01', '2026-12-31', 160.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
-- ---- LUX-CUE Cuenca (4 tarifas) -----------------------------
(
    '77777777-7777-7777-7777-777777777007', 'TAR-CUE-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Hotel Luxemburgo Cuenca 2026', 'TODOS',
    '2026-01-01', '2026-12-31', 75.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777011', 'TAR-CUE-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Hotel Luxemburgo Cuenca 2026', 'TODOS',
    '2026-01-01', '2026-12-31', 115.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777008', 'TAR-CUE-PREMIUM-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    'Tarifa Estandar Suite Premium Hotel Luxemburgo Cuenca 2026', 'TODOS',
    '2026-01-01', '2026-12-31', 230.00, 15.00, 1, NULL, TRUE, 1, 'admin'
),
(
    '77777777-7777-7777-7777-777777777012', 'TAR-CUE-TRIPLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUE'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-TRIPLE'),
    'Tarifa Estandar Suite Triple Hotel Luxemburgo Cuenca 2026', 'TODOS',
    '2026-01-01', '2026-12-31', 130.00, 15.00, 1, NULL, TRUE, 1, 'admin'
);


-- ============================================================
-- VERIFICACION FINAL
-- Resultados esperados:
--   Sucursales           : 30
--   Imagenes sucursal    : 60
--   Tipos habitacion     : 5
--   Imagenes tipo hab    : 10
--   Catalogo servicios   : 21  (5 UIO + 8 GYE + 8 CUE)
--   Tipo-hab catalogo    : 57  (3 sucursales x tipos x amenidades)
--   Habitaciones fisicas : 13
--   Tarifas vigentes     : 12
-- ============================================================
SELECT 'Sucursales:              ' || COUNT(*)::text FROM alojamiento.sucursal;
SELECT 'Imagenes sucursal:       ' || COUNT(*)::text FROM alojamiento.sucursal_imagen;
SELECT 'Tipos de habitacion:     ' || COUNT(*)::text FROM alojamiento.tipo_habitacion;
SELECT 'Imagenes tipo habitacion:' || COUNT(*)::text FROM alojamiento.tipo_habitacion_imagen;
SELECT 'Catalogo servicios:      ' || COUNT(*)::text FROM alojamiento.catalogo_servicios;
SELECT 'Tipo-hab catalogo (N:M): ' || COUNT(*)::text FROM alojamiento.tipo_habitacion_catalogo;
SELECT 'Habitaciones fisicas:    ' || COUNT(*)::text FROM alojamiento.habitacion;
SELECT 'Tarifas vigentes:        ' || COUNT(*)::text FROM alojamiento.tarifa;

-- Todas las sucursales deben tener codigo LUX-
SELECT codigo_sucursal, nombre_sucursal, tipo_alojamiento, categoria_viaje
FROM   alojamiento.sucursal
ORDER  BY categoria_viaje, tipo_alojamiento;

-- Verificacion: ningun codigo sin prefijo LUX
SELECT COUNT(*) AS codigos_sin_lux
FROM   alojamiento.sucursal
WHERE  codigo_sucursal NOT LIKE 'LUX-%';


-- ============================================================
-- VERIFICACION FINAL
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
    trim(th.nombre_tipo_habitacion)             AS tipo,
    COUNT(h.id_habitacion)                      AS habitaciones,
    MIN(h.precio_base)                          AS precio_base_usd,
    t.precio_por_noche                          AS tarifa_noche_usd,
    ROUND(t.precio_por_noche * 1.15, 2)        AS con_iva_usd,
    t.min_noches
FROM       alojamiento.habitacion h
JOIN       alojamiento.sucursal          s  ON s.id_sucursal        = h.id_sucursal
JOIN       alojamiento.tipo_habitacion   th ON th.id_tipo_habitacion = h.id_tipo_habitacion
LEFT JOIN  alojamiento.tarifa            t  ON t.id_sucursal        = h.id_sucursal
                                          AND t.id_tipo_habitacion  = h.id_tipo_habitacion
                                          AND t.estado_tarifa       = 'ACT'
GROUP BY   s.nombre_sucursal, th.nombre_tipo_habitacion,
           t.precio_por_noche, t.min_noches
ORDER BY   s.nombre_sucursal, th.nombre_tipo_habitacion;





-- --- LUX-UIOH: Hostal Luxemburgo La Mariscal ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000014',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIOH'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 55.00,
    'Suite Single Hostal Luxemburgo La Mariscal - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000015',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIOH'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 90.00,
    'Suite Doble Hostal Luxemburgo La Mariscal - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-GYEA: Apartamentos Luxemburgo Kennedy Norte ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000016',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYEA'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 90.00,
    'Suite Doble Apartamentos Luxemburgo Kennedy Norte - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000017',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYEA'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 135.00,
    'Suite Familiar Apartamentos Luxemburgo Kennedy Norte - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-UIOR: Resort Luxemburgo Mariscal Quito ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000018',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIOR'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 90.00,
    'Suite Doble Resort Luxemburgo Mariscal Quito - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000019',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIOR'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 135.00,
    'Suite Familiar Resort Luxemburgo Mariscal Quito - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000020',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIOR'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    '201', 2, 2, 290.00,
    'Suite Premium Resort Luxemburgo Mariscal Quito - piso 2, hab 201. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-IBR: Villa Luxemburgo Colonial Ibarra ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000021',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-IBR'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 80.00,
    'Suite Doble Villa Luxemburgo Colonial Ibarra - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000022',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-IBR'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 120.00,
    'Suite Familiar Villa Luxemburgo Colonial Ibarra - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-CUEH: Hostal Luxemburgo Patrimonial Cuenca ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000023',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUEH'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 50.00,
    'Suite Single Hostal Luxemburgo Patrimonial Cuenca - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000024',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUEH'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 80.00,
    'Suite Doble Hostal Luxemburgo Patrimonial Cuenca - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-MIT: Cabana Luxemburgo Mitad del Mundo ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000025',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MIT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 50.00,
    'Suite Single Cabana Luxemburgo Mitad del Mundo - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MIT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 80.00,
    'Suite Doble Cabana Luxemburgo Mitad del Mundo - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-CUEA: Apartamentos Luxemburgo El Barranco ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000027',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUEA'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 80.00,
    'Suite Doble Apartamentos Luxemburgo El Barranco - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000028',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUEA'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 120.00,
    'Suite Familiar Apartamentos Luxemburgo El Barranco - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-MAN: Resort Luxemburgo Cocos Manta ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000029',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 110.00,
    'Suite Doble Resort Luxemburgo Cocos Manta - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000030',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 165.00,
    'Suite Familiar Resort Luxemburgo Cocos Manta - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000031',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    '201', 2, 2, 80.00,
    'Suite Premium Resort Luxemburgo Cocos Manta - piso 2, hab 201. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-CAN: Villa Luxemburgo Canoa del Pacifico ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000032',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 110.00,
    'Suite Doble Villa Luxemburgo Canoa del Pacifico - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000033',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 165.00,
    'Suite Familiar Villa Luxemburgo Canoa del Pacifico - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-PLA: Hostal Luxemburgo Playas del Sur ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000034',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PLA'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 50.00,
    'Suite Single Hostal Luxemburgo Playas del Sur - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000035',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PLA'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 110.00,
    'Suite Doble Hostal Luxemburgo Playas del Sur - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-SAM: Cabana Luxemburgo Same del Mar ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000036',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-SAM'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 50.00,
    'Suite Single Cabana Luxemburgo Same del Mar - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000037',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-SAM'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 110.00,
    'Suite Doble Cabana Luxemburgo Same del Mar - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-SAL: Apartamentos Luxemburgo Salinas Bay ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000038',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-SAL'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 110.00,
    'Suite Doble Apartamentos Luxemburgo Salinas Bay - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000039',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-SAL'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 165.00,
    'Suite Familiar Apartamentos Luxemburgo Salinas Bay - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-BAN: Resort Luxemburgo Andes Retiro Banos ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000040',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 95.00,
    'Suite Doble Resort Luxemburgo Andes Retiro Banos - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000041',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 145.00,
    'Suite Familiar Resort Luxemburgo Andes Retiro Banos - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000042',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    '201', 2, 2, 150.00,
    'Suite Premium Resort Luxemburgo Andes Retiro Banos - piso 2, hab 201. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-COT: Cabana Luxemburgo Cotopaxi Lodge ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000043',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-COT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 55.00,
    'Suite Single Cabana Luxemburgo Cotopaxi Lodge - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000044',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-COT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 95.00,
    'Suite Doble Cabana Luxemburgo Cotopaxi Lodge - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-BANV: Villa Luxemburgo El Volcan Banos ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000045',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BANV'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 95.00,
    'Suite Doble Villa Luxemburgo El Volcan Banos - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000046',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BANV'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 145.00,
    'Suite Familiar Villa Luxemburgo El Volcan Banos - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-QUI: Hostal Luxemburgo Quilotoa View ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000047',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-QUI'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 55.00,
    'Suite Single Hostal Luxemburgo Quilotoa View - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000048',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-QUI'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 95.00,
    'Suite Doble Hostal Luxemburgo Quilotoa View - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-RIO: Hotel Luxemburgo Summit Riobamba ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000049',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-RIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 55.00,
    'Suite Single Hotel Luxemburgo Summit Riobamba - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000050',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-RIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 95.00,
    'Suite Doble Hotel Luxemburgo Summit Riobamba - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000051',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-RIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    '201', 2, 2, 150.00,
    'Suite Premium Hotel Luxemburgo Summit Riobamba - piso 2, hab 201. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-TEN: Resort Luxemburgo Tena Jungle ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000052',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-TEN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 95.00,
    'Suite Doble Resort Luxemburgo Tena Jungle - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000053',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-TEN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 145.00,
    'Suite Familiar Resort Luxemburgo Tena Jungle - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000054',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-TEN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    '201', 2, 2, 80.00,
    'Suite Premium Resort Luxemburgo Tena Jungle - piso 2, hab 201. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-YAS: Cabana Luxemburgo Yasuni Wildlife ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000055',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-YAS'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 60.00,
    'Suite Single Cabana Luxemburgo Yasuni Wildlife - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000056',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-YAS'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 95.00,
    'Suite Doble Cabana Luxemburgo Yasuni Wildlife - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-MND: Hostal Luxemburgo Mindo Cloud Forest ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000057',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MND'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 60.00,
    'Suite Single Hostal Luxemburgo Mindo Cloud Forest - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000058',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MND'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 95.00,
    'Suite Doble Hostal Luxemburgo Mindo Cloud Forest - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-NAP: Apartamentos Luxemburgo Puerto Napo ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000059',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-NAP'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 95.00,
    'Suite Doble Apartamentos Luxemburgo Puerto Napo - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000060',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-NAP'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 145.00,
    'Suite Familiar Apartamentos Luxemburgo Puerto Napo - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-PUY: Villa Luxemburgo Amazonica Puyo ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000061',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PUY'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 95.00,
    'Suite Doble Villa Luxemburgo Amazonica Puyo - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000062',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PUY'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 145.00,
    'Suite Familiar Villa Luxemburgo Amazonica Puyo - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-PAP: Resort Luxemburgo Termas Papallacta ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000063',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PAP'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 105.00,
    'Suite Doble Resort Luxemburgo Termas Papallacta - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000064',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PAP'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 155.00,
    'Suite Familiar Resort Luxemburgo Termas Papallacta - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000065',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PAP'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    '201', 2, 2, 290.00,
    'Suite Premium Resort Luxemburgo Termas Papallacta - piso 2, hab 201. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-YUN: Villa Luxemburgo Spa Yunguilla ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000066',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-YUN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 105.00,
    'Suite Doble Villa Luxemburgo Spa Yunguilla - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000067',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-YUN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 155.00,
    'Suite Familiar Villa Luxemburgo Spa Yunguilla - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-BANT: Hotel Luxemburgo Termal Banos ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000068',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BANT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 65.00,
    'Suite Single Hotel Luxemburgo Termal Banos - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000069',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BANT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 105.00,
    'Suite Doble Hotel Luxemburgo Termal Banos - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000070',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BANT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    '201', 2, 2, 290.00,
    'Suite Premium Hotel Luxemburgo Termal Banos - piso 2, hab 201. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-CUEW: Apartamentos Luxemburgo Wellness Cuenca ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000071',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUEW'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '101', 1, 2, 105.00,
    'Suite Doble Apartamentos Luxemburgo Wellness Cuenca - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000072',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUEW'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    '102', 1, 4, 155.00,
    'Suite Familiar Apartamentos Luxemburgo Wellness Cuenca - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);

-- --- LUX-MNDW: Cabana Luxemburgo Mindo Wellness ---
INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000073',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MNDW'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    '101', 1, 1, 65.00,
    'Suite Single Cabana Luxemburgo Mindo Wellness - piso 1, hab 101. Disponible para reservas 2026.',
    'DIS', 'admin'
);

INSERT INTO alojamiento.habitacion (
    habitacion_guid, id_sucursal, id_tipo_habitacion,
    numero_habitacion, piso, capacidad_habitacion, precio_base,
    descripcion_habitacion, estado_habitacion, creado_por_usuario
) VALUES (
    '66666666-6666-6666-6666-000000000074',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MNDW'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    '102', 1, 2, 105.00,
    'Suite Doble Cabana Luxemburgo Mindo Wellness - piso 1, hab 102. Disponible para reservas 2026.',
    'DIS', 'admin'
);


-- ============================================================
-- TARIFAS 2026 (vigentes todo el año, permite_portal_publico)
-- Continuacion desde 77...012 (ya existentes en BD02)
-- ============================================================

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000013', 'TAR-UIOH-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIOH'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Hostal Luxemburgo La Mariscal 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    55.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000014', 'TAR-UIOH-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIOH'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Hostal Luxemburgo La Mariscal 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    90.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000015', 'TAR-GYEA-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYEA'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Apartamentos Luxemburgo Kennedy Norte 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    90.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000016', 'TAR-GYEA-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-GYEA'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Apartamentos Luxemburgo Kennedy Norte 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    135.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000017', 'TAR-UIOR-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIOR'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Resort Luxemburgo Mariscal Quito 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    90.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000018', 'TAR-UIOR-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIOR'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Resort Luxemburgo Mariscal Quito 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    135.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000019', 'TAR-UIOR-PREMIUM-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-UIOR'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    'Tarifa Estandar Suite Premium Resort Luxemburgo Mariscal Quito 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    290.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000020', 'TAR-IBR-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-IBR'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Villa Luxemburgo Colonial Ibarra 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    80.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000021', 'TAR-IBR-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-IBR'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Villa Luxemburgo Colonial Ibarra 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    120.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000022', 'TAR-CUEH-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUEH'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Hostal Luxemburgo Patrimonial Cuenca 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    50.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000023', 'TAR-CUEH-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUEH'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Hostal Luxemburgo Patrimonial Cuenca 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    80.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000024', 'TAR-MIT-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MIT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Cabana Luxemburgo Mitad del Mundo 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    50.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000025', 'TAR-MIT-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MIT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Cabana Luxemburgo Mitad del Mundo 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    80.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000026', 'TAR-CUEA-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUEA'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Apartamentos Luxemburgo El Barranco 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    80.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000027', 'TAR-CUEA-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUEA'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Apartamentos Luxemburgo El Barranco 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    120.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000028', 'TAR-MAN-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Resort Luxemburgo Cocos Manta 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    110.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000029', 'TAR-MAN-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Resort Luxemburgo Cocos Manta 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    165.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000030', 'TAR-MAN-PREMIUM-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    'Tarifa Estandar Suite Premium Resort Luxemburgo Cocos Manta 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    80.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000031', 'TAR-CAN-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Villa Luxemburgo Canoa del Pacifico 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    110.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000032', 'TAR-CAN-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Villa Luxemburgo Canoa del Pacifico 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    165.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000033', 'TAR-PLA-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PLA'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Hostal Luxemburgo Playas del Sur 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    50.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000034', 'TAR-PLA-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PLA'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Hostal Luxemburgo Playas del Sur 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    110.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000035', 'TAR-SAM-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-SAM'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Cabana Luxemburgo Same del Mar 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    50.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000036', 'TAR-SAM-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-SAM'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Cabana Luxemburgo Same del Mar 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    110.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000037', 'TAR-SAL-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-SAL'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Apartamentos Luxemburgo Salinas Bay 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    110.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000038', 'TAR-SAL-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-SAL'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Apartamentos Luxemburgo Salinas Bay 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    165.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000039', 'TAR-BAN-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Resort Luxemburgo Andes Retiro Banos 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    95.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000040', 'TAR-BAN-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Resort Luxemburgo Andes Retiro Banos 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    145.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000041', 'TAR-BAN-PREMIUM-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BAN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    'Tarifa Estandar Suite Premium Resort Luxemburgo Andes Retiro Banos 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    150.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000042', 'TAR-COT-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-COT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Cabana Luxemburgo Cotopaxi Lodge 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    55.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000043', 'TAR-COT-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-COT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Cabana Luxemburgo Cotopaxi Lodge 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    95.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000044', 'TAR-BANV-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BANV'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Villa Luxemburgo El Volcan Banos 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    95.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000045', 'TAR-BANV-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BANV'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Villa Luxemburgo El Volcan Banos 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    145.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000046', 'TAR-QUI-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-QUI'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Hostal Luxemburgo Quilotoa View 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    55.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000047', 'TAR-QUI-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-QUI'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Hostal Luxemburgo Quilotoa View 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    95.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000048', 'TAR-RIO-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-RIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Hotel Luxemburgo Summit Riobamba 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    55.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000049', 'TAR-RIO-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-RIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Hotel Luxemburgo Summit Riobamba 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    95.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000050', 'TAR-RIO-PREMIUM-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-RIO'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    'Tarifa Estandar Suite Premium Hotel Luxemburgo Summit Riobamba 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    150.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000051', 'TAR-TEN-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-TEN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Resort Luxemburgo Tena Jungle 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    95.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000052', 'TAR-TEN-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-TEN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Resort Luxemburgo Tena Jungle 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    145.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000053', 'TAR-TEN-PREMIUM-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-TEN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    'Tarifa Estandar Suite Premium Resort Luxemburgo Tena Jungle 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    80.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000054', 'TAR-YAS-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-YAS'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Cabana Luxemburgo Yasuni Wildlife 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    60.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000055', 'TAR-YAS-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-YAS'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Cabana Luxemburgo Yasuni Wildlife 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    95.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000056', 'TAR-MND-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MND'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Hostal Luxemburgo Mindo Cloud Forest 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    60.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000057', 'TAR-MND-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MND'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Hostal Luxemburgo Mindo Cloud Forest 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    95.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000058', 'TAR-NAP-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-NAP'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Apartamentos Luxemburgo Puerto Napo 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    95.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000059', 'TAR-NAP-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-NAP'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Apartamentos Luxemburgo Puerto Napo 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    145.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000060', 'TAR-PUY-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PUY'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Villa Luxemburgo Amazonica Puyo 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    95.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000061', 'TAR-PUY-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PUY'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Villa Luxemburgo Amazonica Puyo 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    145.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000062', 'TAR-PAP-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PAP'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Resort Luxemburgo Termas Papallacta 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    105.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000063', 'TAR-PAP-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PAP'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Resort Luxemburgo Termas Papallacta 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    155.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000064', 'TAR-PAP-PREMIUM-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-PAP'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    'Tarifa Estandar Suite Premium Resort Luxemburgo Termas Papallacta 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    290.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000065', 'TAR-YUN-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-YUN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Villa Luxemburgo Spa Yunguilla 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    105.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000066', 'TAR-YUN-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-YUN'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Villa Luxemburgo Spa Yunguilla 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    155.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000067', 'TAR-BANT-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BANT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Hotel Luxemburgo Termal Banos 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    65.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000068', 'TAR-BANT-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BANT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Hotel Luxemburgo Termal Banos 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    105.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000069', 'TAR-BANT-PREMIUM-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-BANT'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-PREMIUM'),
    'Tarifa Estandar Suite Premium Hotel Luxemburgo Termal Banos 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    290.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000070', 'TAR-CUEW-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUEW'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Apartamentos Luxemburgo Wellness Cuenca 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    105.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000071', 'TAR-CUEW-FAMILIAR-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-CUEW'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-FAMILIAR'),
    'Tarifa Estandar Suite Familiar Apartamentos Luxemburgo Wellness Cuenca 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    155.00, 15.00,
    2, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000072', 'TAR-MNDW-SINGLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MNDW'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-SINGLE'),
    'Tarifa Estandar Suite Single Cabana Luxemburgo Mindo Wellness 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    65.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);

INSERT INTO alojamiento.tarifa (
    tarifa_guid, codigo_tarifa,
    id_sucursal, id_tipo_habitacion,
    nombre_tarifa, canal_tarifa,
    fecha_inicio, fecha_fin,
    precio_por_noche, porcentaje_iva,
    min_noches, max_noches, permite_portal_publico, prioridad,
    creado_por_usuario
) VALUES (
    '77777777-7777-7777-7777-000000000073', 'TAR-MNDW-DOBLE-2026',
    (SELECT id_sucursal FROM alojamiento.sucursal WHERE codigo_sucursal = 'LUX-MNDW'),
    (SELECT id_tipo_habitacion FROM alojamiento.tipo_habitacion WHERE codigo_tipo_habitacion = 'TH-DOBLE'),
    'Tarifa Estandar Suite Doble Cabana Luxemburgo Mindo Wellness 2026', 'TODOS',
    '2026-01-01', '2026-12-31',
    105.00, 15.00,
    1, NULL, TRUE, 1,
    'admin'
);


-- ============================================================
-- VERIFICACION POST-CARGA
-- ============================================================
SELECT
    s.codigo_sucursal,
    s.nombre_sucursal,
    COUNT(DISTINCT h.id_habitacion)   AS habitaciones_dis,
    COUNT(DISTINCT t.id_tarifa)        AS tarifas_vigentes
FROM      alojamiento.sucursal s
LEFT JOIN alojamiento.habitacion h
       ON h.id_sucursal = s.id_sucursal
      AND h.estado_habitacion = 'DIS'
      AND h.es_eliminado = FALSE
LEFT JOIN alojamiento.tarifa t
       ON t.id_sucursal = s.id_sucursal
      AND t.permite_portal_publico = TRUE
      AND t.es_eliminado = FALSE
      AND '2026-06-01'::date BETWEEN t.fecha_inicio AND t.fecha_fin
WHERE  s.es_eliminado = FALSE
GROUP  BY s.codigo_sucursal, s.nombre_sucursal
ORDER  BY s.codigo_sucursal;