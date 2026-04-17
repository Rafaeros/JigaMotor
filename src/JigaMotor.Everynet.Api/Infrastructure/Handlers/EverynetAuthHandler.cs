using JigaMotor.Everynet.Api.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace JigaMotor.Everynet.Api.Infrastructure.Handlers
{
    public class EverynetAuthHandler(IOptions<EverynetOptions> options) : DelegatingHandler
    {
        private readonly string _accessToken = options.Value.AccessToken;


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri is null)
            {
                throw new InvalidOperationException("A URI da requisição não pode ser nula ao tentar injetar o token da Everynet.");
            }

            var uriBuilder = new UriBuilder(request.RequestUri);

            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

            query["access_token"] = _accessToken;

            uriBuilder.Query = query.ToString();

            request.RequestUri = uriBuilder.Uri;

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
