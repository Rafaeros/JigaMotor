using FluentValidation;

namespace JigaMotor.SharePoint.Api.Features.Devices.CreateInitialDevice
{
    public record CreateInitialDeviceRequest(
        string DevEui,
        string AppEui,
        string NwsKey,
        string DevAddr,
        string AppSKey,
        string Bluetooth,
        string Firmware,
        string Memory,
        string Model
    );

    public class CreateInitialDeviceValidator : AbstractValidator<CreateInitialDeviceRequest>
    {
        public CreateInitialDeviceValidator()
        {
            RuleFor(x => x.DevEui).NotEmpty().Length(16).WithMessage("DevEui é obrigatório com 16 caracteres.");
            RuleFor(x => x.AppEui).NotEmpty().WithMessage("AppEui é obrigatório.");
            RuleFor(x => x.NwsKey).NotEmpty().WithMessage("NwsKey é obrigatório.");
            RuleFor(x => x.DevAddr).NotEmpty().WithMessage("DevAddr é obrigatório.");
            RuleFor(x => x.AppSKey).NotEmpty().WithMessage("AppSKey é obrigatório.");
            RuleFor(x => x.Bluetooth).NotEmpty().WithMessage("Bluetooth é obrigatório.");
            RuleFor(x => x.Firmware).NotEmpty().WithMessage("Firmware é obrigatório.");
            RuleFor(x => x.Memory).NotEmpty().WithMessage("Memory é obrigatório.");
            RuleFor(x => x.Model).NotEmpty().WithMessage("Model é obrigatório.");
        }
    }
}
