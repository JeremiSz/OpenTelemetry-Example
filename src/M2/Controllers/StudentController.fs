namespace M2.Controllers

open System
open System.Text
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
open System.Net.Http
open Helpers

[<ApiController>]
[<Route("backend")>]
type StudentController (metricsHelper : MetricsHelper, logger : ILogger<StudentController>, traceProvider : IActivityService) =
    inherit ControllerBase()

    let Propagator : TextMapPropagator = Propagators.DefaultTextMapPropagator;
    let random : Random = new Random();
    let client = new HttpClient();
    
    [<HttpGet>]
    member _.Get() =
        logger.LogInformation("Get request has been made to /backend in M2.", string[])
        logger.LogError(new System.IO.IOException(), "IO Exception thrown.", string[])
        logger.LogWarning("This is a warning.", string[])

        metricsHelper.requestCounter.Add(1);
        
        let httpRequest = new HttpRequestMessage(HttpMethod.Get, "http://localhost:9000");
        try 
            //generate fake errors for the sake of sampling
            if random.Next(0,10) < 5 then
                raise (TimeoutException())
            let httpResponse = client.SendAsync(httpRequest);
            null
        with
        | ex ->  Activity.Current.AddTag("error",true).AddEvent(ActivityEvent(ex.ToString())) |> ignore; null
        |> ignore

        async{
            let collection = getCollection studentCollectionName
            let task = collection.Find<Student>(FilterDefinition.Empty).ToListAsync()
            let! list = Async.AwaitTask(task)
            return list
        }

    [<HttpPost>]
    member _.Post() =
        let publisher() =
            use activity = traceProvider.CreateActivity($"{Workflow_QUEUE} send",ActivityKind.Producer)
            let props = channel.CreateBasicProperties()

            let contextToInject =
                if activity <> null then
                    activity.Context
                elif Activity.Current <> null then
                    Activity.Current.Context
                else
                    Unchecked.defaultof<ActivityContext>

            let InjectTraceContextIntoBasicProperties (props : IBasicProperties) (key : string) (value:string) =
                try
                    if props.Headers = null then 
                        props.Headers <- (new Dictionary<string, obj>())

                    props.Headers[key] <- value
                    printfn "key: %s" key
                    printfn "key: %s" value
                with
                | ex -> logger.LogError(ex, "Failed to inject trace context.") |> ignore

            metricsHelper.latency.Record(random.Next(10), KeyValuePair.Create<string, obj>("hello", "hi"));
        
            activity.SetTag("messaging.system", "rabbitmq") |> ignore
            activity.SetTag("messaging.destination_kind", "queue") |> ignore
            activity.SetTag("messaging.rabbitmq.queue", "sample") |> ignore
  
            let baggage = Baggage.Current;
            let propContext = new PropagationContext(contextToInject,baggage);
            Propagator.Inject(propContext,props,InjectTraceContextIntoBasicProperties)
            
            let rand = Random()
            
            let message = sprintf "%f" (rand.NextDouble())
            let body = Encoding.UTF8.GetBytes(message)
            printfn "publish     : %s" message
            
            (body,props)

        producer publisher
        |> Async.StartAsTask
        |> Async.AwaitTask
        |> ignore
        "sent"

    

    

        