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



[<ApiController>]
[<Route("backend")>]
type StudentController (logger : ILogger<StudentController>, traceProvider : IActivityService) =
    inherit ControllerBase()
    
    
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
        Async.RunSynchronously (producer Workflow_QUEUE)
        "sent"

    

        