namespace JigaMotor.SharePoint.Api.Domain.Entities;

public class Firmware(
    int Id, 
    string Model, 
    string Version, 
    bool IsActive, 
    string Purpose, 
    int Code = 0,
    int ProductCode = 0,
    int HardwareVersion = 0,
    int MemoryVersion = 0,
    string Address = "0x0",
    string? HexPath = null)
{
    public int Id { get; private set; } = Id;
    public string Model { get; private set; } = Model;
    public string Version { get; private set; } = Version;
    public bool IsActive { get; private set; } = IsActive;
    public string Purpose { get; private set; } = Purpose;
    public int Code { get; private set; } = Code;
    public int ProductCode { get; private set; } = ProductCode;
    public int HardwareVersion { get; private set; } = HardwareVersion;
    public int MemoryVersion { get; private set; } = MemoryVersion;
    public string Address { get; private set; } = Address;
    public string? HexPath { get; set; } = HexPath;
}

