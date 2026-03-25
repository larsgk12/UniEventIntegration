namespace UniEventIntegration.UnimicroPlatform.Payroll
{
    public class PayrollService : IPayrollService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PayrollService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendDialogportenUpdate(string source, CancellationToken cancellationToken = default)
        {
            var endpoint = _configuration["Unimicro:PayrollEndpoint"]
                ?? throw new InvalidOperationException("Unimicro:PayrollEndpoint configuration is missing");

            var response = await _httpClient.PostAsJsonAsync(endpoint, new { source }, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
