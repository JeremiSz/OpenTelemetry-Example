namespace TraceProvider
open System.Diagnostics
open MongoDB.Driver
open MongoDB
open MongoDB.Bson

type IActivityService =
    abstract member CreateActivity: string -> Activity
type ActivityService() =
    let serviceName = "CCS.OpenTelemetry.M2";
    let activityFactory = new ActivitySource(serviceName);
    interface IActivityService with
        member _.CreateActivity(name : string) = 
            activityFactory.StartActivity(name)