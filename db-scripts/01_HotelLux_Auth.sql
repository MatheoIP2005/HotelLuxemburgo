-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio AUTH
-- Base de datos: HotelLux_Auth
--
-- CONTENIDO:
--   Schema: seguridad
--   Tablas: rol, usuario_app, usuarios_roles
--
-- NOTA ARQUITECTURA:
--   cliente_guid en usuario_app se llena via bus de eventos / gRPC
--   cuando un usuario del portal se vincula a un cliente en
--   HotelLux_Reservation. No se carga en este script.
-- ============================================================

-- ============================================================
-- SCHEMA
-- ============================================================
CREATE SCHEMA IF NOT EXISTS seguridad;

-- ============================================================
-- TABLA: seguridad.rol
-- ============================================================
CREATE TABLE seguridad.rol (
    id_rol                   INT          GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    rol_guid                 UUID         NOT NULL DEFAULT gen_random_uuid(),
    nombre_rol               CHAR(20)     NOT NULL,
    descripcion_rol          VARCHAR(250) NULL,
    estado_rol               CHAR(3)      NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    activo                   BOOLEAN      NOT NULL DEFAULT TRUE,
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    motivo_inhabilitacion    VARCHAR(150) NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       CHAR(30)     NOT NULL,
    modificado_por_usuario   CHAR(30)     NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          CHAR(25)     NULL,
    CONSTRAINT uq_rol_guid   UNIQUE (rol_guid),
    CONSTRAINT uq_rol_nombre UNIQUE (nombre_rol),
    CONSTRAINT chk_rol_estado CHECK (estado_rol IN ('ACT','INA'))
);

-- ============================================================
-- TABLA: seguridad.usuario_app
-- ============================================================
CREATE TABLE seguridad.usuario_app (
    id_usuario               INT          GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    usuario_guid             UUID         NOT NULL DEFAULT gen_random_uuid(),
    cliente_guid             UUID         NULL,    -- vinculado via bus de eventos / gRPC desde HotelLux_Reservation
    username                 CHAR(15)     NOT NULL,
    correo                   CHAR(120)    NOT NULL,
    nombres                  VARCHAR(30)  NOT NULL,
    apellidos                VARCHAR(30)  NULL,
    password_hash            VARCHAR(500) NOT NULL,
    password_salt            VARCHAR(250) NOT NULL,
    estado_usuario           CHAR(3)      NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN      NOT NULL DEFAULT FALSE,
    activo                   BOOLEAN      NOT NULL DEFAULT TRUE,
    fecha_inhabilitacion_utc TIMESTAMPTZ  NULL,
    motivo_inhabilitacion    VARCHAR(150) NULL,
    fecha_registro_utc       TIMESTAMPTZ  NOT NULL DEFAULT now(),
    creado_por_usuario       CHAR(30)     NOT NULL,
    modificado_por_usuario   CHAR(30)     NULL,
    fecha_modificacion_utc   TIMESTAMPTZ  NULL,
    modificacion_ip          CHAR(25)     NULL,
    CONSTRAINT uq_usuario_app_guid     UNIQUE (usuario_guid),
    CONSTRAINT uq_usuario_app_username UNIQUE (username),
    CONSTRAINT uq_usuario_app_correo   UNIQUE (correo),
    CONSTRAINT chk_usuario_app_estado  CHECK  (estado_usuario IN ('ACT','INA','BLO'))
);

