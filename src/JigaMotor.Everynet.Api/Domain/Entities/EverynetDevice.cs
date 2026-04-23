using System.Text.Json.Serialization;

namespace JigaMotor.Everynet.Api.Domain.Entities
{
    public record Adr(
        int? TxPower,
        [property: JsonPropertyName("datarate")] int? Datarate,
        string Mode,
        bool? Enabled,
        int? MaxTxPower
    );

    public record Geolocation(
        double? Lat,
        double? Lng,
        string? Method,
        int? Precision,
        int? Quality
    );

    public record Rx1(
        int? CurrentDelay,
        int? Delay,
        string? Status
    );

    public record Rx2(
        bool? Force
    );

    public record EverynetDevice(
        string DevEui,
        string AppEui,
        string[] Tags,
        string Activation,
        string Encryption,
        string? DevAddr,
        string? Nwkskey,
        string? Appskey,
        string? AppKey,
        string DevClass,
        int CountersSize,
        Adr? Adr,
        string Band,
        bool? BlockDownlink,
        bool? BlockUplink,
        int? CounterDown,
        int? CounterUp,
        double? CreatedAt,
        double? LastActivity,
        double? LastJoin,
        bool? Locked,
        string? LorawanVersion,
        bool? Multicast,
        bool? StrictCounter,
        Geolocation? Geolocation,
        Rx1? Rx1,
        Rx2? Rx2
    );
}
