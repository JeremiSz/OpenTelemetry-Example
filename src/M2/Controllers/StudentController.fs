namespace M2.Controllers

open System
open System.Text
open System.Threading
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open TraceProvider
open Helpers.RabbitHelper
open Helpers.MongoHelper
open MongoDB.Driver
open OpenTelemetry.Context.Propagation
open System.Diagnostics
open OpenTelemetry
open RabbitMQ.Client
open System.Collections.Generic
open System.Collections


[<ApiController>]
[<Route("backend")>]
type StudentController (logger : ILogger<StudentController>, traceProvider : IActivityService) =
    inherit ControllerBase()

    let Propagator : TextMapPropagator = Propagators.DefaultTextMapPropagator;
    
    
    [<HttpGet>]
    member _.Get() =
        async{
            let collection = getCollection studentCollectionName
            let task = collection.Find<Student>(FilterDefinition.Empty).ToListAsync()
            let! list = Async.AwaitTask(task)
            return list
        }

    [<HttpPost>]
    member _.Post() =
        let publisher() =
            let activity = traceProvider.CreateActivity ($"{Workflow_QUEUE} send",ActivityKind.Producer)

            let channel = getChannel()
            let props = channel.CreateBasicProperties()

            let contextToInject =
                if activity <> null then
                    Console.WriteLine "has activity context"
                    activity.Context
                elif Activity.Current <> null then
                    Console.Write "has context in current"
                    Activity.Current.Context
                else
                    Unchecked.defaultof<ActivityContext>

            let InjectTraceContextIntoBasicProperties (props : IBasicProperties) (key : string) (value:string) =
                try
                    if props.Headers = null then 
                        props.Headers <- (new Dictionary<string, obj>())

                    props.Headers[key] = value |> ignore
                with
                | ex -> logger.LogError(ex, "Failed to inject trace context.") |> ignore
  
            let baggage = Baggage.Current;
            let propContext = new PropagationContext(contextToInject,baggage);
            Propagator.Inject(propContext,props,InjectTraceContextIntoBasicProperties)
            Prop
            
            let rand = Random()
            
            let message = sprintf "%f" (rand.NextDouble())
            let body = Encoding.UTF8.GetBytes(message)
            printfn "publish     : %s" message
            body
        Async.RunSynchronously (producer (getChannel()) publisher Workflow_QUEUE)
        "sent"

    

        