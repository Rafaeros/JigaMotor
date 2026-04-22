using FluentValidation;

namespace JigaMotor.SharePoint.Api.Features.Devices.AttachDeviceLog
{
    public record AttachDeviceLogRequest(
        string Identifier,
        string LogBase64
    );

    public class AttachDeviceLogValidator : AbstractValidator<AttachDeviceLogRequest>
    {
        public AttachDeviceLogValidator()
        {
            RuleFor(x => x.Identifier).NotEmpty().WithMessage("O identificador (DevEui, Serie ou Lora) é obrigatório.");
            RuleFor(x => x.LogBase64).NotEmpty().WithMessage("O conteúdo do log em Base64 é obrigatório.");
        }
    }
}
