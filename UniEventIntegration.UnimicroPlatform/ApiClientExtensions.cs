namespace UniEventIntegration.UnimicroPlatform;

public static class ApiClientExtensions
{
    public static IServiceCollection AddApiClient(
        this IServiceCollection services,
        string sectionName = "ApiClient")
    {
        services
            .AddOptions<ApiClientOptions>()
            .BindConfiguration(sectionName)
            .ValidateOnStart();
        services.AddHttpClient<ApiClient>(static (sp, client) => ConfigureClient(sp, client));
        return services;
    }

    public static IServiceCollection AddApiClient(
        this IServiceCollection services,
        ApiClientOptions clientOptions)
    {
        services
            .AddOptions<ApiClientOptions>()
            .Configure(options =>
            {
                options.RequestTimeout = clientOptions.RequestTimeout;
                options.MaxResponseContentBufferSize = clientOptions.MaxResponseContentBufferSize;
                options.UserAgentProductName = clientOptions.UserAgentProductName;
                options.UserAgentProductVersion = clientOptions.UserAgentProductVersion;
            })
            .ValidateOnStart();
        services.AddHttpClient<ApiClient>(static (sp, client) => ConfigureClient(sp, client));
        return services;
    }

    public static IServiceCollection AddApiClient(
        this IServiceCollection services,
        Action<ApiClientOptions> options)
    {
        services.PostConfigure(options);
        services.AddHttpClient<ApiClient>(static (sp, client) => ConfigureClient(sp, client));
        return services;
    }

    public static IServiceCollection AddApiClient(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
    {
        services.Configure<ApiClientOptions>(configurationSection);
        services.AddHttpClient<ApiClient>(static (sp, client) => ConfigureClient(sp, client));
        return services;
    }

    private static void ConfigureClient(IServiceProvider sp, HttpClient client)
    {
        var options = sp
            .GetRequiredService<IOptionsMonitor<ApiClientOptions>>()
            .CurrentValue;
        client.Timeout = options.RequestTimeout;
        if (options.MaxResponseContentBufferSize.HasValue)
            client.MaxResponseContentBufferSize = options.MaxResponseContentBufferSize.Value;
        client.DefaultRequestHeaders.UserAgent.Clear();
        if (!string.IsNullOrWhiteSpace(options.UserAgentProductName))
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(options.UserAgentProductName, options.UserAgentProductVersion));
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.AcceptCharset.Clear();
        client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
    }
}
