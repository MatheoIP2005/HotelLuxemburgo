using HotelLux.Reservation.Business.DTOs.Reserva;
using HotelLux.Reservation.Business.DTOs.ReservaHabitacion;

namespace HotelLux.Reservation.Business.Validators;

public static class ReservaValidator
{
    public static IReadOnlyList<string> ValidarCreacion(ReservaCreateDTO dto)
    {
        var errors = new List<string>();

        var tieneClienteGuid = dto.ClienteGuid.HasValue && dto.ClienteGuid.Value != Guid.Empty;
        if (!tieneClienteGuid && dto.Cliente is null)
            errors.Add("Se requiere ClienteGuid o un objeto cliente.");
        if (dto.Cliente is not null)
        {
            if (string.IsNullOrWhiteSpace(dto.Cliente.TipoIdentificacion))
                errors.Add("Cliente.TipoIdentificacion es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.Cliente.NumeroIdentificacion))
                errors.Add("Cliente.NumeroIdentificacion es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.Cliente.Nombres))
                errors.Add("Cliente.Nombres es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.Cliente.Apellidos))
                errors.Add("Cliente.Apellidos es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.Cliente.Correo))
                errors.Add("Cliente.Correo es obligatorio.");
        }
        if (dto.SucursalGuid == Guid.Empty)
            errors.Add("SucursalGuid es obligatorio.");
        if (dto.FechaFin <= dto.FechaInicio)
            errors.Add("FechaFin debe ser posterior a FechaInicio.");
        if (!dto.Habitaciones.Any())
            errors.Add("La reserva debe incluir al menos una habitación.");
        if (dto.TotalReserva <= 0)
            errors.Add("TotalReserva debe ser mayor a cero.");
        if (string.IsNullOrWhiteSpace(dto.OrigenCanalReserva))
            errors.Add("OrigenCanalReserva es obligatorio.");

        foreach (var hab in dto.Habitaciones)
        {
            var resueltaPorTipo = hab.HabitacionGuid == Guid.Empty
                && hab.TipoHabitacionGuid.HasValue
                && hab.TipoHabitacionGuid.Value != Guid.Empty;

            if (hab.HabitacionGuid == Guid.Empty && !resueltaPorTipo)
                errors.Add("Cada habitación debe tener habitacionGuid o tipoHabitacionGuid.");

            if (hab.FechaFin <= hab.FechaInicio)
                errors.Add("FechaFin de habitación debe ser posterior a FechaInicio.");
            if (hab.PrecioNocheAplicado <= 0)
                errors.Add("PrecioNocheAplicado debe ser mayor a cero.");
            if (hab.NumAdultos <= 0)
                errors.Add("NumAdultos debe ser al menos 1.");
        }

        return errors;
    }

    public static IReadOnlyList<string> ValidarLineaHabitacion(ReservaHabitacionCreateDTO hab)
    {
        var errors = new List<string>();
        if (hab.HabitacionGuid == Guid.Empty)
            errors.Add("Cada habitación debe tener un HabitacionGuid válido.");
        if (hab.FechaFin <= hab.FechaInicio)
            errors.Add("FechaFin de habitación debe ser posterior a FechaInicio.");
        if (hab.PrecioNocheAplicado <= 0)
            errors.Add("PrecioNocheAplicado debe ser mayor a cero.");
        if (hab.NumAdultos <= 0)
            errors.Add("NumAdultos debe ser al menos 1.");
        if (hab.TotalLinea <= 0)
            errors.Add("TotalLinea debe ser mayor a cero.");
        return errors;
    }
}
