using JigaMotor.SharePoint.Api.Domain.Entities;
using Microsoft.Graph.Models;

namespace JigaMotor.SharePoint.Api.Infrastructure.Mappers;

public static class SharePointFirmwareMapper
{
    public static Firmware ToDomain(ListItem item)
    {
        string GetField(string name)
        {
            if (item.Fields?.AdditionalData == null) return string.Empty;

            if (item.Fields.AdditionalData.TryGetValue(name, out var value))
                return value?.ToString()?.Trim() ?? string.Empty;

            // Tenta variações (ex: Versao -> VERSAO, Title -> TITLE)
            if (item.Fields.AdditionalData.TryGetValue(name.ToUpper(), out value))
                return value?.ToString()?.Trim() ?? string.Empty;

            var pascalName = char.ToUpper(name[0]) + name[1..].ToLower();
            if (item.Fields.AdditionalData.TryGetValue(pascalName, out value))
                return value?.ToString()?.Trim() ?? string.Empty;

            return string.Empty;
        }

        bool GetBoolField(string name)
        {
            if (item.Fields?.AdditionalData == null) return false;

            if (!item.Fields.AdditionalData.TryGetValue(name, out var value))
            {
                if (!item.Fields.AdditionalData.TryGetValue(name.ToUpper(), out value))
                {
                    var pascalName = char.ToUpper(name[0]) + name[1..].ToLower();
                    if (!item.Fields.AdditionalData.TryGetValue(pascalName, out value))
                        return false;
                }
            }

            if (value is bool b) return b;
            if (value?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true) return true;
            if (value?.ToString()?.Equals("1") == true) return true;

            return false;
        }

        int GetIntField(string name)
        {
            if (item.Fields?.AdditionalData == null) return 0;

            if (!item.Fields.AdditionalData.TryGetValue(name, out var value))
            {
                if (!item.Fields.AdditionalData.TryGetValue(name.ToUpper(), out value))
                {
                    var pascalName = char.ToUpper(name[0]) + name[1..].ToLower();
                    if (!item.Fields.AdditionalData.TryGetValue(pascalName, out value))
                        return 0;
                }
            }

            if (value is int i) return i;
            if (int.TryParse(value?.ToString(), out i)) return i;

            return 0;
        }

        _ = int.TryParse(item.Id, out int id);

        return new Firmware(
            Id: id,
            Model: GetField("Title"),
            Version: GetField("Versao"),
            IsActive: GetBoolField("Ativo"),
            Purpose: GetField("Finalidade"),
            Code: GetIntField("Codigo"),
            ProductCode: GetIntField("CodigoProduto"),
            HardwareVersion: GetIntField("Hardware"),
            MemoryVersion: GetIntField("MemoryVersion"),
            Address: GetField("address")
        );

    }
}
