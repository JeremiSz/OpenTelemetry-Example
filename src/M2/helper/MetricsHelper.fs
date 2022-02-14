namespace Helpers

open System.Diagnostics.Metrics

type MetricsHelper()=
    member _.meter = new Meter("CCS.OpenTelemetry.M2")
    member this.requestCounter = this.meter.CreateCounter<int64>("m2_request_counter", "total")
    member this.latency = this.meter.CreateHistogram<double>("m2_random_histogram")