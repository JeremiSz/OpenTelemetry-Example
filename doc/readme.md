# Concepts

* Traces - A tree of spans representing the lifetime of a call to a service.
* Span - Starting time of service and full duration. Has a SpanID.
* Tags - Can be used to provide metadata on a specific trace. Appears in final output along with span.
* Baggage - Metadata passed from parent to child, does not get output at the end.

# Traces

* Traces can have parent/child relationships with each other.
* Traces are connected when they have the same TraceID.
* A trace consists of:
    * TraceID - Identifies the trace.
    * Span(s)
    * Services
    * Depth

# Terminology

| OpenTelemetry | .NET 6.0 |
| :--- | :--- |
| Tracer | ActivitySource |
| Span | Activity |

# OpenTelemetry Collector

* The Collector receives traces, metrics and logs from microservices.
* Traces can be received via OpenTelemetry Protocol (OTLP), Jaeger Protocol or Zipkin Protocol.
* Sampling, compressing and batching are processing options.
* Collector can filter and drop unnecessary data.
* Can insert additional metadata to the data.
* Data can be altered through the use of regular expressions.
* Can have multiple pipelines, each with their own unique receivers, processors and exporters.
* Traces can be exported to OTLP, Jaeger, Zipkin, Prometheus, etc or just logged to view locally.
* Contributor version of the Collector supports extra receivers/exporters such as Prometheus, Kafka and Kubernetes.
* Spans can be renamed by using their metadata tags.
