using System.Net.Http.Json;

namespace UniEventIntegration.AltinnSubscription
{
    public interface IAltinnSubscriptionService
    {
        Task<AltinnSubscriptionResponse> CreateSubscriptionAsync(
            string endPoint,
            string resourceFilter,
            string subjectFilter,
            string typeFilter,
            CancellationToken cancellationToken = default);
    }

    public class AltinnSubscriptionService : IAltinnSubscriptionService
    {
        private readonly HttpClient _httpClient;
        private const string SubscriptionApiUrl = "https://platform.tt02.altinn.no/events/api/v1/subscriptions";

        public AltinnSubscriptionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AltinnSubscriptionResponse> CreateSubscriptionAsync(
            string endPoint,
            string resourceFilter,
            string subjectFilter,
            string typeFilter,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var subscriptionRequest = new AltinnSubscriptionRequest
                {
                    EndPoint = endPoint,
                    ResourceFilter = resourceFilter,
                    SubjectFilter = subjectFilter,
                    TypeFilter = typeFilter
                };

                var response = await _httpClient.PostAsJsonAsync(
                    SubscriptionApiUrl,
                    subscriptionRequest,
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AltinnSubscriptionResult>(
                        cancellationToken: cancellationToken);

                    return new AltinnSubscriptionResponse
                    {
                        Success = true,
                        Subscription = result,
                        StatusCode = response.StatusCode
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new AltinnSubscriptionResponse
                {
                    Success = false,
                    ErrorMessage = $"Failed to create subscription. Status: {response.StatusCode}, Error: {errorContent}",
                    StatusCode = response.StatusCode
                };
            }
            catch (HttpRequestException ex)
            {
                return new AltinnSubscriptionResponse
                {
                    Success = false,
                    ErrorMessage = $"HTTP request failed: {ex.Message}",
                    Exception = ex
                };
            }
            catch (TaskCanceledException ex)
            {
                return new AltinnSubscriptionResponse
                {
                    Success = false,
                    ErrorMessage = "Request timed out",
                    Exception = ex
                };
            }
            catch (Exception ex)
            {
                return new AltinnSubscriptionResponse
                {
                    Success = false,
                    ErrorMessage = $"An unexpected error occurred: {ex.Message}",
                    Exception = ex
                };
            }
        }
    }

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
}
