

namespace JigaMotor.Shared.Models;


public record CreateDeviceRequest(
    string Serie,
    string DevEui,
    string AppEui,
    string AppKey
);

public record DeviceItem(
    string Id,
    string Serie,
    string DevEui,
    string AppEui,
    string AppKey
);

public record DeviceListResponse(
    List<DeviceItem> Items
);

public record UpdateDeviceRequest(
    string? Serie,
    string? DevEui,
    string? AppEui,
    string? AppKey
);

