-- ============================================================
-- HOTEL LUXEMBURGO -- Microservicio AUDIT
-- Base de datos: HotelLux_Audit
-- Motor: PostgreSQL 18
-- Version: 1.1
--
-- ALCANCE: este microservicio NO expone REST publico. Es un
-- consumidor gRPC fire-and-forget que recibe eventos de todos
-- los demas microservicios y los persiste para trazabilidad.
--
-- DIFERENCIA CON EL MONOLITO:
--   En el monolito existian 13 triggers en SQL Server que
--   escribian a seguridad.AUDITORIA en la misma BD. Aqui los
--   triggers DESAPARECEN. Cada microservicio (auth, accommodation,
--   reservation, stay, finance) implementa un cliente gRPC que
--   llama a audit.EmitAuditEvent(...) tras cada operacion CRUD.
--
-- DEPENDENCIAS LOGICAS:
--   - NINGUNA. Es la unica BD totalmente autonoma del sistema.
--     Sus referencias_id son GUIDs sueltos que apuntan a
--     entidades de otras BDs (sin necesidad de validacion).
--
-- CORRECCIONES v1.1 (respecto al original):
--   [EVT-02] Evento reemplazado: 'juan.perez' (GUID 21...004)
--            no existe en Auth. Ahora registra creacion real
--            del usuario 'vendedor' (GUID 21...002).
--   [EVT-03] usuario_ejecutor: 'juan.perez' -> 'vendedor'
--            usuario_guid: 21...004 -> 21...002
--   [EVT-06] usuario_ejecutor: 'juan.perez' -> 'vendedor'
--   [EVT-07] usuario_ejecutor: 'juan.perez' -> 'vendedor'
--   [EVT-08] usuario_ejecutor: 'juan.perez' -> 'vendedor'
--   [EVT-09] usuario_ejecutor: 'vendedor1'  -> 'vendedor'
--   [EVT-10] usuario_ejecutor: 'vendedor1'  -> 'vendedor'
--   [EVT-11] usuario_ejecutor: 'vendedor1'  -> 'vendedor'
--   [EVT-12] usuario_ejecutor: 'vendedor1'  -> 'vendedor'
--            fecha_evento_utc: '2026-05-05 11:05:00+00'
--                           -> '2026-05-02 11:31:10+00'
--            (alineada con finanzas.pago cc2001 en HotelLux_Finance)
--   [EVT-13] usuario_ejecutor: 'pedro.garcia' -> 'vendedor'
--            (pedro.garcia no tiene cuenta en Auth)
--            usuario_guid: NULL -> 21...002
--   [NUEVOS]  Eventos 14-23: flujo completo Carlos Mora
--            (cliente 33...005, RES-005, FAC-006, pago cc2004,
--            estadia aa...005, valoracion dd...1005, pago cc2005)
--            y Turismo Andes (cliente 33...004, RES-004, FAC-005).
--
-- CONTENIDO:
--   Schema: auditoria
--   Tablas: evento_auditoria
--   Datos semilla: 23 eventos representativos del flujo end-to-end
--                  alineados con las otras 5 BDs del sistema.
-- ============================================================


-- ============================================================
-- SCHEMA
-- ============================================================
CREATE SCHEMA IF NOT EXISTS auditoria;


-- ============================================================
-- TABLA: auditoria.evento_auditoria
--
-- Cambios respecto al monolito:
--   - id_auditoria: BIGINT (soporta volumen alto)
--   - datos_anteriores / datos_nuevos: JSONB (mucho mejor que
--     VARCHAR(MAX) de SQL Server: indexable, consultable,
--     valida sintaxis JSON automaticamente)
--   - servicio_origen: VARCHAR(80) con CHECK de los servicios
--     conocidos (auth, accommodation, reservation, stay,
--     finance, gateway). Si llega otro nombre, se rechaza.
-- ============================================================
CREATE TABLE auditoria.evento_auditoria (
    id_auditoria             BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    auditoria_guid           UUID         NOT NULL DEFAULT gen_random_uuid(),
    tabla_afectada           VARCHAR(100) NOT NULL,   -- ej: alojamiento.sucursal
    operacion                VARCHAR(10)  NOT NULL,   -- INSERT | UPDATE | DELETE
    entidad_guid             UUID         NULL,       -- ref logica a la entidad de otra BD
    id_registro_afectado     VARCHAR(100) NULL,       -- ID secuencial del registro (opcional)
    datos_anteriores         JSONB        NULL,       -- estado previo (UPDATE/DELETE)
    datos_nuevos             JSONB        NULL,       -- estado nuevo (INSERT/UPDATE)
    usuario_ejecutor         VARCHAR(100) NOT NULL,   -- username de quien realizo la accion
    usuario_guid             UUID         NULL,       -- ref logica al usuario en Auth
    ip_origen                VARCHAR(45)  NULL,
    servicio_origen          VARCHAR(80)  NOT NULL,
    fecha_evento_utc         TIMESTAMPTZ  NOT NULL DEFAULT now(),
    activo                   BOOLEAN      NOT NULL DEFAULT TRUE,
    CONSTRAINT uq_auditoria_guid     UNIQUE (auditoria_guid),
    CONSTRAINT chk_auditoria_operacion CHECK (operacion IN ('INSERT','UPDATE','DELETE')),
    CONSTRAINT chk_auditoria_servicio  CHECK (
        servicio_origen IN (
            'auth-service',
            'accommodation-service',
            'reservation-service',
            'stay-service',
            'finance-service',
            'gateway-service',
            'audit-service'
        )
    )
);


-- ============================================================
-- INDICES DE APOYO
-- Queries tipicos del endpoint GET /auditoria:
--   - eventos de la tabla X en rango de fechas Y
--   - eventos generados por el usuario Z
--   - eventos sobre la entidad con GUID W
--   - eventos del servicio S
-- ============================================================
CREATE INDEX ix_auditoria_tabla_fecha
    ON auditoria.evento_auditoria(tabla_afectada, fecha_evento_utc DESC);

CREATE INDEX ix_auditoria_usuario
    ON auditoria.evento_auditoria(usuario_ejecutor, fecha_evento_utc DESC);

CREATE INDEX ix_auditoria_servicio
    ON auditoria.evento_auditoria(servicio_origen, fecha_evento_utc DESC);

CREATE INDEX ix_auditoria_entidad
    ON auditoria.evento_auditoria(entidad_guid)
    WHERE entidad_guid IS NOT NULL;

CREATE INDEX ix_auditoria_fecha
    ON auditoria.evento_auditoria(fecha_evento_utc DESC);

-- Indice GIN sobre JSONB para queries tipo datos_nuevos->>'campo' = X
CREATE INDEX ix_auditoria_datos_nuevos_gin
    ON auditoria.evento_auditoria USING GIN (datos_nuevos);