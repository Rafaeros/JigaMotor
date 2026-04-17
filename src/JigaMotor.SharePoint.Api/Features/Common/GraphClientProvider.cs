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
            string.IsNullOrWhiteSpace(_options.ClientId) ||
            string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new InvalidOperationException("As credenciais do Azure AD (TenantId, ClientId ou ClientSecret) estão ausentes no arquivo de configuração.");
        }

        // 2. Configura a credencial "App-Only" (Fluxo Client Credentials)
        var credential = new ClientSecretCredential(
            tenantId: _options.TenantId,
            clientId: _options.ClientId,
            clientSecret: _options.ClientSecret
        );

        // 3. O Escopo Mágico
        // ATENÇÃO: Para autenticação via ClientSecret, a Microsoft EXIGE que o escopo seja sempre este ".default".
        // Isso diz à Microsoft: "Olhe lá no portal do Azure quais permissões a TI me deu (Sites.ReadWrite.All) e aplique todas elas".
        var scopes = new[] { "https://graph.microsoft.com/.default" };

        // 4. Instancia e salva no cache da classe
        _graphClient = new GraphServiceClient(credential, scopes);

        return _graphClient;
    }
}