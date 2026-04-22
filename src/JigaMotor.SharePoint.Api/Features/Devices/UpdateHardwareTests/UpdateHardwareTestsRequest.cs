using FluentValidation;
using JigaMotor.SharePoint.Api.Domain.Entities;

namespace JigaMotor.SharePoint.Api.Features.Devices.UpdateHardwareTests
{
    public record UpdateHardwareTestsRequest(
        string Identifier,
        string? P2pStatus,
        string? AdcStatus,
        string? EverynetStatus,
        string? OtpStatus,
        string? FirmwareStatus
    );

    public class UpdateHardwareTestsValidator : AbstractValidator<UpdateHardwareTestsRequest>
    {
        public UpdateHardwareTestsValidator()
        {
            RuleFor(x => x.Identifier).NotEmpty().WithMessage("Identificador (DevEui ou Série) é obrigatório.");
        }
    }
}
