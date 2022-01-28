namespace TraceProvider
open System.Diagnostics

type IActivityService =
    abstract member CreateActivity: unit -> Activity
type ActivityService() =
    let serviceName = "CCS.OpenTelemetry.M2";
    let activityFactory = new ActivitySource(serviceName);
    interface IActivityService with
        member _.CreateActivity() = 
            activityFactory.StartActivity()