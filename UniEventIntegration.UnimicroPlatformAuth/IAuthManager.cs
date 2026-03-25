using UniEventIntegration.UnimicroPlatformAuth.Models;

namespace UniEventIntegration.UnimicroPlatformAuth;

public interface IAuthManager
{
    ValueTask<TokenResult> GetTokenAsync(CancellationToken cancellationToken = default);

    ValueTask<bool> IsTokenValidAsync(string? token = null);

    ValueTask<IJsonWebTokenWrapper?> GetTokenAsJwtAsync(string? token = null);

    ValueTask<IReadOnlyDictionary<string, string>> GetClaimsAsync(string? token = null);

    ValueTask InvalidateToken();
    ValueTask SetToken(string token);
}
