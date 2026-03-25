namespace UniEventIntegration.Options;

/// <summary>
/// Represents the options for the authentication manager.
/// </summary>
public sealed class AuthManagerOptions
{
    /// <summary>
    /// Gets or sets the identity URI.
    /// </summary>
    public Uri IdentityUri { get; set; } = default!;

    /// <summary>
    /// Gets or sets the client ID.
    /// </summary>
    public string ClientId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the scopes.
    /// </summary>
    public string Scopes { get; set; } = "AppFramework";

    /// <summary>
    /// Gets or sets the thumbprint.
    /// </summary>
    public string? Thumbprint { get; set; }

    /// <summary>
    /// Gets or sets the certificate.
    /// </summary>
    public string? Certificate { get; set; }

    /// <summary>
    /// Gets or sets the certificate path.
    /// </summary>
    public string? CertificatePath { get; set; }

    /// <summary>
    /// Gets or sets the certificate password.
    /// </summary>
    public string? CertificatePwd { get; set; }

    /// <summary>
    /// Gets or sets the request timeout.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the maximum response content buffer size.
    /// </summary>
    public long? MaxResponseContentBufferSize { get; set; }

    /// <summary>
    /// Gets or sets the redirect URI.
    /// </summary>
    public string RedirectUri { get; set; } = "http://localhost:5556";

    /// <summary>
    /// Initializes the authentication manager options with the specified options.
    /// </summary>
    /// <param name="options">The options to initialize from.</param>
    public void Init(AuthManagerOptions options)
    {
        IdentityUri = options.IdentityUri;
        ClientId = options.ClientId;
        ClientSecret = options.ClientSecret;
        UserName = options.UserName;
        Password = options.Password;
        Scopes = options.Scopes;
        Thumbprint = options.Thumbprint;
        Certificate = options.Certificate;
        CertificatePath = options.CertificatePath;
        CertificatePwd = options.CertificatePwd;
        RequestTimeout = options.RequestTimeout;
        MaxResponseContentBufferSize = options.MaxResponseContentBufferSize;
        RedirectUri = options.RedirectUri;
    }
}
