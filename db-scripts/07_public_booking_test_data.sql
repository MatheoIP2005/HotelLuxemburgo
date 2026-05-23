-- ============================================================
-- HotelLux - Public booking test data
-- ============================================================
-- Run this script against database: HotelLux_Accommodation
--
-- Purpose:
--   Creates or resets one dedicated public-test room for POST
--   /api/v1/accommodations/reservas.
--
-- Test route:
--   Sucursal: Hotel Luxemburgo Quito
--   sucursalGuid: 44444444-4444-4444-4444-444444444001
--   Tipo habitacion: Suite Doble
--   tipoHabitacionGuid: 55555555-5555-5555-5555-555555555002
--
-- Notes:
--   Reservation calls Accommodation over gRPC. If the POST still returns
--   "disponibles 0" after running this, verify that Accommodation gRPC
--   is running on http://localhost:5102 and reachable from Reservation.

DO $$
DECLARE
    v_sucursal_id INT;
    v_tipo_habitacion_id INT;
BEGIN
    SELECT id_sucursal
      INTO v_sucursal_id
      FROM alojamiento.sucursal
     WHERE codigo_sucursal = 'LUX-UIO';

    SELECT id_tipo_habitacion
      INTO v_tipo_habitacion_id
      FROM alojamiento.tipo_habitacion
     WHERE codigo_tipo_habitacion = 'TH-DOBLE';

    IF v_sucursal_id IS NULL THEN
        RAISE EXCEPTION 'No existe la sucursal LUX-UIO. Ejecute primero 02_HotelLux_Accommodation.sql.';
    END IF;

    IF v_tipo_habitacion_id IS NULL THEN
        RAISE EXCEPTION 'No existe el tipo TH-DOBLE. Ejecute primero 02_HotelLux_Accommodation.sql.';
    END IF;

    IF EXISTS (
        SELECT 1
          FROM alojamiento.habitacion
         WHERE habitacion_guid = '66666666-6666-6666-6666-666666666099'
    ) THEN
        UPDATE alojamiento.habitacion
           SET id_sucursal = v_sucursal_id,
               id_tipo_habitacion = v_tipo_habitacion_id,
               numero_habitacion = '999',
               piso = 9,
               capacidad_habitacion = 2,
               precio_base = 120.00,
               descripcion_habitacion = 'Habitacion doble dedicada a pruebas publicas de reserva.',
               estado_habitacion = 'DIS',
               es_eliminado = FALSE,
               fecha_inhabilitacion_utc = NULL,
               motivo_inhabilitacion = NULL,
               modificado_por_usuario = 'public-booking-test',
               fecha_modificacion_utc = now(),
               modificacion_ip = '127.0.0.1',
               servicio_origen = 'accommodation-service'
         WHERE habitacion_guid = '66666666-6666-6666-6666-666666666099';

    ELSIF EXISTS (
        SELECT 1
          FROM alojamiento.habitacion
         WHERE id_sucursal = v_sucursal_id
           AND numero_habitacion = '999'
    ) THEN
        UPDATE alojamiento.habitacion
           SET habitacion_guid = '66666666-6666-6666-6666-666666666099',
               id_tipo_habitacion = v_tipo_habitacion_id,
               piso = 9,
               capacidad_habitacion = 2,
               precio_base = 120.00,
               descripcion_habitacion = 'Habitacion doble dedicada a pruebas publicas de reserva.',
               estado_habitacion = 'DIS',
               es_eliminado = FALSE,
               fecha_inhabilitacion_utc = NULL,
               motivo_inhabilitacion = NULL,
               modificado_por_usuario = 'public-booking-test',
               fecha_modificacion_utc = now(),
               modificacion_ip = '127.0.0.1',
               servicio_origen = 'accommodation-service'
         WHERE id_sucursal = v_sucursal_id
           AND numero_habitacion = '999';

    ELSE
        INSERT INTO alojamiento.habitacion (
            habitacion_guid,
            id_sucursal,
            id_tipo_habitacion,
            numero_habitacion,
            piso,
            capacidad_habitacion,
            precio_base,
            descripcion_habitacion,
            estado_habitacion,
            creado_por_usuario,
            servicio_origen
        ) VALUES (
            '66666666-6666-6666-6666-666666666099',
            v_sucursal_id,
            v_tipo_habitacion_id,
            '999',
            9,
            2,
            120.00,
            'Habitacion doble dedicada a pruebas publicas de reserva.',
            'DIS',
            'public-booking-test',
            'accommodation-service'
        );
    END IF;

    UPDATE alojamiento.tarifa
       SET fecha_inicio = '2026-01-01',
           fecha_fin = '2026-12-31',
           precio_por_noche = 120.00,
           porcentaje_iva = 15.00,
           min_noches = 1,
           max_noches = NULL,
           canal_tarifa = 'TODOS',
           permite_portal_publico = TRUE,
           prioridad = 1,
           estado_tarifa = 'ACT',
           es_eliminado = FALSE,
           fecha_inhabilitacion_utc = NULL,
           motivo_inhabilitacion = NULL,
           modificado_por_usuario = 'public-booking-test',
           fecha_modificacion_utc = now(),
           modificacion_ip = '127.0.0.1'
     WHERE codigo_tarifa = 'TAR-UIO-DOBLE-2026';
END $$;

-- Quick verification. Expected: one row with estado_habitacion = DIS.
SELECT
    s.sucursal_guid,
    th.tipo_habitacion_guid,
    h.habitacion_guid,
    s.codigo_sucursal,
    th.codigo_tipo_habitacion,
    h.numero_habitacion,
    h.estado_habitacion,
    h.es_eliminado,
    h.precio_base
FROM alojamiento.habitacion h
JOIN alojamiento.sucursal s ON s.id_sucursal = h.id_sucursal
JOIN alojamiento.tipo_habitacion th ON th.id_tipo_habitacion = h.id_tipo_habitacion
WHERE h.habitacion_guid = '66666666-6666-6666-6666-666666666099';

-- Suggested POST body:
-- {
--   "sucursalGuid": "44444444-4444-4444-4444-444444444001",
--   "fechaInicio": "2026-08-20T15:00:00.000Z",
--   "fechaFin": "2026-08-22T12:00:00.000Z",
--   "origenCanalReserva": "WEB",
--   "observaciones": "Reserva de prueba publica con habitacion doble Quito 999",
--   "esWalkin": false,
--   "cliente": {
--     "tipoIdentificacion": "CED",
--     "numeroIdentificacion": "1799999901",
--     "nombres": "Camila Isabel",
--     "apellidos": "Torres Andrade",
--     "correo": "camila.torres.test001@example.com",
--     "telefono": "0991234567",
--     "direccion": "Av. Republica y Eloy Alfaro, Quito"
--   },
--   "habitaciones": [
--     {
--       "tipoHabitacionGuid": "55555555-5555-5555-5555-555555555002",
--       "numHabitaciones": 1,
--       "numAdultos": 2,
--       "numNinos": 0
--     }
--   ]
-- }
