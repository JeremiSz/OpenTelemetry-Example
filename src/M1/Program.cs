using System.Diagnostics;

using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

// Define some important constants and the activity source
var serviceName = "CCS.OpenTelemetry.M1";
var serviceVersion = "1.0.0";

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(serviceName)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .AddJaegerExporter()
    .Build();

var MyActivitySource = new ActivitySource(serviceName);

using var activity = MyActivitySource.StartActivity("SayHello");
activity?.SetTag("universal?", "this is universal");



// https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Jaeger/README.md



var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => {
    activity?.SetTag("foo", 1);
    activity?.SetTag("req", "This was a get");
    activity?.SetTag("baz", new int[] { 1, 2, 3 });
});

app.MapPost("/", () => {
    activity?.SetTag("foo", 1);
    activity?.SetTag("req", "This was a post");
    activity?.SetTag("baz", new int[] { 1, 2, 3 });
});

app.Run();