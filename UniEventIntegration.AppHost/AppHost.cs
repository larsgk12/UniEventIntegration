var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.UniEventIntegration>("unieventintegration");

builder.Build().Run();
