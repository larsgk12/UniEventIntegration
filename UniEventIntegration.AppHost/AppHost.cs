var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.UniEventIntegration>("unieventintegration");

//builder.AddProject<Projects.UniEventIntegration_AltinnSubscription>("unieventintegration-altinnsubscription");

builder.Build().Run();
