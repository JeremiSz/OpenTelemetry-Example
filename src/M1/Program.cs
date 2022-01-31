using M1.Controllers;
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
    .AddOtlpExporter()
    .AddSource(serviceName)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation();
});

var MyActivitySource = new ActivitySource(serviceName);

builder.Services.AddSingleton<ActivitySource>(MyActivitySource);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();