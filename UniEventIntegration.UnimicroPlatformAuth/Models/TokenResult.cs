namespace UniEventIntegration.UnimicroPlatformAuth.Models;

/// <summary>
/// Represents the result of a token operation.
/// </summary>
public record TokenResult(bool IsValid, string? Token, string? Message, DateTime? ValidToUTC)
{
    /// <summary>
    /// Creates a valid token result.
    /// </summary>
    /// <param name="token">The token value.</param>
    /// <param name="validToUTC">The UTC date and time when the token is valid until.</param>
    /// <returns>A valid token result.</returns>
    public static TokenResult Valid(string token, DateTime validToUTC)
        => new(true, token, null, validToUTC);

    /// <summary>
    /// Creates an invalid token result.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>An invalid token result.</returns>
    public static TokenResult NotValid(string message)
        => new(false, null, message, null);
};

/// <summary>
/// Provides extension methods for the <see cref="TokenResult"/> class.
/// </summary>
public static class TokenResultExtensions
{
    /// <summary>
    /// Converts the <see cref="TokenResult"/> to an <see cref="AuthenticationHeaderValue"/> for use in HTTP headers.
    /// </summary>
    /// <param name="result">The token result.</param>
    /// <returns>An <see cref="AuthenticationHeaderValue"/> or null if the token result is invalid.</returns>
    public static AuthenticationHeaderValue? ToAuthHeader(this TokenResult result)
    {
        if (result is null || !result.IsValid) return null;
        return new AuthenticationHeaderValue("Bearer", result.Token);
    }
}
