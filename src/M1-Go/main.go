package main

import (
	"context"
	"fmt"
	"log"
	"net/http"

	//"runtime/trace"

	"go.opentelemetry.io/contrib/instrumentation/net/http/otelhttp"
	"go.opentelemetry.io/otel"
	"go.opentelemetry.io/otel/baggage"
	"go.opentelemetry.io/otel/exporters/otlp/otlptrace"
	"go.opentelemetry.io/otel/exporters/otlp/otlptrace/otlptracegrpc"
	"go.opentelemetry.io/otel/propagation"

	//"go.opentelemetry.io/otel/exporters/otlp/otlptrace/otlptracehttp"

	"go.opentelemetry.io/otel/sdk/resource"
	sdktrace "go.opentelemetry.io/otel/sdk/trace"
	semconv "go.opentelemetry.io/otel/semconv/v1.7.0"
)

var tracer = otel.Tracer("tracer")

func homePage(w http.ResponseWriter, r *http.Request) {
	if r.Method == "GET" {
		client := http.Client{Transport: otelhttp.NewTransport(http.DefaultTransport)}
		ctx := context.Background()
		ctx, span := tracer.Start(ctx, "Calling M2")
		//ctx, span := trace.SpanFromContext(ctx).TracerProvider().Tracer("tracer").Start(ctx, "Calling M2")

		defer span.End()

		fmt.Print(span.SpanContext().SpanID())

		//propagation.TraceContext.Inject(ctx, propagation.TextMapCarrier())

		req, _ := http.NewRequestWithContext(ctx, "GET", "https://localhost:7225/backend", nil)

		client.Do(req)
	} else if r.Method == "PUT" {
		//handle put here
	} else {
		fmt.Println("Method not supported")
	}

}

func handleRequests() {
	handler := http.HandlerFunc(homePage)
	wrappedHandler := otelhttp.NewHandler(handler, "hello world", otelhttp.WithTracerProvider(otel.GetTracerProvider()))
	//return otelhttp.NewHandler(handler, name, otelhttp.WithTracerProvider(otel.GetTracerProvider()))
	http.Handle("/", wrappedHandler)
	log.Fatal(http.ListenAndServe(":10000", nil))
}

func newExporter(ctx context.Context) (*otlptrace.Exporter, error) {
	opts := []otlptracegrpc.Option{
		otlptracegrpc.WithInsecure(),
	}

	client := otlptracegrpc.NewClient(opts...)
	return otlptrace.New(ctx, client)
}

func newTraceProvider(exp *otlptrace.Exporter) *sdktrace.TracerProvider {
	// The service.name attribute is required.
	resource :=
		resource.NewWithAttributes(
			semconv.SchemaURL,
			semconv.ServiceNameKey.String("M1-Go"),
		)

	return sdktrace.NewTracerProvider(
		sdktrace.WithBatcher(exp),
		sdktrace.WithResource(resource),
	)
}

func main() {
	ctx := baggage.ContextWithoutBaggage(context.Background())

	// Configure a new exporter using environment variables for sending data to Honeycomb over gRPC.
	exp, err := newExporter(ctx)
	if err != nil {
		log.Fatalf("failed to initialize exporter: %v", err)
	}

	// Create a new tracer provider with a batch span processor and the otlp exporter.
	tp := newTraceProvider(exp)

	// Handle this error in a sensible manner where possible
	defer func() { _ = tp.Shutdown(ctx) }()

	// Set the Tracer Provider and the W3C Trace Context propagator as globals
	otel.SetTracerProvider(tp)

	// Register the trace context and baggage propagators so data is propagated across services/processes.
	otel.SetTextMapPropagator(
		propagation.NewCompositeTextMapPropagator(
			propagation.TraceContext{},
			propagation.Baggage{},
		),
	)
	handleRequests()
}
