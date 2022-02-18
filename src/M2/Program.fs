namespace M2

#nowarn "20"
open System
open System.Threading
open System.Diagnostics.Metrics
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open OpenTelemetry.Trace
open OpenTelemetry.Resources
open OpenTelemetry.Metrics
open TraceProvider
open Helpers
open Helpers.RabbitHelper
open OpenTelemetry.Context.Propagation
open System.Diagnostics
open M2.Controllers
open OpenTelemetry.Logs
open Microsoft.Extensions.Logging


module Program =
    let exitCode = 0
    let serviceName = "CCS.OpenTelemetry.M2";
    let serviceVersion = "1.0.0";
    let Propagator : TextMapPropagator = new TraceContextPropagator()
    let ActivitySource = new ActivitySource(serviceName)

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)
        builder.Services.AddOpenTelemetryTracing(fun b ->
            b
                .AddOtlpExporter()
                .AddSource(serviceName)
                .AddSource(nameof(StudentController))
                .AddSource(nameof(RabbitHelper))
                .AddSource(nameof(MongoHelper))
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName,serviceVersion=serviceVersion))
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddMongoDBInstrumentation()
            |> ignore
        )

        builder.Services.AddOpenTelemetryMetrics(fun b ->
            b
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName).AddTelemetrySdk())
                .AddMeter("CCS.OpenTelemetry.M2")
                .AddOtlpExporter()
                .AddConsoleExporter()
            |> ignore
        )

        builder.Logging.AddOpenTelemetry(fun b ->
            b.IncludeFormattedMessage <- true
            b.IncludeScopes <- true
            b.ParseStateValues <- true
            b.AddOtlpExporter() |> ignore
        )

        builder.Services.AddSingleton<MetricsHelper, MetricsHelper>()
        builder.Services.AddTransient<IActivityService, ActivityService>()
        builder.Services.AddControllers()
        
        let app = builder.Build()

        app.UseHttpsRedirection()

        app.UseAuthorization()

        app.MapControllers()

        let token = new CancellationTokenSource()   
              
        //start consumers
        Async.Start(consumer token);

        app.Run()

        token.Cancel()

        exitCode
