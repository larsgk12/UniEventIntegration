namespace UniEventIntegration.UnimicroPlatformAuth.Models;

internal sealed record TokenErrorResponse(
    [property: JsonPropertyName("error")] string? Error,
    [property: JsonPropertyName("error_description")] string? ErrorDescription)
{
    public override string ToString()
        => string.IsNullOrWhiteSpace(Error)
            ? "Unknown error"
            : $"{Error}: {ErrorDescription}";
};

