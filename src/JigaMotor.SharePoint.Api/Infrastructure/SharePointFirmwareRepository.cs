using JigaMotor.SharePoint.Api.Domain.Entities;
using JigaMotor.SharePoint.Api.Domain.Interfaces;
using JigaMotor.SharePoint.Api.Features.Common;
using JigaMotor.SharePoint.Api.Infrastructure.Configuration;
using JigaMotor.SharePoint.Api.Infrastructure.Mappers;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using System.Text.Json;
using System.Net.Http.Headers;
using System.IO.Compression;

namespace JigaMotor.SharePoint.Api.Infrastructure;

public class SharePointFirmwareRepository(
    GraphClientProvider graphProvider,
    IHttpClientFactory httpClientFactory,
    IOptions<SharePointOptions> options) : IFirmwareRepository
{
    private readonly SharePointOptions _options = options.Value;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private string? _cachedSiteId;
    private string? _cachedListId;

    public async Task<List<Firmware>> GetByModelAsync(string model)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);
            
            var result = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
            {
                config.Headers.Add("Prefer", "HonorNonIndexedQueriesWarningMayFailRandomly");
                config.QueryParameters.Expand = ["fields"];
                config.QueryParameters.Select = ["id", "fields"];
                config.QueryParameters.Filter = $"fields/Title eq '{model}' and (fields/Finalidade eq 'Teste' or fields/Finalidade eq 'Producao') and fields/Ativo eq 1";
            });


            var firmwares = result?.Value?.Select(SharePointFirmwareMapper.ToDomain).ToList() ?? [];

            foreach (var firmware in firmwares)
            {
                var item = result?.Value?.FirstOrDefault(i => i.Id == firmware.Id.ToString());
                firmware.HexPath = await ProcessFirmwareAttachmentsFromItemAsync(item, firmware);
            }

            return firmwares;
        });
    }

    public async Task<List<Firmware>> GetAllActiveAsync()
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);
            
            var result = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
            {
                config.Headers.Add("Prefer", "HonorNonIndexedQueriesWarningMayFailRandomly");
                config.QueryParameters.Expand = ["fields"];
                config.QueryParameters.Select = ["id", "fields"];
                config.QueryParameters.Filter = "fields/Ativo eq 1 and (fields/Finalidade eq 'Teste' or fields/Finalidade eq 'Producao')";
            });


            var firmwares = result?.Value?.Select(SharePointFirmwareMapper.ToDomain).ToList() ?? [];

            foreach (var firmware in firmwares)
            {
                var item = result?.Value?.FirstOrDefault(i => i.Id == firmware.Id.ToString());
                firmware.HexPath = await ProcessFirmwareAttachmentsFromItemAsync(item, firmware);
            }

            return firmwares;
        });
    }

    private async Task<(string SiteId, string ListId)> ResolveSiteAndListAsync(GraphServiceClient client)
    {
        if (!string.IsNullOrEmpty(_cachedSiteId) && !string.IsNullOrEmpty(_cachedListId))
            return (_cachedSiteId, _cachedListId);

        var site = await client.Sites[_options.SiteUrl].GetAsync()
            ?? throw new Exception($"Site SharePoint não encontrado: {_options.SiteUrl}");
        _cachedSiteId = site.Id;

        var listResponse = await client.Sites[_cachedSiteId].Lists.GetAsync();
        var targetList = listResponse?.Value?.FirstOrDefault(l =>
            l.DisplayName?.Equals(_options.FirmwareListName, StringComparison.OrdinalIgnoreCase) == true ||
            l.Name?.Equals(_options.FirmwareListName, StringComparison.OrdinalIgnoreCase) == true)
            ?? throw new Exception($"Lista de Firmwares '{_options.FirmwareListName}' não encontrada.");

        _cachedListId = targetList.Id;

        return (_cachedSiteId, _cachedListId);
    }

    private async Task<string?> ProcessFirmwareAttachmentsFromItemAsync(ListItem? item, Firmware firmware)
    {
        // 1. Tenta encontrar localmente primeiro (independente de SharePoint)
        var localHex = GetLocalFirmwarePath(firmware, "dummy.hex");
        if (!string.IsNullOrEmpty(localHex)) return localHex;
        
        var localBin = GetLocalFirmwarePath(firmware, "dummy.bin");
        if (!string.IsNullOrEmpty(localBin)) return localBin;

        if (item?.Id == null) return null;

        var client = graphProvider.GetAuthenticatedClient();
        var (siteId, listId) = await ResolveSiteAndListAsync(client);


        // Estratégia 1: Tenta via Graph API BETA (mais moderna)
        var graphAccessToken = await graphProvider.GetGraphAccessTokenAsync();
        var graphBetaUrl = $"https://graph.microsoft.com/beta/sites/{siteId}/lists/{listId}/items/{item.Id}/attachments";
        
        var hexPath = await TryGetAttachmentFromUrlAsync(graphBetaUrl, graphAccessToken, firmware, siteId, listId, isGraph: true);
        if (!string.IsNullOrEmpty(hexPath)) return hexPath;

        // Estratégia 2: Tenta via SharePoint REST API (Legado, mas robusto para anexos)
        var spAccessToken = await graphProvider.GetSharePointAccessTokenAsync();
        var baseUrl = _options.SiteUrl.Replace(":/", "/");
        if (!baseUrl.StartsWith("https://")) baseUrl = "https://" + baseUrl;
        var spRestUrl = $"{baseUrl}/_api/web/lists/getbytitle('{_options.FirmwareListName}')/items({item.Id})/AttachmentFiles";

        hexPath = await TryGetAttachmentFromUrlAsync(spRestUrl, spAccessToken, firmware, siteId, listId, isGraph: false);
        
        return hexPath;
    }

    private string GetLocalFirmwarePath(Firmware firmware, string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var customFileName = $"{firmware.Model}_v{firmware.Version}_{firmware.Purpose}{extension}";
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firmwares");
        var filePath = Path.Combine(directoryPath, customFileName);

        if (File.Exists(filePath))
        {
            return filePath;
        }

        return string.Empty;
    }


    private async Task<string?> TryGetAttachmentFromUrlAsync(string url, string token, Firmware firmware, string siteId, string listId, bool isGraph)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (!isGraph) request.Headers.TryAddWithoutValidation("Accept", "application/json;odata=verbose");

        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) 
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[FIRMWARE ERROR] Status: {response.StatusCode} em {url}. Detalhe: {error}");
            return null;
        }


        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        
        JsonElement results;
        if (isGraph)
        {
            if (!doc.RootElement.TryGetProperty("value", out results)) return null;
        }
        else
        {
            if (!doc.RootElement.TryGetProperty("d", out var d) || !d.TryGetProperty("results", out results)) return null;
        }

        foreach (var attachment in results.EnumerateArray())
        {
            var fileName = isGraph ? attachment.GetProperty("name").GetString() : attachment.GetProperty("FileName").GetString();
            var id = isGraph ? attachment.GetProperty("id").GetString() : null;
            var serverRelativeUrl = !isGraph ? attachment.GetProperty("ServerRelativeUrl").GetString() : null;

            if (fileName == null) continue;

            // Check if already exists locally
            var localPath = GetLocalFirmwarePath(firmware, fileName);
            if (!string.IsNullOrEmpty(localPath)) 
            {
                Console.WriteLine($"[FIRMWARE] Usando arquivo local: {localPath}");
                return localPath;
            }

            if (fileName.EndsWith(".hex", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
            {
                var customFileName = $"{firmware.Model}_v{firmware.Version}_{firmware.Purpose}{Path.GetExtension(fileName)}";
                if (isGraph)
                    return await DownloadGraphAttachmentAsync(siteId, listId, firmware.Id.ToString(), id!, customFileName, token);
                else
                {
                    var baseUrl = _options.SiteUrl.Replace(":/", "/");
                    if (!baseUrl.StartsWith("https://")) baseUrl = "https://" + baseUrl;
                    return await DownloadFileFromRestAsync(baseUrl, serverRelativeUrl!, customFileName, token);
                }
            }

            if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                if (isGraph)
                {
                    var path = await DownloadAndExtractHexFromZipAsync(siteId, listId, firmware.Id.ToString(), id!, fileName, firmware, token);
                    if (!string.IsNullOrEmpty(path)) return path;
                }
                else
                {
                    var path = await DownloadAndExtractHexFromRestZipAsync(serverRelativeUrl!, fileName, firmware, token);
                    if (!string.IsNullOrEmpty(path)) return path;
                }
            }
        }

        return null;
    }


    private async Task<string?> DownloadAndExtractHexFromRestZipAsync(string serverRelativeUrl, string zipName, Firmware firmware, string accessToken)
    {
        var baseUrl = _options.SiteUrl.Replace(":/", "/");
        if (!baseUrl.StartsWith("https://")) baseUrl = "https://" + baseUrl;

        // Use the REST API endpoint for downloading to ensure the Bearer token works
        var downloadUrl = $"{baseUrl}/_api/web/GetFileByServerRelativeUrl('{serverRelativeUrl}')/$value";



        using var httpClient = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var tempZipPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
        using (var fs = File.Create(tempZipPath))
        {
            await response.Content.CopyToAsync(fs);
        }

        try
        {
            using var archive = ZipFile.OpenRead(tempZipPath);
            var entry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".hex", StringComparison.OrdinalIgnoreCase) || e.Name.EndsWith(".bin", StringComparison.OrdinalIgnoreCase));
            
            if (entry != null)
            {
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firmwares");
                if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                var customFileName = $"{firmware.Model}_v{firmware.Version}_{firmware.Purpose}{entry.Name[entry.Name.LastIndexOf('.')..]}";
                var filePath = Path.Combine(directoryPath, customFileName);
                
                entry.ExtractToFile(filePath, true);
                return filePath;


            }
        }
        catch { return null; }
        finally { if (File.Exists(tempZipPath)) File.Delete(tempZipPath); }

        return null;
    }

    private async Task<string> DownloadFileFromRestAsync(string baseUrl, string serverRelativeUrl, string fileName, string accessToken)
    {
        // Use the REST API endpoint for downloading to ensure the Bearer token works
        var downloadUrl = $"{baseUrl}/_api/web/GetFileByServerRelativeUrl('{serverRelativeUrl}')/$value";

        using var httpClient = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return string.Empty;

        var contentStream = await response.Content.ReadAsStreamAsync();
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firmwares");
        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

        var filePath = Path.Combine(directoryPath, fileName);
        using (var fileStream = File.Create(filePath))
        {
            await contentStream.CopyToAsync(fileStream);
        }

        return filePath;
    }


    private async Task<string?> DownloadAndExtractHexFromZipAsync(string siteId, string listId, string itemId, string attachmentId, string zipName, Firmware firmware, string accessToken)
    {
        var downloadUrl = $"https://graph.microsoft.com/v1.0/sites/{siteId}/lists/{listId}/items/{itemId}/attachments/{attachmentId}/$value";
        
        using var httpClient = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var tempZipPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
        using (var fs = File.Create(tempZipPath))
        {
            await response.Content.CopyToAsync(fs);
        }

        try
        {
            using var archive = ZipFile.OpenRead(tempZipPath);
            // Procura por .hex ou .bin
            var entry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".hex", StringComparison.OrdinalIgnoreCase) || e.Name.EndsWith(".bin", StringComparison.OrdinalIgnoreCase));
            
            if (entry != null)
            {
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firmwares");
                if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

                var customFileName = $"{firmware.Model}_v{firmware.Version}_{firmware.Purpose}{entry.Name[entry.Name.LastIndexOf('.')..]}";
                var filePath = Path.Combine(directoryPath, customFileName);
                
                entry.ExtractToFile(filePath, true);
                return filePath;


            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO ZIP] Erro ao processar arquivo {zipName}: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempZipPath)) File.Delete(tempZipPath);
        }

        return null;
    }

    private async Task<string> DownloadGraphAttachmentAsync(string siteId, string listId, string itemId, string attachmentId, string fileName, string accessToken)
    {
        var downloadUrl = $"https://graph.microsoft.com/v1.0/sites/{siteId}/lists/{listId}/items/{itemId}/attachments/{attachmentId}/$value";

        using var httpClient = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return string.Empty;

        var contentStream = await response.Content.ReadAsStreamAsync();
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firmwares");
        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

        var filePath = Path.Combine(directoryPath, fileName);
        using (var fileStream = File.Create(filePath))
        {
            await contentStream.CopyToAsync(fileStream);
        }

        return filePath;
    }


    private static async Task<T> ExecuteWithGraphErrorHandlingAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (ODataError ex)
        {
            Console.WriteLine($"\n[ERRO DO GRAPH] Código: {ex.Error?.Code}");
            Console.WriteLine($"[ERRO DO GRAPH] Mensagem: {ex.Error?.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERRO REPOSITÓRIO FIRMWARE] {ex.Message}");
            throw;
        }
    }
}
