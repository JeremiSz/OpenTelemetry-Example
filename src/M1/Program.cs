using System.Diagnostics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;

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

builder.Services.AddOpenTelemetryMetrics(b =>
{
    b
    .AddHttpClientInstrumentation()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName).AddTelemetrySdk())
    .AddMeter("M1 Meter")
    .AddOtlpExporter();
});

var MyActivitySource = new ActivitySource(serviceName);

builder.Services.AddSingleton<ActivitySource>(MyActivitySource);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();