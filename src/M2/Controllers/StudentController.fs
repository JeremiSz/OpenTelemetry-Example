namespace M2.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open TraceProvider
open MongoDB.Driver
open MongoDB
open MongoDB.Bson
open MongoDB.Bson.Serialization.Attributes
open Microsoft.FSharp.Control
open System.Threading
open MongoDB.Bson.Serialization
open RabbitMQ.Client
open EasyNetQ
open MongoDB.Driver.Core.Extensions.DiagnosticSources
open MongoDB.Driver.Core.Configuration

type Student = 
    {
        [<BsonId>]
        [<BsonRepresentation(BsonType.ObjectId)>] 
        _id : string
        [<BsonElement("name")>] 
        name:string
    }

[<ApiController>]
[<Route("backend")>]
type StudentController (logger : ILogger<StudentController>, traceProvider : IActivityService) =
    inherit ControllerBase()

    //let bus = RabbitHutch.CreateBus "host=localhost"

    let test (message : string) = 
        Console.WriteLine("Hello, tested.")
    
    //do bus.PubSub.Subscribe<string>("test", test, CancellationToken.None) |> ignore

    //mongo setup
    let connectionString = "mongodb+srv://123:123@mongocult.3pgkf.mongodb.net/myFirstDatabase?retryWrites=true&w=majority"
    let databaseName = "testing"
    let collectionName = "gnitset"
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
        async{
            //let str = "test string"
            //bus.PubSub.Publish(str)
            let collection = database.GetCollection(collectionName)
            let task = collection.Find<Student>(FilterDefinition.Empty).ToListAsync()
            let! list = Async.AwaitTask(task)
            return list
        }
        