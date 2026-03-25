namespace UniEventIntegration.Models;

public record Endpoints(
    Uri AppFramework,
    Uri Identity,
    Uri Job,
    Uri Files,
    Uri License,
    string Signalr,
    Uri Integration,
    Uri FrontEnd
);
