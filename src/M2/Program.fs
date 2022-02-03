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
open RabbitMQ.Client.Events
open Helpers.RabbitHelper
open Helpers.MongoHelper
open MongoDB.Bson
open OpenTelemetry.Context.Propagation
open RabbitMQ.Client
open System.Collections
open System.Linq
open System.Collections.Generic
open OpenTelemetry
open System.Diagnostics



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
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName,serviceVersion=serviceVersion))
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddMongoDBInstrumentation()
         
            |> ignore
        )

        let ExtractTraceContextFromBasicProperties (props : IBasicProperties) (key : string) : IEnumerable<string> =
            let output = Enumerable.Empty<string>()
            try
                let isValid,result = props.Headers.TryGetValue key
                if isValid then
                    result.ToString()
                    |> output.Append
                    |> ignore
            
            with 
            | ex -> Console.WriteLine($"Failed to extract trace context: {ex}")

            output

        let dbhandler sender (data:BasicDeliverEventArgs) =
            let propagrationContext = Propagator.Extract(Unchecked.defaultof<PropagationContext>,data.BasicProperties, ExtractTraceContextFromBasicProperties)
            Baggage.Current <- propagrationContext.Baggage

            let activity = ActivitySource.StartActivity("inserting now",ActivityKind.Consumer,propagrationContext.ActivityContext)
            let body = data.Body.ToArray()
            let message = Encoding.UTF8.GetString(body)
            
            new BsonElement("name",message)
            |> (new BsonDocument()).Add
            |> (getCollection workflowCollectionName).InsertOne

        
        

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
