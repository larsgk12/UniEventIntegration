namespace UniEventIntegration.UnimicroPlatformAuth;

/// <summary>
/// Represents an interface for a JSON Web Token wrapper.
/// </summary>
public interface IJsonWebTokenWrapper
{
    /// <summary>
    /// Gets the claims associated with the JSON Web Token.
    /// </summary>
    IEnumerable<Claim> Claims { get; }

    /// <summary>
    /// Gets the expiration date and time of the JSON Web Token.
    /// </summary>
    DateTime ValidTo { get; }
}

/// <summary>
/// Represents a wrapper for a JSON Web Token.
/// </summary>
/// <param name="jsonWebToken">The JSON Web Token to wrap.</param>
public sealed class JsonWebTokenWrapper(JsonWebToken jsonWebToken) : IJsonWebTokenWrapper
{
    /// <inheritdoc/>
    public IEnumerable<Claim> Claims => jsonWebToken.Claims;

    /// <inheritdoc/>
    public DateTime ValidTo => jsonWebToken.ValidTo;
}
