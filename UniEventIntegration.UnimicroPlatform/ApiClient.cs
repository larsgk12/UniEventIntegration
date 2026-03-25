using UniEventIntegration.Models;
using UniEventIntegration.UnimicroPlatformAuth;
using UniEventIntegration.UnimicroPlatformAuth.Models;
using UniEventIntegration.Utils.Extensions;

namespace UniEventIntegration.UnimicroPlatform;

public sealed class ApiClient(HttpClient httpClient, IAuthManager authManager)
{
    public async ValueTask<HttpClient?> GetHttpClientAsync(
        Uri apiEndpoint,
        Guid companyKey,
        bool ensureApiPath = true,
        CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(apiEndpoint);
        Guard.IsNotDefault(companyKey);

        var token = await authManager.GetTokenAsync(cancellationToken);
        if (!token.IsValid) return null;

        httpClient.DefaultRequestHeaders.Authorization = token.ToAuthHeader();
        httpClient.DefaultRequestHeaders.Add("companykey", companyKey.ToString());
        httpClient.BaseAddress = ensureApiPath
            ? apiEndpoint.AppendPathIfMissing("api")
            : apiEndpoint;
        return httpClient;
    }

    public ValueTask<HttpClient?> GetHttpClientAsync(
        string apiEndpoint,
        Guid companyKey,
        bool ensureApiPath = true,
        CancellationToken cancellationToken = default)
    {
        Guard.IsNotNullOrWhiteSpace(apiEndpoint);
        return GetHttpClientAsync(new Uri(apiEndpoint), companyKey, ensureApiPath, cancellationToken);
    }

    public async Task<Endpoints?> GetEndpointsAsync(Uri apiEndpoint, CancellationToken cancellationToken = default)
    {
        httpClient.BaseAddress = apiEndpoint.AppendPathIfMissing("api");
        var resp = await httpClient.GetAsync("endpoints", cancellationToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Endpoints>(cancellationToken: cancellationToken);
    }
}
