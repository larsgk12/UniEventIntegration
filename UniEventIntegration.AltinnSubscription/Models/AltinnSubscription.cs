using System;
using System.Collections.Generic;
using System.Text;

namespace UniEventIntegration.AltinnSubscription.Models;

public class AltinnSubscriptionRequest
{
    public string EndPoint { get; set; } = string.Empty;
    public string ResourceFilter { get; set; } = string.Empty;
    public string SubjectFilter { get; set; } = string.Empty;
    public string TypeFilter { get; set; } = string.Empty;
}

public class AltinnSubscriptionResponse
{
    public bool Success { get; set; }
    public AltinnSubscriptionResult? Subscription { get; set; }
    public string? ErrorMessage { get; set; }
    public System.Net.HttpStatusCode? StatusCode { get; set; }
    public Exception? Exception { get; set; }
}

public class AltinnSubscriptionResult
{
    public int Id { get; set; }
    public string EndPoint { get; set; } = string.Empty;
    public string ResourceFilter { get; set; } = string.Empty;
    public string SubjectFilter { get; set; } = string.Empty;
    public string TypeFilter { get; set; } = string.Empty;
    public string? Consumer { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? Created { get; set; }
    public bool Validated { get; set; }
}
