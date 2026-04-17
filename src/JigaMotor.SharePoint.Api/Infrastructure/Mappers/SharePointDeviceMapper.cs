using JigaMotor.SharePoint.Api.Domain.Entities;
using Microsoft.Graph.Models;

namespace JigaMotor.SharePoint.Api.Infrastructure.Mappers;

public static class SharePointDeviceMapper
{
    public static DeviceProductionRecord ToDomain(ListItem item)
    {
        string GetField(string name)
        {
            if (item.Fields?.AdditionalData != null && item.Fields.AdditionalData.TryGetValue(name, out var value))
            {
                return value?.ToString()?.Trim() ?? string.Empty;
            }
            return string.Empty;
        }

        DateTime? GetDate(string name)
        {
            var strValue = GetField(name);
            if (DateTime.TryParse(strValue, out var date)) return date;
            return null;
        }

        var network = new NetworkKeys(
            LoraId: GetField("LORA"),
            DevEui: GetField("DEVEUI"),
            AppEui: GetField("APPEUI"),
            DevAddr: GetField("DEVADDR"),
            NwkSKey: GetField("NWSKEY"),
            AppSKey: GetField("APPSKEY")
        );

        var tests = new HardwareTests(
            P2pStatus: GetField("P2P"),
            AdcStatus: GetField("ADC"),
            EverynetStatus: GetField("EVERYNET"),
            OtpStatus: GetField("OTP"),
            FirmwareStatus: GetField("FW")
        );

        var consumption = new PowerConsumptionTests(
            LrwEmOnStatus: GetField("LRWEMON"),
            AdvEmOnStatus: GetField("ADVEMON"),
            PacketEmOnStatus: GetField("PACKETEMON"),
            LrwEmOffStatus: GetField("LRWEMOFF"),
            AdvEmOffStatus: GetField("ADVEMOFF"),
            PacketEmOffStatus: GetField("PACKETEMOFF")
        );

        var metadata = new ProductionMetadata(
            TagStatus: GetField("TAG"),
            RdpStatus: GetField("RDP"),
            BoxNumber: GetField("CAIXA"),
            ProductionOrder: GetField("OrdemProducao"),
            PunchDate: GetDate("DataBatida")
        );

        _ = int.TryParse(GetField("Title"), out int domainId);

        return new DeviceProductionRecord(
            Id: domainId,
            Serie: GetField("SERIE"),
            BatchNumber: GetField("LOTE"),
            FirmwareVersion: GetField("FIRMWARE"),
            MemoryVersion: GetField("MEMORY"),
            Bluetooth: GetField("BLUETOOTH"),
            RecordDate: GetDate("DATA") ?? DateTime.MinValue,
            Model: GetField("MODELO"),
            Network: network,
            Tests: tests,
            Consumption: consumption,
            Metadata: metadata
        );
    }
}