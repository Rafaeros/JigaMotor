namespace JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOn
{
    public record SendEmergencyOnResponse(string DevEui, string MessageId, DateTime SentAt);
}
