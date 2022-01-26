namespace M2.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

[<ApiController>]
[<Route("backend")>]
type StudentController (logger : ILogger<StudentController>) =
    inherit ControllerBase()

    [<HttpGet>]
    member _.Get() =
        "Joe Doe"
