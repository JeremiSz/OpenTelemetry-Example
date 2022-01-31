namespace M2
#nowarn "20"
open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open OpenTelemetry
open OpenTelemetry.Trace
open OpenTelemetry.Resources
open TraceProvider

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
            |> ignore
        )
        builder.Services.AddTransient<IActivityService, ActivityService>()
        builder.Services.AddControllers()
        

        let app = builder.Build()

        app.UseHttpsRedirection()

        app.UseAuthorization()
        app.MapControllers()

        app.Run()

        exitCode
