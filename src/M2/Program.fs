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
open Helpers.RabbitHelper
open Helpers.MongoHelper
open MongoDB.Bson

module Program =
    let exitCode = 0
    let serviceName = "CCS.OpenTelemetry.M2";
    let serviceVersion = "1.0.0";

    let dbhandler sender (data:BasicDeliverEventArgs) =
        let body = data.Body.ToArray()
        let message = Encoding.UTF8.GetString(body)
        
        new BsonElement("name",message)
        |> (new BsonDocument()).Add
        |> (getCollection workflowCollectionName).InsertOne
    
    

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

        
        app.MapControllers()


        let token = new CancellationTokenSource()   
              
        //start consumers
        Async.Start(consumer Workflow_QUEUE dbhandler token);

        app.Run()

        token.Cancel()

        exitCode
