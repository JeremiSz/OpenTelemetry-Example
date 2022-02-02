namespace M2.Controllers

open System
open System.Text
open System.Threading
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open TraceProvider
open MongoDB.Driver
open MongoDB.Bson
open MongoDB.Bson.Serialization.Attributes
open RabbitMQ.Client
open RabbitMQ.Client.Events
open MongoDB.Driver.Core.Extensions.DiagnosticSources
open MongoDB.Driver.Core.Configuration

type Student = 
    {
        [<BsonId>]
        [<BsonRepresentation(BsonType.ObjectId)>] 
        _id : string
        [<BsonElement("name")>] 
        name : string
    }

[<ApiController>]
[<Route("backend")>]
type StudentController (logger : ILogger<StudentController>, traceProvider : IActivityService) =
    inherit ControllerBase()

    (* let testFunction (data :BasicDeliverEventArgs) = 
        let body = data.Body.ToArray();
        let message = Encoding.UTF8.GetString(body);
        Console.WriteLine(" [x] Received {0}", message); *)

    //rabbitMQ setup
   (* let factory = new ConnectionFactory()
    do factory.HostName <- "localhost"
    //do factory.UserName <- "producer"
    let queueName = "testing"
    let routingKey = "RabbitMQ"

    let connection = factory.CreateConnection()
    let channel = connection.CreateModel()
    do channel.QueueDeclare(queueName,false,false,false,null) |> ignore
    let consumer = new EventingBasicConsumer(channel)

    do consumer.Received.AddHandler(new EventHandler<BasicDeliverEventArgs>(fun sender (data:BasicDeliverEventArgs) -> 
        let body = data.Body.ToArray()
        let message = Encoding.UTF8.GetString(body)
        printfn "%s %s" "consumed" message));

    let consumeTesult = channel.BasicConsume(queueName,true,consumer) |> ignore; *)

    //mongo setup
    let connectionString = "mongodb://localhost:27017"
    let databaseName = "studentDB"
    let collectionName = "students"
    let setting = MongoClientSettings.FromConnectionString(connectionString)
    let makeClusterBuilder(clusterbuilder:ClusterBuilder)= 
        let DAES = new DiagnosticsActivityEventSubscriber()
        clusterbuilder.Subscribe DAES |> ignore
    
    let actionCB = Action<ClusterBuilder> makeClusterBuilder

    do setting.ClusterConfigurator <- actionCB
    let client = new MongoClient(setting)
    let database = client.GetDatabase(databaseName)
    
    [<HttpGet>]
    member _.Get() =
       (* let connection = factory.CreateConnection()
        let channel = connection.CreateModel()
        channel.QueueDeclare(queueName,false,false,false,null) |> ignore
        let message = "we made a queue. Well done us"
        let body = Encoding.UTF8.GetBytes(message)
        channel.BasicPublish("",routingKey,null,body) *)
        async{
            let collection = database.GetCollection(collectionName)
            let task = collection.Find<Student>(FilterDefinition.Empty).ToListAsync()
            let! list = Async.AwaitTask(task)
            return list
        }
        //"Published"

    

        