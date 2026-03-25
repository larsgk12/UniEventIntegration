namespace UniEventIntegration.UnimicroPlatform;

public sealed class ApiException(HttpStatusCode statusCode, string? message, string? errorReference)
    : Exception(string.IsNullOrWhiteSpace(message) ? "No message was received..." : message)
{
    public HttpStatusCode StatusCode => statusCode;

    public string? ErrorReference => errorReference;
}
