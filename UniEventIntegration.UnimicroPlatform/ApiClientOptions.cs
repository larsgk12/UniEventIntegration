namespace UniEventIntegration.UnimicroPlatform;

public sealed class ApiClientOptions
{
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public long? MaxResponseContentBufferSize { get; set; }
    public string? UserAgentProductName { get; set; }
    public string? UserAgentProductVersion { get; set; }
}
