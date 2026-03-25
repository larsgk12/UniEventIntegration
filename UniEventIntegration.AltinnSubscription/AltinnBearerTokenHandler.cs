using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;


namespace UniEventIntegration.AltinnSubscription
{
    public class AltinnBearerTokenHandler : DelegatingHandler
    {
        private readonly IAltinnTokenProvider _tokenProvider;

        public AltinnBearerTokenHandler(IAltinnTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _tokenProvider.GetTokenAsync(cancellationToken);
            
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }

    public interface IAltinnTokenProvider
    {
        Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
    }

    public class AltinnTokenProvider : IAltinnTokenProvider
    {
        private readonly IConfiguration _configuration;

        public AltinnTokenProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
        {
            // Option 1: Get from configuration/appsettings
            var token = _configuration["Altinn:BearerToken"] ?? string.Empty;
            
            // Option 2: Get from a secure token service/vault
            // var token = await _tokenService.GetTokenAsync();
            
            return Task.FromResult(token);
        }
    }
}