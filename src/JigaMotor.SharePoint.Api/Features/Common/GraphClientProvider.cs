using Azure.Identity;
using JigaMotor.SharePoint.Api.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace JigaMotor.SharePoint.Api.Features.Common;

public class GraphClientProvider(IOptions<SharePointOptions> options)
{
    private readonly SharePointOptions _options = options.Value;
    private GraphServiceClient? _graphClient;

    public GraphServiceClient GetAuthenticatedClient()
    {
        // Padrão Singleton: se já conectou antes, reutiliza a mesma conexão
        if (_graphClient != null) return _graphClient;

        // 1. Blindagem: Garante que o appsettings não está faltando nada
        if (string.IsNullOrWhiteSpace(_options.TenantId) ||
            string.IsNullOrWhiteSpace(_options.ClientId))
        {
            throw new InvalidOperationException("As credenciais do Azure AD (TenantId ou ClientId) estão ausentes no arquivo de configuração.");
        }

        // 2. Configura a credencial "Interactive" (Login via Navegador)
        var options = new InteractiveBrowserCredentialOptions
        {
            TenantId = _options.TenantId,
            ClientId = _options.ClientId,
            RedirectUri = new Uri("http://localhost:41473")
        };
        var credential = new InteractiveBrowserCredential(options);

        // 3. O Escopo Mágico
        var scopes = new[] { "https://graph.microsoft.com/.default" };

        // 4. Instancia e salva no cache da classe
        _graphClient = new GraphServiceClient(credential, scopes);

        return _graphClient;
    }

    public async Task<string> GetSharePointAccessTokenAsync()
    {
        var options = new InteractiveBrowserCredentialOptions
        {
            TenantId = _options.TenantId,
            ClientId = _options.ClientId,
            RedirectUri = new Uri("http://localhost:41473")
        };
        var credential = new InteractiveBrowserCredential(options);

        // Extrai o host de forma robusta (ex: "tenant.sharepoint.com")
        var rawUrl = _options.SiteUrl.Replace(":/", "/");
        if (!rawUrl.StartsWith("https://")) rawUrl = "https://" + rawUrl;
        
        var uri = new Uri(rawUrl);
        var host = uri.Host; 

        // O escopo para SharePoint REST API deve ser o host + /.default
        var scopes = new[] { $"https://{host}/.default" };

        var tokenRequestContext = new Azure.Core.TokenRequestContext(scopes);
        var token = await credential.GetTokenAsync(tokenRequestContext);

        return token.Token;
    }

    public async Task<string> GetGraphAccessTokenAsync()
    {
        var options = new InteractiveBrowserCredentialOptions
        {
            TenantId = _options.TenantId,
            ClientId = _options.ClientId,
            RedirectUri = new Uri("http://localhost")
        };
        var credential = new InteractiveBrowserCredential(options);

        var scopes = new[] { "https://graph.microsoft.com/.default" };
        var tokenRequestContext = new Azure.Core.TokenRequestContext(scopes);
        var token = await credential.GetTokenAsync(tokenRequestContext);

        return token.Token;
    }
}