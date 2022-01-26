using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

var serviceName = "CCS.OpenTelemetry.M1";
var serviceVersion = "1.0.0";

// https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Jaeger/README.md

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(serviceName)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .AddJaegerExporter()
    .Build();

var MyActivitySource = new ActivitySource(serviceName);
using var activity = MyActivitySource.StartActivity("M1");

// Can set universal tags that get sent with all requests made to the API
// activity?.SetTag("universal?", "this is universal");

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/v1/student", () =>
{
    activity?.SetTag("Get", "Get request made to M1.");
    HttpClient client = new HttpClient();
    HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7225/backend");
    HttpResponseMessage httpResponse = client.Send(httpRequest);
    return httpResponse.ToString();
});
//.Produces(StatusCodes.Status200OK)
//.Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/v1/student", () =>
{
    activity?.SetTag("Post", "Post request made to M1");
});
    //.Produces(StatusCodes.Status200OK)
    //.Produces(StatusCodes.Status400BadRequest);

app.Run();