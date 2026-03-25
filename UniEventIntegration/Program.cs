using UniEventIntegration.AltinnSubscription;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register token provider
builder.Services.AddSingleton<IAltinnTokenProvider, AltinnTokenProvider>();
builder.Services.AddTransient<AltinnBearerTokenHandler>();

// Register HttpClient with the handler
builder.Services.AddHttpClient<IAltinnSubscriptionService, AltinnSubscriptionService>()
    .AddHttpMessageHandler<AltinnBearerTokenHandler>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
