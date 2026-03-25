using UniEventIntegration.Helpers;
using UniEventIntegration.Options;
using UniEventIntegration.UnimicroPlatformAuth.Models;
using UniEventIntegration.Utils.Extensions;

namespace UniEventIntegration.UnimicroPlatformAuth;

public sealed class AuthManager(
    IHttpClientFactory httpClientFactory,
    IJsonWebTokenFactory tokenFactory,
    IOptionsMonitor<AuthManagerOptions> options,
    ILogger<AuthManager>? logger) : IAuthManager, IDisposable
{
    /// <summary>
    /// SemaphoreSlim used to synchronize access to the token.
    /// </summary>
    private readonly SemaphoreSlim _tokenSemaphore = new(1);

    /// <summary>
    /// The current token.
    /// </summary>
    private string? _token;

    /// <summary>
    /// The UTC time when the token is valid until.
    /// </summary>
    private DateTime _tokenValidToUTC;

    /// <summary>
    /// Retrieves the claims from the specified token.
    /// </summary>
    /// <param name="token">The token to retrieve the claims from. If not specified, the current token will be used.</param>
    /// <returns>An enumerable collection of key-value pairs representing the claims.</returns>
    public async ValueTask<IReadOnlyDictionary<string, string>> GetClaimsAsync(string? token = null)
    {
        var jwt = await GetTokenAsJwtAsync(token).ConfigureAwait(false);
        return jwt is null || jwt.Claims is null
            ? []
            : jwt.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);
    }

    /// <summary>
    /// Retrieves the token as a JSON Web Token (JWT) wrapper.
    /// </summary>
    /// <param name="token">The token to retrieve as a JWT. If not specified, the current token will be used.</param>
    /// <returns>The token as a JWT wrapper.</returns>
    public async ValueTask<IJsonWebTokenWrapper?> GetTokenAsJwtAsync(string? token = null)
    {
        await _tokenSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            var tmpToken = string.IsNullOrWhiteSpace(token)
                ? _token
                : token;
            return string.IsNullOrWhiteSpace(tmpToken) ? default : tokenFactory.Create(tmpToken);
        }
        catch (Exception ex)
        {
            if (logger is not null)
                AuthManagerLogging.TokenValidationError(logger, ex);
            return default;
        }
        finally { _tokenSemaphore.Release(); }
    }

    /// <summary>
    /// Checks if the token is valid.
    /// </summary>
    /// <param name="token">The token to check. If not specified, the current token will be used.</param>
    /// <returns>True if the token is valid, false otherwise.</returns>
    public async ValueTask<bool> IsTokenValidAsync(string? token = null)
        => await GetTokenAsJwtAsync(token).ConfigureAwait(false) switch
        {
            var jwt when jwt is not null && jwt.ValidTo > DateTime.UtcNow.AddMinutes(5) => true,
            _ => false
        };

    /// <summary>
    /// Retrieves a valid token asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="TokenResult"/> representing the result of the token retrieval.</returns>
    public async ValueTask<TokenResult> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (await IsTokenValidAsync().ConfigureAwait(false))
            return await GetValidTokenAsync().ConfigureAwait(false);
        try
        {
            if (options.CurrentValue.IdentityUri is null)
                throw new InvalidOperationException("Identity endpoint was not provided ['IdentityUri']");
            if (string.IsNullOrWhiteSpace(options.CurrentValue.ClientId))
                throw new InvalidOperationException("Client id was not provided ['ClientId']");
            if (string.IsNullOrWhiteSpace(options.CurrentValue.Scopes))
                throw new InvalidOperationException("Scope was not provided ['Scope']");
            // Prio 1: Certificate
            if (!string.IsNullOrWhiteSpace(options.CurrentValue.Certificate))
                return await GetTokenByCertificateAsync(cancellationToken).ConfigureAwait(false);

            // Prio 2: CertificatePath/CertificatePwd
            if (!string.IsNullOrWhiteSpace(options.CurrentValue.CertificatePath)
                && !string.IsNullOrWhiteSpace(options.CurrentValue.CertificatePwd))
            {
                return await GetTokenByCertificatePathAsync(cancellationToken).ConfigureAwait(false);
            }

            // Prio 3: Thumbprint
            if (!string.IsNullOrWhiteSpace(options.CurrentValue.Thumbprint))
                return await GetTokenByThumbprintAsync(cancellationToken).ConfigureAwait(false);

            // Prio 4: UserName/Password
            return !string.IsNullOrWhiteSpace(options.CurrentValue.ClientSecret)
                && !string.IsNullOrWhiteSpace(options.CurrentValue.UserName)
                && !string.IsNullOrWhiteSpace(options.CurrentValue.Password)
                ? await GetTokenByCredentialsAsync(cancellationToken).ConfigureAwait(false)
                : await GetTokenThroughPKCEAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (logger is not null)
                AuthManagerLogging.AuthenticationFailed(logger, ex);
            return TokenResult.NotValid(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a valid token.
    /// </summary>
    /// <returns>A <see cref="TokenResult"/> representing the result of the token retrieval.</returns>
    private async Task<TokenResult> GetValidTokenAsync()
    {
        await _tokenSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return TokenResult.Valid(_token!, _tokenValidToUTC);
        }
        finally { _tokenSemaphore.Release(); }
    }

    /// <summary>
    /// Sets a valid token.
    /// </summary>
    /// <param name="token">The token to set.</param>
    /// <param name="expiresIn">The expiration time of the token in seconds.</param>
    /// <returns>A <see cref="TokenResult"/> representing the result of the token setting.</returns>
    private async Task<TokenResult> SetValidTokenAsync(string token, int expiresIn)
    {
        await _tokenSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            _token = token;
            _tokenValidToUTC = DateTime.UtcNow.AddSeconds(expiresIn).AddMinutes(-5);
            return TokenResult.Valid(_token!, _tokenValidToUTC);
        }
        finally { _tokenSemaphore.Release(); }
    }

    /// <summary>
    /// Retrieves a token using a certificate asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="TokenResult"/> representing the result of the token retrieval.</returns>
    private async Task<TokenResult> GetTokenByCertificateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Span<byte> input = Convert.FromBase64String(options.CurrentValue.Certificate!);
            using var cert = X509CertificateLoader.LoadPkcs12(input, null, X509KeyStorageFlags.EphemeralKeySet)
                ?? throw new InvalidOperationException("Could not interpret provided certificate.");

            var tokenEndpoint = await GetTokenEndpoint(cancellationToken).ConfigureAwait(false);
            var clientToken = CreateClientToken(tokenEndpoint, cert);
            TokenPayload response = await InternalGetToken(tokenEndpoint, clientToken, cancellationToken).ConfigureAwait(false);
            return await SetValidTokenAsync(response.Token!, response.ExpiresIn).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (logger is not null)
                AuthManagerLogging.AuthenticationFailed(logger, ex);
            return TokenResult.NotValid(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a token using a certificate path asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="TokenResult"/> representing the result of the token retrieval.</returns>
    private async Task<TokenResult> GetTokenByCertificatePathAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var cert = X509CertificateLoader.LoadPkcs12FromFile(options.CurrentValue.CertificatePath!, options.CurrentValue.CertificatePwd!, X509KeyStorageFlags.EphemeralKeySet)
                ?? throw new InvalidOperationException("Could not load a valid certificate.");

            var tokenEndpoint = await GetTokenEndpoint(cancellationToken).ConfigureAwait(false);
            var clientToken = CreateClientToken(tokenEndpoint, cert);
            var response = await InternalGetToken(tokenEndpoint, clientToken, cancellationToken).ConfigureAwait(false);
            return await SetValidTokenAsync(response.Token!, response.ExpiresIn).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (logger is not null)
                AuthManagerLogging.AuthenticationFailed(logger, ex);
            return TokenResult.NotValid(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the token endpoint asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The token endpoint.</returns>
    private async Task<Uri> GetTokenEndpoint(CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = options.CurrentValue.IdentityUri;
        client.Timeout = options.CurrentValue.RequestTimeout;
        if (options.CurrentValue.MaxResponseContentBufferSize.HasValue)
            client.MaxResponseContentBufferSize = options.CurrentValue.MaxResponseContentBufferSize.Value;
        var response = await client.GetAsync(".well-known/openid-configuration", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var config = await response.Content.ReadFromJsonAsync<OpenIDConfig>(cancellationToken: cancellationToken).ConfigureAwait(false);
        return config is null || config.TokenEndpoint is null
            ? throw new InvalidOperationException("Could not get openid configuration")
            : config.TokenEndpoint;
    }

    /// <summary>
    /// Retrieves a token using a thumbprint asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="TokenResult"/> representing the result of the token retrieval.</returns>
    private async Task<TokenResult> GetTokenByThumbprintAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var cert = await CertificateHelper.GetCertificateAsync(options.CurrentValue.Thumbprint!).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Could not load a valid certificate.");

            var tokenEndpoint = await GetTokenEndpoint(cancellationToken).ConfigureAwait(false);
            var clientToken = CreateClientToken(tokenEndpoint, cert);
            var response = await InternalGetToken(tokenEndpoint, clientToken, cancellationToken).ConfigureAwait(false);

            return await SetValidTokenAsync(response.Token!, response.ExpiresIn).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (logger is not null)
                AuthManagerLogging.AuthenticationFailed(logger, ex);
            return TokenResult.NotValid(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a token internally.
    /// </summary>
    /// <param name="tokenEndpoint">The token endpoint.</param>
    /// <param name="clientToken">The client token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="TokenPayload"/> representing the token response.</returns>
    private async Task<TokenPayload> InternalGetToken(Uri tokenEndpoint, string clientToken, CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();
        client.Timeout = options.CurrentValue.RequestTimeout;
        if (options.CurrentValue.MaxResponseContentBufferSize.HasValue)
            client.MaxResponseContentBufferSize = options.CurrentValue.MaxResponseContentBufferSize.Value;

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
                { "client_assertion", clientToken },
                { "client_id", options.CurrentValue.ClientId },
                { "scope", options.CurrentValue.Scopes }
            });

        var response = await client.PostAsync(tokenEndpoint, content, cancellationToken).ConfigureAwait(false);
        string? errorResponse;
        if (response.IsSuccessStatusCode)
        {
            var tokenPayload = await response.Content.ReadFromJsonAsync<TokenPayload>(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (tokenPayload is not null) return tokenPayload;
            errorResponse = "HTTP status was OK, but a 'TokenPayload' could not be deserialized.";
        }
        else
        {
            var error = await response.Content.ReadFromJsonAsync<TokenErrorResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
            errorResponse = error?.ToString() ?? "Unknown error";
        }
        throw new InvalidOperationException($"Could not get a valid token: {errorResponse}");
    }

    /// <summary>
    /// Retrieves a token using credentials asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="TokenResult"/> representing the result of the token retrieval.</returns>
    private async Task<TokenResult> GetTokenByCredentialsAsync(CancellationToken cancellationToken)
    {
        List<KeyValuePair<string, string>> pairs =
        [
            new("client_id", options.CurrentValue.ClientId),
            new("client_secret", options.CurrentValue.ClientSecret!),
            new("grant_type", "password"),
            new("username", options.CurrentValue.UserName!),
            new("password", options.CurrentValue.Password!),
            new("scope", options.CurrentValue.Scopes)
        ];
        try
        {
            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = options.CurrentValue.IdentityUri.AppendSlash();
            client.Timeout = options.CurrentValue.RequestTimeout;
            if (options.CurrentValue.MaxResponseContentBufferSize.HasValue)
                client.MaxResponseContentBufferSize = options.CurrentValue.MaxResponseContentBufferSize.Value;
            using var resp = await client.PostAsync("connect/token", new FormUrlEncodedContent(pairs), cancellationToken).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            var tokenPayload = await resp.Content.ReadFromJsonAsync<TokenPayload>(cancellationToken: cancellationToken).ConfigureAwait(false);
            return !string.IsNullOrWhiteSpace(tokenPayload?.Token)
                ? await SetValidTokenAsync(tokenPayload.Token, tokenPayload.ExpiresIn).ConfigureAwait(false)
                : TokenResult.NotValid("No valid token was returned.");
        }
        catch (Exception ex)
        {
            if (logger is not null)
                AuthManagerLogging.AuthenticationFailed(logger, ex);
            return TokenResult.NotValid(ex.Message);
        }
    }

    /// <summary>
    /// Creates a client token.
    /// </summary>
    /// <param name="audience">The audience of the token.</param>
    /// <param name="certificate">The certificate used to sign the token.</param>
    /// <returns>The client token.</returns>
    private string CreateClientToken(Uri audience, X509Certificate2 certificate)
    {
        Guard.IsNotNull(audience);
        Guard.IsNotNull(certificate);

        var now = DateTime.UtcNow;
        var securityKey = new X509SecurityKey(certificate);
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
        var descr = new SecurityTokenDescriptor()
        {
            Issuer = options.CurrentValue.ClientId,
            Audience = audience.ToString(),
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(1),
            SigningCredentials = signingCredentials,
            Claims = new Dictionary<string, object>
            {
                ["sub"] = options.CurrentValue.ClientId,
                ["jti"] = Guid.NewGuid().ToString(),
            }
        };
        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(descr);
    }

    private async Task<TokenResult> GetTokenThroughPKCEAsync(CancellationToken cancellationToken)
    {
        try
        {
            var state = GetRandomString(32);
            var code_verifier = GetRandomString(32);
            var code_challenge = GetCodeChallenge(code_verifier);
            var redirect_uri = options.CurrentValue.RedirectUri;

            var authUri = options.CurrentValue.IdentityUri.AppendSlash();
            var bldr = new StringBuilder(authUri!.AbsoluteUri);
            bldr.Append("connect/authorize");
            bldr.Append("?response_type=code");
            bldr.Append("&scope=openid%20profile%20");
            bldr.Append(Uri.EscapeDataString(options.CurrentValue.Scopes));
            bldr.Append("&redirect_uri=");
            bldr.Append(Uri.EscapeDataString(redirect_uri));
            bldr.Append("&client_id=");
            bldr.Append(options.CurrentValue.ClientId);
            bldr.Append("&state=");
            bldr.Append(state);
            bldr.Append("&code_challenge=");
            bldr.Append(code_challenge);
            bldr.Append("&code_challenge_method=S256");

            var startInfo = new ProcessStartInfo
            {
                FileName = bldr.ToString(),
                UseShellExecute = true
            };

            using var http = new HttpListener();
            http.Prefixes.Add(redirect_uri.AppendSlash());
            http.Start();
            Process.Start(startInfo);

            var context = await http.GetContextAsync().ConfigureAwait(false);
            var code = context.Request?.QueryString?.Get("code");
            var error = context.Request?.QueryString?.Get("error");
            var incoming_state = context.Request?.QueryString?.Get("state");
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;
            await context.Response.OutputStream.WriteAsync(
                Encoding.UTF8.GetBytes("""
                    <html>
                        <head>
                            <title>Authentication</title>
                        </head>
                        <body>
                            <h1>Authentication</h1>
                            <p>Authentication succeeded.</p>
                            <p>You can now close this page.</p>
                        </body>
                    </html>
                    """),
                cancellationToken);
            context.Response.Close();

            http.Stop();

            if (!string.IsNullOrWhiteSpace(error))
                return TokenResult.NotValid(error);

            if (string.IsNullOrWhiteSpace(code)
                || string.IsNullOrWhiteSpace(incoming_state)
                || state != incoming_state)
            {
                return TokenResult.NotValid("Invalid response.");
            }

            var grant = string.Format(
                CultureInfo.InvariantCulture,
                "code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&scope=&grant_type=authorization_code",
                    code,
                    Uri.EscapeDataString(redirect_uri),
                    options.CurrentValue.ClientId,
                    code_verifier);
            StringContent content = new(grant, Encoding.UTF8, "application/x-www-form-urlencoded");

            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = options.CurrentValue.IdentityUri.AppendSlash();
            client.Timeout = options.CurrentValue.RequestTimeout;
            if (options.CurrentValue.MaxResponseContentBufferSize.HasValue)
                client.MaxResponseContentBufferSize = options.CurrentValue.MaxResponseContentBufferSize.Value;
            using var resp = await client.PostAsync("connect/token", content, cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                var tokenError = await resp.Content.ReadFromJsonAsync<TokenErrorResponse>(cancellationToken).ConfigureAwait(false);
                return TokenResult.NotValid(tokenError?.ToString() ?? "Unknown error");
            }
            var tokenPayload = await resp.Content.ReadFromJsonAsync<TokenPayload>(cancellationToken: cancellationToken).ConfigureAwait(false);
            return !string.IsNullOrWhiteSpace(tokenPayload?.Token)
                ? await SetValidTokenAsync(tokenPayload.Token, tokenPayload.ExpiresIn).ConfigureAwait(false)
                : TokenResult.NotValid("No valid token was returned.");
        }
        catch (Exception ex)
        {
            if (logger is not null)
                AuthManagerLogging.AuthenticationFailed(logger, ex);
            return TokenResult.NotValid(ex.Message);
        }
    }

    /// <summary>
    /// Generates a random string of the specified length.
    /// </summary>
    /// <param name="length">The length of the random string.</param>
    /// <returns>A random string.</returns>
    private static string GetRandomString(int length)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(length);
        var base64 = Convert.ToBase64String(bytes);
        return base64.Replace('+', '-').Replace('/', '_').Replace("=", "");
    }

    /// <summary>
    /// Generates a code challenge for the specified code verifier.
    /// </summary>
    /// <param name="codeVerifier">The code verifier to generate the code challenge for.</param>
    /// <returns>The generated code challenge.</returns>
    private static string GetCodeChallenge(string codeVerifier)
    {
        var bytes = Encoding.UTF8.GetBytes(codeVerifier);
        var hash = SHA256.HashData(bytes);
        var base64 = Convert.ToBase64String(hash);
        return base64.Replace('+', '-').Replace('/', '_').Replace("=", "");
    }

    public async ValueTask InvalidateToken()
    {
        await _tokenSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            _token = null;
            _tokenValidToUTC = DateTime.MinValue;
        }
        catch { }
        finally { _tokenSemaphore.Release(); }
    }

    public async ValueTask SetToken(string token)
    {
        var wrapper = await GetTokenAsJwtAsync(token).ConfigureAwait(false);
        if (wrapper is null || wrapper.ValidTo <= DateTime.UtcNow.AddMinutes(5))
            return;

        await _tokenSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            _token = token;
            _tokenValidToUTC = wrapper.ValidTo;
        }
        catch { }
        finally { _tokenSemaphore.Release(); }
    }

    public void Dispose() => _tokenSemaphore.Dispose();
}

internal static partial class AuthManagerLogging
{
    [LoggerMessage(1, LogLevel.Error, "Token validation error.")]
    public static partial void TokenValidationError(ILogger logger, Exception exception);

    [LoggerMessage(2, LogLevel.Error, "Authentication failed.")]
    public static partial void AuthenticationFailed(ILogger logger, Exception exception);
}
