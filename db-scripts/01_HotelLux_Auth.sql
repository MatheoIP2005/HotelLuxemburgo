-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio AUTH
-- Base de datos: HotelLux_Auth
-- Motor: PostgreSQL 18
-- Version: 2.0
--
-- DEPENDENCIAS: ninguna (es la primera BD a crear)
--
-- CONTENIDO:
--   Schema: seguridad
--   Tablas: rol, usuario_app, usuarios_roles
--   Datos semilla:
--     2 roles  : ADMIN (CRUD completo), VENDEDOR (sin DELETE)
--     2 usuarios: admin / vendedor
--     2 asignaciones rol-usuario
--
-- INSTRUCCIONES DE EJECUCION EN pgAdmin:
--   1. Click derecho sobre 'Databases' -> Create -> Database...
--      Database: HotelLux_Auth
--      Owner: postgres
--      Encoding: UTF8
--      Click Save.
--   2. Click derecho sobre la BD HotelLux_Auth -> Query Tool
--   3. Abrir este archivo (File -> Open) y ejecutar (F5).
--   4. Verificar al final los SELECT de conteo retornen
--      2 roles, 2 usuarios, 2 asignaciones.
--
-- CONTRASENAS (BCrypt cost 11, listas para usar):
--   admin    -> admin1234
--   vendedor -> vendedor1234
-- ============================================================


-- ============================================================
-- SCHEMA
-- ============================================================
CREATE SCHEMA IF NOT EXISTS seguridad;


-- ============================================================
-- TABLA: seguridad.rol
-- ============================================================
CREATE TABLE seguridad.rol (
    id_rol                   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    rol_guid                 UUID NOT NULL DEFAULT gen_random_uuid(),
    nombre_rol               VARCHAR(50)  NOT NULL,
    descripcion_rol          VARCHAR(250) NULL,
    estado_rol               CHAR(3)      NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    activo                   BOOLEAN      NOT NULL DEFAULT TRUE,
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    motivo_inhabilitacion    VARCHAR(250) NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100) NOT NULL,
    modificado_por_usuario   VARCHAR(100) NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          VARCHAR(45)  NULL,
    CONSTRAINT uq_rol_guid    UNIQUE (rol_guid),
    CONSTRAINT uq_rol_nombre  UNIQUE (nombre_rol),
    CONSTRAINT chk_rol_estado CHECK (estado_rol IN ('ACT','INA'))
);


-- ============================================================
-- TABLA: seguridad.usuario_app
-- ============================================================
CREATE TABLE seguridad.usuario_app (
    id_usuario               INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    usuario_guid             UUID         NOT NULL DEFAULT gen_random_uuid(),
    cliente_guid             UUID         NULL,
    username                 VARCHAR(50)  NOT NULL,
    correo                   VARCHAR(120) NOT NULL,
    nombres                  VARCHAR(120) NOT NULL,
    apellidos                VARCHAR(120) NULL,
    password_hash            VARCHAR(500) NOT NULL,
    password_salt            VARCHAR(250) NOT NULL,
    estado_usuario           CHAR(3)      NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    activo                   BOOLEAN      NOT NULL DEFAULT TRUE,
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    motivo_inhabilitacion    VARCHAR(250) NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100) NOT NULL,
    modificado_por_usuario   VARCHAR(100) NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          VARCHAR(45)  NULL,
    CONSTRAINT uq_usuario_app_guid     UNIQUE (usuario_guid),
    CONSTRAINT uq_usuario_app_username UNIQUE (username),
    CONSTRAINT uq_usuario_app_correo   UNIQUE (correo),
    CONSTRAINT chk_usuario_app_estado  CHECK  (estado_usuario IN ('ACT','INA','BLO'))
);


-- ============================================================
-- TABLA: seguridad.usuarios_roles (N:M)
-- ============================================================
CREATE TABLE seguridad.usuarios_roles (
    id_usuario_rol           INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_usuario               INT          NOT NULL,
    id_rol                   INT          NOT NULL,
    estado_usuario_rol       CHAR(3)      NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    activo                   BOOLEAN      NOT NULL DEFAULT TRUE,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       VARCHAR(100) NOT NULL,
    modificado_por_usuario   VARCHAR(100) NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          VARCHAR(45)  NULL,
    CONSTRAINT uq_usuarios_roles UNIQUE (id_usuario, id_rol),
    CONSTRAINT fk_usuarios_roles_usuario FOREIGN KEY (id_usuario)
        REFERENCES seguridad.usuario_app(id_usuario) ON DELETE CASCADE,
    CONSTRAINT fk_usuarios_roles_rol FOREIGN KEY (id_rol)
        REFERENCES seguridad.rol(id_rol) ON DELETE CASCADE,
    CONSTRAINT chk_usuarios_roles_estado CHECK (estado_usuario_rol IN ('ACT','INA'))
);


