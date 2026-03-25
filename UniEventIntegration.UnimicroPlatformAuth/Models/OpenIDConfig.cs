namespace UniEventIntegration.UnimicroPlatformAuth.Models;

/// <summary>
/// Represents the OpenID configuration.
/// </summary>
internal sealed record OpenIDConfig([property: JsonPropertyName("token_endpoint")] Uri TokenEndpoint);
