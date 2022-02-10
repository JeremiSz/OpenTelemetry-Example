using System.Diagnostics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using M1.helpers;
using System.Diagnostics.Metrics;
using M1.Controllers;
using Npgsql;

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
    .AddAspNetCoreInstrumentation()
    .AddNpgsql();
});

builder.Services.AddOpenTelemetryMetrics(b =>
{
    b
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddConsoleExporter()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName).AddTelemetrySdk())
    .AddMeter("M1 Meter")
    .AddOtlpExporter();
});

builder.Logging.AddOpenTelemetry(b =>
{
    b.IncludeFormattedMessage = true;
    b.IncludeScopes = true;
    b.ParseStateValues = true;
    b.AddOtlpExporter();
});

var MyActivitySource = new ActivitySource(serviceName);

builder.Services.AddSingleton<ActivitySource>(MyActivitySource);
builder.Services.AddSingleton<MetricsHelper>(new MetricsHelper(getRequests,getCPU));

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();

Measurement<long> getRequests()
{
    return new Measurement<long>(StudentController.requestCount);
}
Measurement<long> getCPU()
{
    return new Measurement<long>(StudentController.CPU);
}