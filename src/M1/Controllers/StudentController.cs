using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System.Diagnostics;
using System.Diagnostics.Metrics;


namespace M1.Controllers
{
   [ApiController]
   [Route("api/student")]
    public class StudentController : Controller
    {
        Meter meter = new Meter("M1 Controller");
        Counter<long> counter;
        Histogram<double> histogram;
        Random random = new Random();

        public StudentController(ActivitySource activitySource)
        {
            if (activitySource == null)
                throw new ArgumentNullException(nameof(activitySource));

            this.activitySource = activitySource;

            counter = meter.CreateCounter<long>("RequestCounter", "total");
            histogram = meter.CreateHistogram<double>("RandomHistogram");
        }

        ActivitySource activitySource;
        
        [HttpGet]
        public string get()
        {
            var activity = Activity.Current;
            activity?.SetTag("Get", "Get request made to M1.");

            counter.Add(1);

            helper();
            
            HttpClient client = new HttpClient();
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7225/backend");
            HttpResponseMessage httpResponse = client.Send(httpRequest); 

            return "Get Students";
        }

        [HttpPost]
        public string post()
        {
            var activity = Activity.Current;
            activity?.SetTag("Post", "Post request made to M1.");

            HttpClient client = new HttpClient();
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7225/backend");
            HttpResponseMessage httpResponse = client.Send(httpRequest);

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            histogram.Record(random.Next(10), KeyValuePair.Create<string, object>("hello", "hi"));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.


            return "post students";
        }

        void helper()
        {
            using var activity = activitySource.StartActivity("M1");
            activity?.SetBaggage("Done", "helper");
            
        }
    }
}
