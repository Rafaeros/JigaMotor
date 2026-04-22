using JigaMotor.SharePoint.Api.Domain.Entities;
using JigaMotor.SharePoint.Api.Domain.Interfaces;
using JigaMotor.SharePoint.Api.Features.Common;
using JigaMotor.SharePoint.Api.Infrastructure.Configuration;
using JigaMotor.SharePoint.Api.Infrastructure.Mappers;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using System.Net.Http.Headers;

namespace JigaMotor.SharePoint.Api.Infrastructure;

public class SharePointDeviceRepository(
    GraphClientProvider graphProvider,
    IHttpClientFactory httpClientFactory,
    IOptions<SharePointOptions> options) : IDeviceRepository
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly SharePointOptions _options = options.Value;
    private string? _cachedSiteId;
    private string? _cachedListId;
    public async Task<List<DeviceProductionRecord>> GetAllAsync()
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);
            var allDevices = new List<DeviceProductionRecord>();

            await FetchPaginatedItemsAsync(client, siteId, listId, (item) =>
            {
                var device = SharePointDeviceMapper.ToDomain(item);
                allDevices.Add(device);
            });

            return allDevices;
        });
    }

    public async Task<DeviceProductionRecord?> GetByDevEuiAsync(string DevEui)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);
            var result = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
            {
                config.QueryParameters.Expand = ["fields"];
                config.QueryParameters.Filter = $"fields/DEVEUI eq '{DevEui}'";
                config.QueryParameters.Top = 1;
            });
            var item = result?.Value?.FirstOrDefault();
            return item != null ? SharePointDeviceMapper.ToDomain(item) : null;
        });
    }

    public async Task<DeviceProductionRecord?> GetBySerieAsync(string serie)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);
            var result = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
            {
                config.QueryParameters.Expand = ["fields"];
                config.QueryParameters.Filter = $"fields/SERIE eq '{serie}'";
                config.QueryParameters.Top = 1;
            });
            var item = result?.Value?.FirstOrDefault();
            return item != null ? SharePointDeviceMapper.ToDomain(item) : null;
        });
    }

    public async Task<DeviceProductionRecord> GetByLoraAsync(string LoraId)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);
            var result = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
            {
                config.QueryParameters.Expand = ["fields"];
                config.QueryParameters.Filter = $"fields/LORA eq '{LoraId}'";
                config.QueryParameters.Top = 1;
            });
            var item = result?.Value?.FirstOrDefault() ?? throw new Exception($"Dispositivo com LoRa ID '{LoraId}' não encontrado.");
            return SharePointDeviceMapper.ToDomain(item);
        });
    }

    public async Task<bool> ExistsByDevEuiAsync(string DevEui)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);
            var result = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
            {
                config.QueryParameters.Expand = ["fields"];
                config.QueryParameters.Filter = $"fields/DEVEUI eq '{DevEui}'";
                config.QueryParameters.Top = 1;
            });
            return result?.Value?.Count > 0;
        });
    }

    public async Task<bool> ExistsByKeysAsync(string devEui, string lora, string serie)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);
            var result = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
            {
                config.QueryParameters.Expand = ["fields"];
                config.QueryParameters.Filter = $"fields/DEVEUI eq '{devEui}' or fields/LORA eq '{lora}' or fields/SERIE eq '{serie}'";
                config.QueryParameters.Top = 1;
            });
            return result?.Value?.Count > 0;
        });
    }

    public async Task<bool> ExistsByLoraOrSerieAsync(string lora, string serie)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);
            var result = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
            {
                config.QueryParameters.Expand = ["fields"];
                config.QueryParameters.Filter = $"fields/LORA eq '{lora}' or fields/SERIE eq '{serie}'";
                config.QueryParameters.Top = 1;
            });
            return result?.Value?.Count > 0;
        });
    }

    public async Task<DeviceProductionRecord> AddInitialAsync(DeviceProductionRecord device)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);

            var listItem = new ListItem
            {
                Fields = new FieldValueSet
                {
                    AdditionalData = SharePointDeviceMapper.ToSharePoint(device)
                }
            };

            var createdItem = await client.Sites[siteId].Lists[listId].Items.PostAsync(listItem) ?? throw new Exception("Falha ao criar o dispositivo. O retorno do SharePoint foi nulo.");

            // Precisamos buscar de novo para ter os dados mapeados completos (ou mapear direto se preferir)
            var expandedItem = await client.Sites[siteId].Lists[listId].Items[createdItem.Id].GetAsync(config =>
            {
                config.QueryParameters.Expand = ["fields"];
            });

            return SharePointDeviceMapper.ToDomain(expandedItem ?? createdItem);
        });
    }

    public async Task<bool> UpdateHardwareTestsAsync(string identifier, HardwareTests tests)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);

            var item = await FindItemByIdentifierAsync(client, siteId, listId, identifier);
            if (item == null) return false;

            var data = new Dictionary<string, object>
            {
                { "DATA", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") }
            };

            if (!string.IsNullOrWhiteSpace(tests.P2pStatus)) data.Add("P2P", tests.P2pStatus);
            if (!string.IsNullOrWhiteSpace(tests.AdcStatus)) data.Add("ADC", tests.AdcStatus);
            if (!string.IsNullOrWhiteSpace(tests.EverynetStatus)) data.Add("EVERYNET", tests.EverynetStatus);
            if (!string.IsNullOrWhiteSpace(tests.OtpStatus)) data.Add("OTP", tests.OtpStatus);
            if (!string.IsNullOrWhiteSpace(tests.FirmwareStatus)) data.Add("FW", tests.FirmwareStatus);

            if (data.Count == 1) return true; // Somente DATA, nada a atualizar

            var updateFields = new FieldValueSet { AdditionalData = data };
            await client.Sites[siteId].Lists[listId].Items[item.Id].Fields.PatchAsync(updateFields);
            return true;
        });
    }

    public async Task<bool> UpdatePowerConsumptionTestsAsync(string identifier, PowerConsumptionTests tests)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);

            var item = await FindItemByIdentifierAsync(client, siteId, listId, identifier);
            if (item == null) return false;

            var data = new Dictionary<string, object>
            {
                { "DATA", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") }
            };

            if (!string.IsNullOrWhiteSpace(tests.LrwEmOnStatus)) data.Add("LRWEMON", tests.LrwEmOnStatus);
            if (!string.IsNullOrWhiteSpace(tests.AdvEmOnStatus)) data.Add("ADVEMON", tests.AdvEmOnStatus);
            if (!string.IsNullOrWhiteSpace(tests.PacketEmOnStatus)) data.Add("PACKETEMON", tests.PacketEmOnStatus);
            if (!string.IsNullOrWhiteSpace(tests.LrwEmOffStatus)) data.Add("LRWEMOFF", tests.LrwEmOffStatus);
            if (!string.IsNullOrWhiteSpace(tests.AdvEmOffStatus)) data.Add("ADVEMOFF", tests.AdvEmOffStatus);
            if (!string.IsNullOrWhiteSpace(tests.PacketEmOffStatus)) data.Add("PACKETEMOFF", tests.PacketEmOffStatus);

            if (data.Count == 1) return true;

            var updateFields = new FieldValueSet { AdditionalData = data };
            await client.Sites[siteId].Lists[listId].Items[item.Id].Fields.PatchAsync(updateFields);
            return true;
        });
    }

    public async Task<bool> UpdateProductionMetadataAsync(string identifier, ProductionMetadata metadata)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);

            var item = await FindItemByIdentifierAsync(client, siteId, listId, identifier);
            if (item == null) return false;

            var data = new Dictionary<string, object>
            {
                { "DATA", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") }
            };

            if (!string.IsNullOrWhiteSpace(metadata.BoxNumber)) data.Add("Caixa", metadata.BoxNumber);
            if (!string.IsNullOrWhiteSpace(metadata.ProductionOrder)) data.Add("OrdemProducao", metadata.ProductionOrder);
            if (!string.IsNullOrWhiteSpace(metadata.TagStatus)) data.Add("TAG", metadata.TagStatus);
            if (!string.IsNullOrWhiteSpace(metadata.RdpStatus)) data.Add("RDP", metadata.RdpStatus);
            if (metadata.PunchDate.HasValue) data.Add("DataBatida", metadata.PunchDate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"));

            if (data.Count == 1) return true;

            var updateFields = new FieldValueSet { AdditionalData = data };
            await client.Sites[siteId].Lists[listId].Items[item.Id].Fields.PatchAsync(updateFields);
            return true;
        });
    }

    public async Task<int> GetMaxLoraIdAsync()
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);

            var result = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
            {
                config.QueryParameters.Expand = ["fields"];
                config.QueryParameters.Top = 1;
                config.QueryParameters.Orderby = ["fields/LORA desc"];
            });

            var item = result?.Value?.FirstOrDefault();
            if (item == null) return 0;

            var loraStr = item.Fields?.AdditionalData != null && item.Fields.AdditionalData.TryGetValue("LORA", out var value)
                ? value?.ToString()
                : null;

            return int.TryParse(loraStr, out int loraId) ? loraId : 0;
        });
    }

    public async Task<int> GetMaxDomainIdAsync()
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);

            var result = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
            {
                config.QueryParameters.Expand = ["fields"];
                config.QueryParameters.Top = 1;
                config.QueryParameters.Orderby = ["fields/N desc"];
            });

            var item = result?.Value?.FirstOrDefault();
            if (item == null) return 0;

            var nStr = item.Fields?.AdditionalData != null && item.Fields.AdditionalData.TryGetValue("N", out var value)
                ? value?.ToString()
                : null;

            return int.TryParse(nStr, out int id) ? id : 0;
        });
    }

    public async Task<bool> DeleteByDevEuiAsync(string devEui)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);

            var searchResult = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
            {
                config.QueryParameters.Filter = $"fields/DEVEUI eq '{devEui}'";
                config.QueryParameters.Top = 1;
            });

            var item = searchResult?.Value?.FirstOrDefault();
            if (item == null) return false;

            await client.Sites[siteId].Lists[listId].Items[item.Id].DeleteAsync();
            return true;
        });
    }

    public async Task<bool> AddAttachmentAsync(string identifier, string fileName, byte[] content)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);

            // 1. Encontrar o item Id pelo identificador (DevEui, Serie ou Lora)
            var searchResult = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
            {
                config.QueryParameters.Expand = ["fields"];
                config.QueryParameters.Filter = $"fields/DEVEUI eq '{identifier}' or fields/SERIE eq '{identifier}' or fields/LORA eq '{identifier}'";
                config.QueryParameters.Top = 1;
            });

            var item = searchResult?.Value?.FirstOrDefault();
            if (item == null) return false;

            var accessToken = await graphProvider.GetSharePointAccessTokenAsync();


            var baseUrl = _options.SiteUrl.Replace(":/", "/");
            if (!baseUrl.StartsWith("https://")) baseUrl = "https://" + baseUrl;
            
            var requestUrl = $"{baseUrl}/_api/web/lists/getbytitle('{_options.ListName}')/items({item.Id})/AttachmentFiles/add(FileName='{fileName}')";

            using var httpClient = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            // SharePoint REST API prefere odata=verbose para garantir compatibilidade
            // Usamos TryAddWithoutValidation porque o .NET pode validar o ";" como caractere inválido no construtor
            request.Headers.Accept.Clear();
            request.Headers.TryAddWithoutValidation("Accept", "application/json;odata=verbose");
            
            request.Content = new ByteArrayContent(content);

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;
                throw new Exception($"[REST ERROR] Status: {statusCode} ({response.StatusCode}). Body: {errorBody}");
            }

            return true;
        });
    }

    public async Task<DeviceProductionRecord?> GetByIdentifierAsync(string identifier)
    {
        return await ExecuteWithGraphErrorHandlingAsync(async () =>
        {
            var client = graphProvider.GetAuthenticatedClient();
            var (siteId, listId) = await ResolveSiteAndListAsync(client);
            var result = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
            {
                config.QueryParameters.Expand = ["fields"];
                config.QueryParameters.Filter = $"fields/DEVEUI eq '{identifier}' or fields/SERIE eq '{identifier}' or fields/LORA eq '{identifier}'";
                config.QueryParameters.Top = 1;
            });
            var item = result?.Value?.FirstOrDefault();
            return item != null ? SharePointDeviceMapper.ToDomain(item) : null;
        });
    }


    /// <summary>
    /// Resolve e faz o cache dos IDs do Site e da Lista no SharePoint.
    /// </summary>
    private async Task<(string SiteId, string ListId)> ResolveSiteAndListAsync(GraphServiceClient client)
    {
        if (!string.IsNullOrEmpty(_cachedSiteId) && !string.IsNullOrEmpty(_cachedListId))
            return (_cachedSiteId, _cachedListId);

        var site = await client.Sites[_options.SiteUrl].GetAsync()
            ?? throw new Exception($"Site SharePoint não encontrado: {_options.SiteUrl}");
        _cachedSiteId = site.Id;

        var listResponse = await client.Sites[_cachedSiteId].Lists.GetAsync();
        var targetList = listResponse?.Value?.FirstOrDefault(l =>
            l.DisplayName?.Equals(_options.ListName, StringComparison.OrdinalIgnoreCase) == true ||
            l.Name?.Equals(_options.ListName, StringComparison.OrdinalIgnoreCase) == true)
            ?? throw new Exception($"Lista '{_options.ListName}' não encontrada.");

        _cachedListId = targetList.Id;

        if (string.IsNullOrEmpty(_cachedSiteId) || string.IsNullOrEmpty(_cachedListId))
            throw new Exception("Não foi possível resolver os IDs do Site ou da Lista.");

        return (_cachedSiteId, _cachedListId);
    }

    /// <summary>
    /// Encapsula a complexidade do PageIterator do Graph SDK.
    /// </summary>
    private static async Task FetchPaginatedItemsAsync(
        GraphServiceClient client,
        string siteId,
        string listId,
        Action<ListItem> onItemProcessed)
    {
        var initialResponse = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
        {
            config.QueryParameters.Expand = ["fields"];
            config.QueryParameters.Top = 999;
        }) ?? throw new Exception("A resposta inicial do SharePoint retornou nula.");

        var pageIterator = PageIterator<ListItem, ListItemCollectionResponse>.CreatePageIterator(
            client,
            initialResponse,
            (item) =>
            {
                onItemProcessed(item);
                return true;
            });


        await pageIterator.IterateAsync();
    }

    private static async Task<ListItem?> FindItemByIdentifierAsync(GraphServiceClient client, string siteId, string listId, string identifier)
    {
        var result = await client.Sites[siteId].Lists[listId].Items.GetAsync(config =>
        {
            config.QueryParameters.Expand = ["fields"];
            config.QueryParameters.Filter = $"fields/DEVEUI eq '{identifier}' or fields/SERIE eq '{identifier}' or fields/LORA eq '{identifier}'";
            config.QueryParameters.Top = 1;
        });
        return result?.Value?.FirstOrDefault();
    }

    /// <summary>
    /// Um Interceptador para não repetirmos o bloco Try-Catch em todos os métodos do repositório.
    /// </summary>
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
            if (ex.Error?.Details != null)
            {
                foreach (var detail in ex.Error.Details)
                    Console.WriteLine($"[DETALHE] {detail.Code}: {detail.Message}");
            }
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERRO REPOSITÓRIO] {ex.Message}");
            throw;
        }
    }
}