package controllers;

import io.opentelemetry.api.OpenTelemetry;
import io.opentelemetry.api.trace.propagation.W3CTraceContextPropagator;
import io.opentelemetry.context.propagation.ContextPropagators;
import io.opentelemetry.exporter.otlp.trace.OtlpGrpcSpanExporter;
import io.opentelemetry.extension.annotations.WithSpan;
import io.opentelemetry.sdk.OpenTelemetrySdk;
import io.opentelemetry.sdk.trace.SdkTracerProvider;
import io.opentelemetry.sdk.trace.export.BatchSpanProcessor;
import play.mvc.*;
import io.opentelemetry.api.trace.Span;

import java.io.IOException;


/**
 * This controller contains an action to handle HTTP requests
 * to the application's home page.
 */
public class HomeController extends Controller {

    /**
     * An action that renders an HTML page with a welcome message.
     * The configuration in the <code>routes</code> file means that
     * this method will be called when the application receives a
     * <code>GET</code> request with a path of <code>/</code>.
     */
    // private OpenTelemetry openTelemetry;

    public HomeController(){
//        SdkTracerProvider sdkTracerProvider = SdkTracerProvider.builder()
//                .addSpanProcessor(BatchSpanProcessor.builder(OtlpGrpcSpanExporter.builder().build()).build())
//                .build();
//
//        openTelemetry = OpenTelemetrySdk.builder()
//                .setTracerProvider(sdkTracerProvider)
//                .setPropagators(ContextPropagators.create(W3CTraceContextPropagator.getInstance()))
//                .buildAndRegisterGlobal();
    }

    public Result index() {
        helperOne();
        return ok("yes");
    }

    @WithSpan
    private void helperOne(){
        Span span = Span.current();
        span.setAttribute("hi","hi");
        span.recordException(new IOException());
    }
}
