package main

import (
	"context"
	"fmt"
	"log"
	"net/http"

	"go.opentelemetry.io/contrib/instrumentation/net/http/otelhttp"
	"go.opentelemetry.io/otel"
	"go.opentelemetry.io/otel/baggage"
	"go.opentelemetry.io/otel/sdk/resource"
	"go.opentelemetry.io/otel/trace"

	//"go.opentelemetry.io/otel/trace"
	"go.opentelemetry.io/otel/exporters/otlp/otlptrace"
	"go.opentelemetry.io/otel/exporters/otlp/otlptrace/otlptracegrpc"
	"go.opentelemetry.io/otel/propagation"
	sdktrace "go.opentelemetry.io/otel/sdk/trace"

	"go.opentelemetry.io/otel/attribute"
	semconv "go.opentelemetry.io/otel/semconv/v1.7.0"
)

//var tracer = otel.Tracer("tracer")

func httpHandler(w http.ResponseWriter, r *http.Request) {
	fmt.Fprintf(w, "Hello, World! I am instrumented autoamtically!")
	//ctx := r.Context()
	span := trace.SpanFromContext(r.Context())
	defer span.End()
	ctx := context.Background()
	ctx = trace.ContextWithSpan(ctx, span)

	fmt.Println("handler: ", span.SpanContext().SpanID())

	sleepy(ctx)
}

func sleepy(ctx context.Context) {
	client := http.Client{Transport: otelhttp.NewTransport(http.DefaultTransport)}
	tracer := otel.Tracer("tracer")
	//spanCtx := otel.GetTextMapPropagator().Extract(ctx, http)
	ctx, span := tracer.Start(ctx, "sleep")
	//ctx = trace.ContextWithSpan(ctx, span)
	defer span.End()

	fmt.Println(span.SpanContext().SpanID())

	//req, _ := http.NewRequestWithContext(ctx, "GET", "https://localhost:7225/backend", nil)
	req, _ := http.NewRequest("GET", "https://localhost:7225/backend", nil)
	//val := fmt.Sprintf("%.2x-%s-%s-%s", 0, span.SpanContext().TraceID(), span.SpanContext().SpanID(), span.SpanContext().TraceFlags())
	//req.Header.Set("traceparent", val)

	sc := trace.SpanContextFromContext(ctx)
	if !sc.IsValid() {
		return
	}
	if ts := sc.TraceState().String(); ts != "" {
		req.Header.Set("tracestate", ts)
	}
	// Clear all flags other than the trace-context supported sampling bit.
	flags := sc.TraceFlags() & trace.FlagsSampled
	h := fmt.Sprintf("%.2x-%s-%s-%s",
		0,
		sc.TraceID(),
		sc.SpanID(),
		flags)
	req.Header.Set("traceparent", h)

	//tmp := otel.GetTextMapPropagator()

	//span.SetAttributes(attribute.String("sleep.duration", "i am not sleeping"))

	go client.Do(req)
}

func newResource() *resource.Resource {
	r, _ := resource.Merge(
		resource.Default(),
		resource.NewWithAttributes(
			semconv.SchemaURL,
			semconv.ServiceNameKey.String("m1"),
			semconv.ServiceVersionKey.String("v0.1.0"),
			attribute.String("environment", "demo"),
		),
	)
	return r
}

func newExporter(ctx context.Context) (*otlptrace.Exporter, error) {
	opts := []otlptracegrpc.Option{
		otlptracegrpc.WithInsecure(),
	}

	client := otlptracegrpc.NewClient(opts...)
	return otlptrace.New(ctx, client)
}

func main() {
	// Wrap your httpHandler function.
	handler := http.HandlerFunc(httpHandler)
	wrappedHandler := otelhttp.NewHandler(handler, "m1")
	http.Handle("/", wrappedHandler)

	ctx := baggage.ContextWithoutBaggage(context.Background())

	exp, _ := newExporter(ctx)

	tp := sdktrace.NewTracerProvider(
		sdktrace.WithBatcher(exp),
		sdktrace.WithResource(newResource()),
	)

	otel.SetTracerProvider(tp)

	otel.SetTextMapPropagator(
		propagation.NewCompositeTextMapPropagator(
			propagation.TraceContext{},
			propagation.Baggage{},
		),
	)

	// And start the HTTP serve.
	log.Fatal(http.ListenAndServe(":10000", nil))
}
