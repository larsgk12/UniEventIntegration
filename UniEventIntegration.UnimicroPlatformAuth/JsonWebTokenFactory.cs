namespace UniEventIntegration.UnimicroPlatformAuth;

public interface IJsonWebTokenFactory
{
    /// <summary>
    /// Creates an instance of <see cref="IJsonWebTokenWrapper"/> from the specified token.
    /// </summary>
    /// <param name="token">The token string.</param>
    /// <returns>An instance of <see cref="IJsonWebTokenWrapper"/>.</returns>
    IJsonWebTokenWrapper Create(string token);
}

public sealed class JsonWebTokenFactory : IJsonWebTokenFactory
{
    /// <inheritdoc/>
    public IJsonWebTokenWrapper Create(string token)
    {
        var tokenHandler = new JsonWebTokenHandler();
        var jsonWebToken = tokenHandler.ReadJsonWebToken(token);
        return new JsonWebTokenWrapper(jsonWebToken);
    }
}
