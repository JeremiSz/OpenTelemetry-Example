using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

var serviceName = "CCS.OpenTelemetry.M1";
var serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetryTracing(b =>
{
    b
    .AddJaegerExporter()
    .AddSource(serviceName)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation();
});

var MyActivitySource = new ActivitySource(serviceName);


var app = builder.Build();

app.MapGet("/api/v1/student", () =>
{
    using var activity = MyActivitySource.StartActivity("M1"); 
    activity?.SetTag("Get", "Get request made to M1.");
    //HttpClient client = new HttpClient();
    //HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7225/backend");
    //HttpResponseMessage httpResponse = client.Send(httpRequest);
    return "Get Students";
})
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/v1/student", () =>
{
    using var activity = MyActivitySource.StartActivity("M1"); 
    activity?.SetTag("Post", "Post request made to M1");
})
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest);

app.Run();