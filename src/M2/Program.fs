namespace M2

#nowarn "20"
open System
open System.Threading
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
        builder.Services.AddOpenTelemetryTracing(fun builder ->
            builder
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

        builder.Logging.AddOpenTelemetry(fun b ->
            b.IncludeFormattedMessage <- true
            b.IncludeScopes <- true
            b.ParseStateValues <- true
            b.AddOtlpExporter() |> ignore
        )

        builder.Services.AddTransient<IActivityService, ActivityService>()
        builder.Services.AddControllers()
        
        let app = builder.Build()

        app.UseHttpsRedirection()

        app.UseAuthorization()

        app.MapControllers()

        let token = new CancellationTokenSource()   
              
        //start consumers
        Async.Start(consumer (getChannel()) Workflow_QUEUE dbhandler token);

        app.Run()

        token.Cancel()

        exitCode
