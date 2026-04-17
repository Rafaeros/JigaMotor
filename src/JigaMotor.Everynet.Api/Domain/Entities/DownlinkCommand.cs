namespace JigaMotor.Everynet.Api.Domain.Entities
{
    public record DownlinkCommand(string DevEui, string Payload, int Port, bool Confirmed = false);
}