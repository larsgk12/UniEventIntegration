namespace UniEventIntegration.Helpers;

internal static class CertificateHelper
{
    /// <summary>
    /// Retrieves a certificate asynchronously based on the specified thumbprint.
    /// </summary>
    /// <param name="thumbprint">The thumbprint of the certificate.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation that returns an <see cref="X509Certificate2"/> object, or <c>null</c> if the certificate is not found.</returns>
    public static async Task<X509Certificate2?> GetCertificateAsync(string thumbprint)
    {
        using X509Store store = new(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadOnly);

        if (store is not null)
        {
            // Prio 1: Use Windows certificate store:
            if (store.Certificates is null || store.Certificates.Count == 0)
                return default;
            var col = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            return col is null || col.Count == 0
                ? default
                : col[0];
        }

        // Prio 2: Load from linux-filesystem
        var linuxFileCert = await LoadLinuxCertificateAsync(thumbprint).ConfigureAwait(false);
        if (linuxFileCert is not null)
            return linuxFileCert;

        // Prio 3: Read from connectionstrings (key = {thumbprint}, pwd = {thumprint}_pwd)
        return GetFromBase64ConnectionString(thumbprint);
    }

    /// <summary>
    /// Loads a certificate asynchronously from the Linux file system based on the specified thumbprint.
    /// </summary>
    /// <param name="thumbprint">The thumbprint of the certificate.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation that returns an <see cref="X509Certificate2"/> object, or <c>null</c> if the certificate is not found.</returns>
    private static async Task<X509Certificate2?> LoadLinuxCertificateAsync(string thumbprint)
    {
        // https://learn.microsoft.com/en-us/azure/app-service/configure-ssl-certificate-in-code
        var fn = $"/var/ssl/certs/{thumbprint}.der";
        if (!File.Exists(fn))
            fn = $"/var/ssl/private/{thumbprint}.pfx";
        if (!File.Exists(fn))
            fn = $"/var/ssl/private/{thumbprint}.p12";
        if (!File.Exists(fn))
            return default;
        var bytes = await File.ReadAllBytesAsync(fn).ConfigureAwait(false);
        return X509CertificateLoader.LoadPkcs12(bytes, null, X509KeyStorageFlags.EphemeralKeySet);
    }

    /// <summary>
    /// Retrieves a certificate from a base64 connection string based on the specified name.
    /// </summary>
    /// <param name="name">The name of the certificate.</param>
    /// <returns>An <see cref="X509Certificate2"/> object, or <c>null</c> if the certificate is not found.</returns>
    private static X509Certificate2? GetFromBase64ConnectionString(ReadOnlySpan<char> name)
    {
        var value = Environment.GetEnvironmentVariable($"CUSTOMCONNSTR_{name}");
        if (string.IsNullOrEmpty(value)) return default;
        var pwd = Environment.GetEnvironmentVariable($"CUSTOMCONNSTR_{name}_pwd");
        if (string.IsNullOrEmpty(pwd)) return default;
        return X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(value), pwd);
    }
}