-- ============================================================
-- INDICES DE APOYO
-- ============================================================
CREATE INDEX ix_usuario_app_estado
    ON seguridad.usuario_app(estado_usuario, activo, correo);

CREATE INDEX ix_usuario_app_cliente
    ON seguridad.usuario_app(cliente_guid)
    WHERE cliente_guid IS NOT NULL;

CREATE INDEX ix_rol_estado
    ON seguridad.rol(estado_rol, activo);


-- ============================================================
-- DATOS SEMILLA
--
-- ESQUEMA DE GUIDs:
--   Prefijo 22xxxxxx-... = roles    (rol_guid)
--   Prefijo 21xxxxxx-... = usuarios (usuario_guid)
-- ============================================================


-- ============================================================
-- ROLES
--
-- ADMIN   -> puede hacer CREATE, READ, UPDATE y DELETE
-- VENDEDOR -> puede hacer CREATE, READ y UPDATE (sin DELETE)
-- ============================================================
INSERT INTO seguridad.rol (
    rol_guid,
    nombre_rol,
    descripcion_rol,
    creado_por_usuario
) VALUES
(
    '22222222-2222-2222-2222-222222222001',
    'ADMIN',
    'Administrador general. Permisos: CREATE, READ, UPDATE, DELETE sobre todos los recursos del sistema.',
    'system'
),
(
    '22222222-2222-2222-2222-222222222002',
    'VENDEDOR',
    'Personal de recepcion y ventas. Permisos: CREATE, READ, UPDATE. No puede eliminar registros.',
    'system'
);


-- ============================================================
-- USUARIOS
--
-- Contrasenas hasheadas con BCrypt cost 11:
--   admin    -> admin1234
--   vendedor -> vendedor1234
-- ============================================================
INSERT INTO seguridad.usuario_app (
    usuario_guid,
    cliente_guid,
    username,
    correo,
    nombres,
    apellidos,
    password_hash,
    password_salt,
    creado_por_usuario
) VALUES
(
    '21111111-1111-1111-1111-111111111001',
    NULL,
    'admin',
    'admin@hotelluxemburgo.com',
    'Administrador',
    'Sistema',
    '$2b$11$v94uNRF3DUsZz7ooOGUB7OoffuU2BfDumNiweoRRA8jUIcXPplm9m',
    '$2b$11$v94uNRF3DUsZz7ooOGUB7O',
    'system'
),
(
    '21111111-1111-1111-1111-111111111002',
    NULL,
    'vendedor',
    'vendedor@hotelluxemburgo.com',
    'Vendedor',
    'Recepcion',
    '$2b$11$MDtqAiMfwhca7Upp4vBRI.x5wEU0W5j6DuukLXYFwHQBGbmkOqfRC',
    '$2b$11$MDtqAiMfwhca7Upp4vBRI.',
    'system'
);


-- ============================================================
-- ASIGNACION DE ROLES A USUARIOS
-- ============================================================
INSERT INTO seguridad.usuarios_roles (
    id_usuario,
    id_rol,
    creado_por_usuario
)
SELECT u.id_usuario, r.id_rol, 'system'
FROM   seguridad.usuario_app u
CROSS  JOIN seguridad.rol r
WHERE  (u.username = 'admin'    AND r.nombre_rol = 'ADMIN')
   OR  (u.username = 'vendedor' AND r.nombre_rol = 'VENDEDOR');


-- ============================================================
-- VERIFICACION FINAL
-- Debes ver: 2 roles, 2 usuarios, 2 asignaciones.
-- ============================================================
SELECT 'Roles creados:        ' || COUNT(*)::text AS resultado FROM seguridad.rol;
SELECT 'Usuarios creados:     ' || COUNT(*)::text AS resultado FROM seguridad.usuario_app;
SELECT 'Asignaciones rol-usr: ' || COUNT(*)::text AS resultado FROM seguridad.usuarios_roles;

-- Vista rapida de usuarios con sus roles
SELECT
    u.username,
    u.correo,
    u.nombres || ' ' || COALESCE(u.apellidos, '') AS nombre_completo,
    r.nombre_rol,
    r.descripcion_rol,
    u.estado_usuario,
    u.activo
FROM   seguridad.usuario_app u
LEFT   JOIN seguridad.usuarios_roles ur ON ur.id_usuario = u.id_usuario
LEFT   JOIN seguridad.rol            r  ON r.id_rol      = ur.id_rol
ORDER  BY u.id_usuario;