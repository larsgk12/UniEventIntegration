namespace UniEventIntegration.AltinnSubscription.Models;


public class CloudEvent
{
    public string Id { get; set; } = default!;
    public string Source { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string SpecVersion { get; set; } = default!;
    public string? Subject { get; set; }
    public object? Data { get; set; }
}

public class AltinnCloudEvent : CloudEvent
{
    public string? Resource { get; set; }
}
