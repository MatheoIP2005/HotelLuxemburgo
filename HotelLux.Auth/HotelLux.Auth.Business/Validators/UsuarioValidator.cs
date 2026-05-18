using FluentValidation;
using HotelLux.Auth.Business.DTOs.Usuarios;

namespace HotelLux.Auth.Business.Validators;

public class UsuarioValidator : AbstractValidator<UsuarioCreateDTO>
{
    public UsuarioValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().MaximumLength(50);
        RuleFor(x => x.Correo)
            .NotEmpty().MaximumLength(120).EmailAddress();
        RuleFor(x => x.Nombres)
            .NotEmpty().MaximumLength(120);
        RuleFor(x => x.Apellidos)
            .MaximumLength(120);
        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(8);
    }
}
