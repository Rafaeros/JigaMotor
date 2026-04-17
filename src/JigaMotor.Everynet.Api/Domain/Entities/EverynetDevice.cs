namespace JigaMotor.Everynet.Api.Domain.Entities
{

    public record Adr(int? TxPower, int? DataRate, string Mode);

    public record EverynetDevice(
        string DevEui,
        string AppEui,
        string Activation,
        string Encryption,
        string DevAddr,
        string Nwkskey,
        string Appskey,
        string DevClass,
        int CountersSize,
        Adr? Adr,
        string Band
    );
}
