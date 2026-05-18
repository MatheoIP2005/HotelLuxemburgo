using FluentValidation;
using HotelLux.Auth.Business.DTOs.Roles;

namespace HotelLux.Auth.Business.Validators;

public class RolValidator : AbstractValidator<RolCreateDTO>
{
    public RolValidator()
    {
        RuleFor(x => x.NombreRol)
            .NotEmpty().MaximumLength(50);
        RuleFor(x => x.DescripcionRol)
            .MaximumLength(250);
    }
}
