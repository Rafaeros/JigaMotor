using FluentValidation;

namespace JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOff
{
    public record SendEmergencyOffRequest(string DevEui, string PayloadBase64, int Port = 1);

    public class SendEmergencyOffRequestValidator : AbstractValidator<SendEmergencyOffRequest>
    {
        public SendEmergencyOffRequestValidator()
        {
            RuleFor(x => x.DevEui)
                .NotEmpty().WithMessage("O DevEui é obrigatório.")
                .Length(16).WithMessage("O DevEui deve ter exatamente 16 caracteres.");
            
            RuleFor(x => x.PayloadBase64)
                .NotEmpty().WithMessage("O comando (PayloadBase64) é obrigatório.");

            RuleFor(x => x.Port)
                .GreaterThan(0).WithMessage("A porta (Port) deve ser maior que 0.");
        }
    }
}
