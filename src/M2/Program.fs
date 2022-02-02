namespace M2

#nowarn "20"
open System
open System.Threading
open System.Text
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open OpenTelemetry.Trace
open OpenTelemetry.Resources
open TraceProvider
open RabbitMQ.Client
open RabbitMQ.Client.Events
open Rabbit.Hello

module Program =
    let exitCode = 0
    let serviceName = "CCS.OpenTelemetry.M2";
    let serviceVersion = "1.0.0";

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)
        builder.Services.AddOpenTelemetryTracing(fun builder ->
            builder
                .AddOtlpExporter()
                .AddSource(serviceName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName,serviceVersion=serviceVersion))
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddMongoDBInstrumentation()
            |> ignore
        )
        builder.Services.AddTransient<IActivityService, ActivityService>()
        builder.Services.AddControllers()
        
        let app = builder.Build()

        app.UseHttpsRedirection()

        app.UseAuthorization()

        let host = "localhost"
        let queue = "data_stream"
        let routingKey = "data_stream"

        (*async {
            let token = new CancellationTokenSource()
            token.CancelAfter 5000
    
            seq { 
              (producer host queue routingKey token); 
              (consumer "1" host queue token); 
              (consumer "2" host queue token) } 
            |> Async.Parallel
            |> Async.RunSynchronously |> ignore
          } |> Async.RunSynchronously *)

        app.MapControllers()

        app.Run()

        exitCode
