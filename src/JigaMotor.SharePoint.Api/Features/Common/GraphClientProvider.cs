using Microsoft.Graph;
using Azure.Identity;
using Microsoft.Extensions.Options;
using JigaMotor.Shared.Config;

namespace JigaMotor.SharePoint.Api.Features.Common;

public class GraphClientProvider(IOptions<SharePointOptions> options)
{
    private readonly SharePointOptions _options = options.Value;
    private GraphServiceClient? _graphClient;

    public GraphServiceClient GetAuthenticatedClient()
    {
        if (_graphClient != null) return _graphClient;

        Console.WriteLine($"[DEBUG-CONFIG] Tenant lido: '{_options.TenantId}'");
        Console.WriteLine($"[DEBUG-CONFIG] Client lido: '{_options.ClientId}'");

        var options = new InteractiveBrowserCredentialOptions
        {
            TenantId = _options.TenantId,
            ClientId = _options.ClientId,
        };

        if (!string.IsNullOrEmpty(_options.RedirectUri) && _options.RedirectUri.Contains("localhost"))
        {
            options.RedirectUri = new Uri(_options.RedirectUri);
        }
        else
        {
            Console.WriteLine($"[AVISO] RedirectUri '{_options.RedirectUri}' ignorado no modo interativo. Usando padrão http://localhost para teste local.");
        }

        var credential = new InteractiveBrowserCredential(options);
        var scopes = new[] { "https://graph.microsoft.com/.default" };

        _graphClient = new GraphServiceClient(credential, scopes);
        return _graphClient;
    }
}