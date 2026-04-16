using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using JigaMotor.SharePoint.Api.Features.Common;
using JigaMotor.SharePoint.Api.Infrastructure.Configuration;
using JigaMotor.SharePoint.Api.Infrastructure.Mappers;
using JigaMotor.SharePoint.Api.Domain.Entities;
using JigaMotor.SharePoint.Api.Domain.Interfaces;

namespace JigaMotor.SharePoint.Api.Infrastructure;

public class SharePointDeviceRepository(
    GraphClientProvider graphProvider,
    IOptions<SharePointOptions> options) : IDeviceRepository
{
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