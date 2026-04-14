using JigaMotor.Shared.Models;
using JigaMotor.Shared.Interfaces;
using JigaMotor.SharePoint.Api.Features.Common;
using Microsoft.Extensions.Options;
using JigaMotor.Shared.Config;

namespace JigaMotor.SharePoint.Api.Infraestructure
{
    public class SharePointDeviceRepository(
        GraphClientProvider graphProvider,
        IOptions<SharePointOptions> options) : IDeviceRepository
    {
        private readonly SharePointOptions _options = options.Value;

        public async Task<List<DeviceItem>> GetAllAsync()
        {
            try
            {
                var graphClient = graphProvider.GetAuthenticatedClient();

                // 1. RESOLVE O SITE DINAMICAMENTE
                // O Graph espera: hostname:/sites/caminho (SEM https://)
                var sanitizedUrl = _options.SiteUrl
                    .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("http://", "", StringComparison.OrdinalIgnoreCase);
                
                // Garante que existe apenas um ':' separando o host do caminho
                if (!sanitizedUrl.Contains(':') && sanitizedUrl.Contains('/'))
                {
                    var firstSlash = sanitizedUrl.IndexOf('/');
                    sanitizedUrl = sanitizedUrl.Insert(firstSlash, ":");
                }

                Console.WriteLine($"[DEBUG] Buscando site em: {sanitizedUrl}");
                var site = await graphClient.Sites[sanitizedUrl].GetAsync();
                
                if (site == null || string.IsNullOrEmpty(site.Id))
                {
                    throw new Exception($"Não foi possível encontrar o site SharePoint na URL: {_options.SiteUrl}");
                }
                
                var siteId = site.Id;
                Console.WriteLine($"[DEBUG] Site resolvido! ID: {siteId}");
                Console.WriteLine($"[DEBUG] Buscando lista: {_options.ListName}");
                var listResponse = await graphClient.Sites[siteId].Lists.GetAsync();
                var targetList = listResponse?.Value?.FirstOrDefault(l => 
                    l.DisplayName?.Equals(_options.ListName, StringComparison.OrdinalIgnoreCase) == true || 
                    l.Name?.Equals(_options.ListName, StringComparison.OrdinalIgnoreCase) == true);

                if (targetList == null || string.IsNullOrEmpty(targetList.Id))
                {
                    throw new Exception($"Lista '{_options.ListName}' não encontrada no site.");
                }

                var listId = targetList.Id;
                Console.WriteLine($"[DEBUG] Lista resolvida! ID: {listId}");

                var result = await graphClient.Sites[siteId].Lists[listId].Items.GetAsync(config =>
                {
                    config.QueryParameters.Expand = ["fields"];
                });

                var list = result?.Value?.Select(item =>
                {
                    string ObterCampo(string nome)
                    {
                        return item.Fields?.AdditionalData != null && item.Fields.AdditionalData.TryGetValue(nome, out var valor)
                            ? valor?.ToString() ?? "N/A"
                            : "N/A";
                    }

                    return new DeviceItem(
                        Id: item.Id ?? string.Empty,
                        Serie: ObterCampo("SERIE"),
                        DevEui: ObterCampo("DEVEUI"),
                        AppEui: ObterCampo("APPEUI"),
                        AppKey: ObterCampo("APPKEY")
                    );
                }).ToList() ?? [];

                return list;
            }
            catch (Microsoft.Graph.Models.ODataErrors.ODataError ex)
            {
                Console.WriteLine($"\n[ERRO DO GRAPH] Código: {ex.Error?.Code}");
                Console.WriteLine($"[ERRO DO GRAPH] Mensagem: {ex.Error?.Message}");
                if (ex.Error?.Details != null)
                {
                    foreach (var detail in ex.Error.Details)
                    {
                        Console.WriteLine($"[DETALHE] {detail.Code}: {detail.Message}");
                    }
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
}