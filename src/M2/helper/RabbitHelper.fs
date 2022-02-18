namespace Helpers

open System
open System.Threading
open System.Diagnostics;
open System.Linq;
open System.Text;
open RabbitMQ.Client
open RabbitMQ.Client.Events
open OpenTelemetry.Context.Propagation
open OpenTelemetry
open MongoDB.Bson
open System.Collections.Generic
open MongoHelper


module RabbitHelper =
    let private HOST = "localhost"
    let private PORT = 5672;
    let Student_QUEUE = "data_stream"
    let Workflow_QUEUE = "students"
    let serviceName = "CCS.OpenTelemetry.M2";
    let Propagator : TextMapPropagator = new TraceContextPropagator()
    let ActivitySource = new ActivitySource(serviceName)
    let private connection = 
        let factory = ConnectionFactory()
        factory.HostName <- HOST
        factory.Port <- PORT
        factory.CreateConnection()
    let channel = connection.CreateModel();
    do channel.QueueDeclare(Workflow_QUEUE, false, false, false, null) |> ignore

    let ExtractTraceContextFromBasicProperties (props : IBasicProperties) (key : string) : IEnumerable<string> =
        let mutable output = Enumerable.Empty<string>()
        
        try
            let isValid,result = props.Headers.TryGetValue key
            if isValid then
                let array = [| Encoding.UTF8.GetString(result :?> byte[]) |]
                output <- array
        with 
        | ex -> Console.WriteLine($"Failed to extract trace context: {ex}")

        output

    let dbhandler sender (data:BasicDeliverEventArgs) =
        async{
            let propagrationContext = Propagator.Extract(Unchecked.defaultof<PropagationContext>, data.BasicProperties, ExtractTraceContextFromBasicProperties)
            Baggage.Current <- propagrationContext.Baggage
        
            use activity = ActivitySource.StartActivity(serviceName,ActivityKind.Consumer,propagrationContext.ActivityContext)
            activity.SetTag("messaging.system", "rabbitmq") |> ignore
            activity.SetTag("messaging.destination_kind", "queue") |> ignore
            activity.SetTag("messaging.rabbitmq.queue", "sample") |> ignore
        
            let body = data.Body.ToArray()
            let message = Encoding.UTF8.GetString(body)

            new BsonElement("name",message)
            |> (new BsonDocument()).Add
            |> (getCollection workflowCollectionName).InsertOne
        }
        |>ignore

    let consumerObject = EventingBasicConsumer(channel)
    consumerObject.Received.AddHandler(new EventHandler<BasicDeliverEventArgs>(dbhandler))

    let producer (publisher: unit -> byte[] * IBasicProperties)  = async {

        let body, basicProperties = publisher()
        channel.BasicPublish("", Workflow_QUEUE, basicProperties,body )
    }

    let consumer (token: CancellationTokenSource) = async {

        do channel.BasicConsume(Workflow_QUEUE, true, consumerObject) |> ignore

        while not token.IsCancellationRequested do
            Thread.Sleep(500)
    }

