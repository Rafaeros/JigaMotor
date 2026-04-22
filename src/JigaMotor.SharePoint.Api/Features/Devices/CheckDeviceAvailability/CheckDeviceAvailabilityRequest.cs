using FluentValidation;

namespace JigaMotor.SharePoint.Api.Features.Devices.CheckDeviceAvailability
{
    public record CheckDeviceAvailabilityRequest(
        string DevEui,
        string Lora,
        string Serie
    );

    public class CheckDeviceAvailabilityValidator : AbstractValidator<CheckDeviceAvailabilityRequest>
    {
        public CheckDeviceAvailabilityValidator()
        {
            RuleFor(x => x.DevEui)
                .NotEmpty().WithMessage("DevEui é obrigatório.")
                .Length(16).WithMessage("DevEui deve ter 16 caracteres.");

            RuleFor(x => x.Lora)
                .NotEmpty().WithMessage("Lora id é obrigatório.");

            RuleFor(x => x.Serie)
                .NotEmpty().WithMessage("Série é obrigatória.");
        }
    }
}