-- ============================================================
-- TABLA: seguridad.usuarios_roles (N:M)
-- ============================================================
CREATE TABLE seguridad.usuarios_roles (
    id_usuario_rol           INT         GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_usuario               INT         NOT NULL,  -- FK a seguridad.usuario_app; sincronizado via bus/gRPC
    id_rol                   INT         NOT NULL,  -- FK a seguridad.rol; sincronizado via bus/gRPC
    estado_usuario_rol       CHAR(3)     NOT NULL DEFAULT 'ACT',
    es_eliminado             BOOLEAN     NOT NULL DEFAULT FALSE,
    activo                   BOOLEAN     NOT NULL DEFAULT TRUE,
    fecha_registro_utc       TIMESTAMPTZ NOT NULL DEFAULT now(),
    creado_por_usuario       CHAR(30)    NOT NULL,
    modificado_por_usuario   CHAR(30)    NULL,
    fecha_modificacion_utc   TIMESTAMPTZ NULL,
    modificacion_ip          CHAR(25)    NULL,
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
-- ============================================================
-- DATOS DEL SISTEMA
-- ============================================================
-- ============================================================

-- ============================================================
-- ROLES
--
-- ESQUEMA DE GUIDs:
--   Prefijo 22xxxxxx-... = roles (rol_guid)
--
-- ADMIN    -> CREATE, READ, UPDATE, DELETE sobre todos los
--             recursos del sistema.
-- VENDEDOR -> CREATE, READ, UPDATE. Sin permiso de DELETE.
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
    'Administrador general del sistema. Permisos completos: CREATE, READ, UPDATE y DELETE sobre todos los recursos.',
    'system'
),
(
    '22222222-2222-2222-2222-222222222002',
    'VENDEDOR',
    'Personal de recepcion y ventas. Permisos: CREATE, READ, UPDATE. No puede eliminar registros del sistema.',
    'system'
);

-- ============================================================
-- USUARIOS DEL SISTEMA
--
-- ESQUEMA DE GUIDs:
--   Prefijo 21xxxxxx-... = usuarios (usuario_guid)
--
-- Contrasenas hasheadas con BCrypt cost 11:
--   admin    -> admin1234
--   vendedor -> vendedor1234
--
-- NOTA: cliente_guid = NULL para usuarios de staff.
--       Se llena via bus de eventos / gRPC unicamente
--       cuando el usuario esta vinculado a un cliente
--       registrado en HotelLux_Reservation.
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
    '$2a$12$Z2Gvfmfvp8xxdByGHmgPY.5gpMvE/SQ2MpJt86bl4q3Wv6df9sYBq',
    '$2a$12$Z2Gvfmfvp8xxdByGHmgPY',
    'system'
),
(
    '21111111-1111-1111-1111-111111111002',
    NULL,
    'vendedor',
    'vendedor@hotelluxemburgo.com',
    'Vendedor',
    'Recepcion',
    '$2a$12$35KBjtktrV3wPTPaez/28.X8yEupQ4LMWgcLTGBOKkZCewtf5xsvK',
    '$2a$12$35KBjtktrV3wPTPaez/28',
    'system'
),
(
    gen_random_uuid(),
    NULL,
    'pookingint',
    'pookingint@hotelluxemburgo.com',
    'Pooking',
    'Int',
    '$2a$12$cO/NpN3U0vOW2.noBD64k.ACtk25slMBAYoeAaBQ748Itwg1VJ0aq',
    '$2a$12$cO/NpN3U0vOW2.noBD64k',
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

INSERT INTO seguridad.usuarios_roles (
    id_usuario,
    id_rol,
    creado_por_usuario
)
SELECT u.id_usuario, r.id_rol, 'system'
FROM   seguridad.usuario_app u
CROSS  JOIN seguridad.rol r
WHERE  u.username = 'pookingint'
  AND  r.nombre_rol = 'ADMIN';

-- ============================================================
-- VERIFICACION FINAL
-- Resultados esperados: 2 roles, 2 usuarios, 2 asignaciones.
-- ============================================================
SELECT 'Roles creados:        ' || COUNT(*)::text AS resultado FROM seguridad.rol;
SELECT 'Usuarios creados:     ' || COUNT(*)::text AS resultado FROM seguridad.usuario_app;
SELECT 'Asignaciones rol-usr: ' || COUNT(*)::text AS resultado FROM seguridad.usuarios_roles;

-- Vista rapida: usuarios con sus roles asignados
SELECT
    u.username,
    u.correo,
    u.nombres || ' ' || COALESCE(u.apellidos, '') AS nombre_completo,
    r.nombre_rol,
    r.descripcion_rol,
    u.estado_usuario,
    u.activo
FROM   seguridad.usuario_app    u
LEFT   JOIN seguridad.usuarios_roles ur ON ur.id_usuario = u.id_usuario
LEFT   JOIN seguridad.rol            r  ON r.id_rol      = ur.id_rol
ORDER  BY u.id_usuario;