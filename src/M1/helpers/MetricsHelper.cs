using System.Diagnostics.Metrics;

namespace M1.helpers
{
    public class MetricsHelper
    {
        private readonly Meter meter;
 
        public Counter<long> requestCounter { get; private set; }
        public Histogram<double> latency { get; private set; }
        public ObservableCounter<long> observer_b { get; private set; }
        public ObservableGauge<long> observer_c { get; private set; }
        public MetricsHelper(Func<Measurement<long>> getCounterMeasurement, Func<Measurement<long>> getGaugeMeasurment)
        {
            meter = new Meter("CCS.OpenTelemetry.M1");
            requestCounter = meter.CreateCounter<long>("m1_request_counter", "total");
            latency = meter.CreateHistogram<double>("m1_random_histogram");
            observer_b = meter.CreateObservableCounter("m1_real_world_metric", getCounterMeasurement);
            observer_c = meter.CreateObservableGauge("m1_real_world_amount", getGaugeMeasurment);
        }
    }
}
