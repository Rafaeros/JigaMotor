namespace JigaMotor.SharePoint.Api.Domain.Entities;

public record NetworkKeys(
    string LoraId,
    string DevEui,
    string AppEui,
    string DevAddr,
    string NwkSKey,
    string AppSKey
);

public record HardwareTests(
    string P2pStatus,
    string AdcStatus,
    string EverynetStatus,
    string OtpStatus,
    string FirmwareStatus
);

public record PowerConsumptionTests(
    string LrwEmOnStatus,
    string AdvEmOnStatus,
    string PacketEmOnStatus,
    string LrwEmOffStatus,
    string AdvEmOffStatus,
    string PacketEmOffStatus
);

public record ProductionMetadata(
    string TagStatus,
    string RdpStatus,
    string BoxNumber,
    string? ProductionOrder,
    DateTime? PunchDate
);

public class DeviceProductionRecord(int Id, string? Serie, string? BatchNumber, string? FirmwareVersion, string? MemoryVersion, string? Bluetooth, DateTime? RecordDate, string? Model, NetworkKeys? Network, HardwareTests? Tests, PowerConsumptionTests? Consumption, ProductionMetadata? Metadata)
{
    public int Id { get; private set; } = Id;
    public string? Serie { get; private set; } = Serie;
    public string? BatchNumber { get; private set; } = BatchNumber;
    public string? FirmwareVersion { get; private set; } = FirmwareVersion;
    public string? MemoryVersion { get; private set; } = MemoryVersion;
    public string? Bluetooth { get; private set; } = Bluetooth;
    public DateTime? RecordDate { get; private set; } = RecordDate;
    public string? Model { get; private set; } = Model;
    public NetworkKeys? Network { get; private set; } = Network;
    public HardwareTests? Tests { get; private set; } = Tests;
    public PowerConsumptionTests? Consumption { get; private set; } = Consumption;
    public ProductionMetadata? Metadata { get; private set; } = Metadata;
}