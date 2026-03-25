namespace UniEventIntegration.UnimicroPlatformAuth.Models;

/// <summary>
/// Represents the payload of a token.
/// </summary>
internal sealed record TokenPayload(
    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    [property: JsonPropertyName("access_token")]
    string? Token,

    /// <summary>
    /// Gets or sets the expiration time of the token in seconds.
    /// </summary>
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn,

    /// <summary>
    /// Gets or sets the type of the token.
    /// </summary>
    [property: JsonPropertyName("token_type")]
    string? TokenType,

    /// <summary>
    /// Gets or sets the scope of the token.
    /// </summary>
    [property: JsonPropertyName("scope")]
    string? Scope);
