namespace UniEventIntegration.Models;

public record ApiEntityBase
{
    public int ID { get; init; }
    public int? StatusCode { get; init; }
    public IDictionary<string, object> CustomValues { get; } = new Dictionary<string, object>();
}
