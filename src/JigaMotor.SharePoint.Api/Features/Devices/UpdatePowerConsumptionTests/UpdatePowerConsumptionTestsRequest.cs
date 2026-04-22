using FluentValidation;

namespace JigaMotor.SharePoint.Api.Features.Devices.UpdatePowerConsumptionTests
{
    public record UpdatePowerConsumptionTestsRequest(
        string Identifier,
        string? LrwEmOnStatus,
        string? AdvEmOnStatus,
        string? PacketEmOnStatus,
        string? LrwEmOffStatus,
        string? AdvEmOffStatus,
        string? PacketEmOffStatus
    );

    public class UpdatePowerConsumptionTestsValidator : AbstractValidator<UpdatePowerConsumptionTestsRequest>
    {
        public UpdatePowerConsumptionTestsValidator()
        {
            RuleFor(x => x.Identifier).NotEmpty().WithMessage("Identificador (DevEui, Série ou LoRa) é obrigatório.");
        }
    }
}
