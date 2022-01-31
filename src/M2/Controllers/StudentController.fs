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
open BagnoDB.Connection
open BagnoDB.Query
open BagnoDB.Filter
open BagnoDB.Serialization
open BagnoDB.Conventions
open BagnoDB
open Microsoft.FSharp.Control
open System.Threading
open MongoDB.Bson.Serialization
open RabbitMQ.Client
open EasyNetQ

type Student = { 
    _id: BsonObjectId
    name: string
}

[<ApiController>]
[<Route("backend")>]
type StudentController (logger : ILogger<StudentController>, traceProvider : IActivityService) =
    inherit ControllerBase()

    let collection = "students"
    let database = "studentsDB"

    let config : Config = {
        host = "localhost"
        port = 27017
        user = Some ""
        password = Some ""
        authDb = Some ""
    }
      
    let configuration =
        Connection.host config
        |> Connection.database database
        |> Connection.collection collection

    //let bus = RabbitHutch.CreateBus "host=localhost"

    let test (message : string) = 
        Console.WriteLine("Hello, tested.")
    
    //do bus.PubSub.Subscribe<string>("test", test, CancellationToken.None) |> ignore

    [<HttpGet>]
    member _.Get() =
        //let str = "test string"
        //bus.PubSub.Publish(str)
        "Hit M2 API"
        (* let filter = Filter.empty
        let options = FindOptions<Student>()
        Console.Write("i am about to search")
        async {
            Console.Write("i am searching")
            let! result = 
                Connection.host config
                |> Connection.database database
                |> Connection.collection collection
                |> Query.filter CancellationToken.None options filter
            
            return result
        } |> Async.StartAsTask *)
    (* member _.Get() =
        let connection = Connection.create config database collection
        //let filter = Filter.empty
        let options = FindOptions<Student>()
        Console.Write("i am about to search")
        Console.Write("i am searching")
        getAll CancellationToken.None options connection
        |> Async.RunSynchronously *)
    (* member _.Get() = 
        Serialization.bson (BagnoSerializationProvider ())
        Conventions.create
        |> Conventions.add (OptionConvention ())
        |> Conventions.add (RecordConvention ())
        |> Conventions.build "F# Type Conventions"

        let filter = Filter.empty
        let options = FindOptions<Student>() *)