using FluentValidation;

namespace JigaMotor.Everynet.Api.Features.Devices.CreateDevice
{
    public class CreateDeviceValidator : AbstractValidator<CreateDeviceRequest>
    {
        public CreateDeviceValidator()
        {
            RuleFor(x => x.DevEui).NotEmpty().Length(16).Matches("^[0-9a-fA-F]+$");
            RuleFor(x => x.AppEui).NotEmpty().Length(16).Matches("^[0-9a-fA-F]+$");
            RuleFor(x => x.DevAddr).NotEmpty();
            RuleFor(x => x.Nwkskey).NotEmpty().Length(32);
            RuleFor(x => x.Appskey).NotEmpty().Length(32);
            RuleFor(x => x.Tags).NotNull();

            RuleFor(x => x.Activation).Must(x => x is null or "ABP" or "OTAA")
                .WithMessage("Activation deve ser 'ABP' ou 'OTAA' (ou omitido para padrão 'ABP').");
        }
    }
}
