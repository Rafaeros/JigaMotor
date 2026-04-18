using FluentValidation;

namespace JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOn
{
    public record SendEmergencyOnRequest(string DevEui, string PayloadBase64, int Port = 1);

    public class SendEmergencyOnRequestValidator : AbstractValidator<SendEmergencyOnRequest>
    {
        public SendEmergencyOnRequestValidator()
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
