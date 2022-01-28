namespace M2.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open TraceProvider

[<ApiController>]
[<Route("backend")>]
type StudentController (logger : ILogger<StudentController>, traceProvider : IActivityService) =
    inherit ControllerBase()
    

    [<HttpGet>]
    member _.Get() =
        let activity = traceProvider.CreateActivity();
        activity.Stop()
        "Joe Doe"
