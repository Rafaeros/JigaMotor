using FluentValidation;

namespace JigaMotor.Everynet.Api.Features.Devices.UpdateDevice
{
    public class UpdateDeviceValidator : AbstractValidator<UpdateDeviceRequest>
    {
        public UpdateDeviceValidator()
        {
            RuleFor(x => x.AppEui).NotEmpty().Length(16).Matches("^[0-9a-fA-F]+$");
            RuleFor(x => x.Activation).NotEmpty().Must(x => x is "ABP" or "OTAA");
            RuleFor(x => x.Encryption).NotEmpty();
            RuleFor(x => x.Band).NotEmpty();
            
            When(x => x.Activation == "ABP", () => {
                RuleFor(x => x.DevAddr).NotEmpty();
                RuleFor(x => x.Nwkskey).NotEmpty().Length(32);
                RuleFor(x => x.Appskey).NotEmpty().Length(32);
            });
        }
    }
}
