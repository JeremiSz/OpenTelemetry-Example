# Concepts

* Traces - A tree of spans representing the lifetime of a call to a service.
* Span - Starting time of service and full duration. Has a SpanID.
* Tag - Key value Metadata on a specific span. Does endpoint.
* Baggage - Metadata passed from parent to child span, does not reach endpoint.
* Event - A Log tied to a specific span.

# Traces

* Traces can have parent/child relationships with each other.
* Traces are connected in the endpoint when they have the same TraceID.
* A trace consists of:
    * TraceID - Identifies the trace.
    * Span(s)
    * Number of Services
    * Depth

# Terminology

| OpenTelemetry | .NET 6.0 | Java |
| :--- | :--- | :--- |
| Tracer | ActivitySource | Tracer | 
| Span | Activity | Span | 
| Baggage | Baggage | Baggage | 
| Tag | Tag | Attribute |
| Event | Event | Event |

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

# Java-Specific Information

* In Java, you can record information on a throwable by using the recordException() method.

# Useful links
* For filtering traces use [Tail Sampler](https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/processor/tailsamplingprocessor) in collector.
* Examples of Tail Sampling include include probabilistic, string_attribute and numeric_attribute.
* [Conversation](https://github.com/open-telemetry/opentelemetry-collector-contrib/issues/1797) on possible future deprecation of Tail Sampler.
* [Discussion](https://github.com/open-telemetry/opentelemetry-collector/issues/2336) on new proccessor for filtering 