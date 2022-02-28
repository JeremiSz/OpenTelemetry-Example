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

	//"go.opentelemetry.io/otel/trace"
	"go.opentelemetry.io/otel/exporters/otlp/otlptrace"
	"go.opentelemetry.io/otel/exporters/otlp/otlptrace/otlptracegrpc"
	"go.opentelemetry.io/otel/propagation"
	sdktrace "go.opentelemetry.io/otel/sdk/trace"

	"go.opentelemetry.io/otel/attribute"
	semconv "go.opentelemetry.io/otel/semconv/v1.7.0"
)

var tracer = otel.Tracer("tracer")

func httpHandler(w http.ResponseWriter, r *http.Request) {
	fmt.Fprintf(w, "Hello, World! I am instrumented autoamtically!")
	ctx := r.Context()
	sleepy(ctx)
}

func sleepy(ctx context.Context) {
	client := http.Client{Transport: otelhttp.NewTransport(http.DefaultTransport)}
	_, span := tracer.Start(ctx, "sleep")
	defer span.End()

	req, _ := http.NewRequestWithContext(ctx, "GET", "https://localhost:7225/backend", nil)
	span.SetAttributes(attribute.String("sleep.duration", "i am not sleeping"))
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
