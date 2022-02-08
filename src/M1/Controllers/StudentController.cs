using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using M1.helpers;


namespace M1.Controllers
{
   [ApiController]
   [Route("api/student")]
    public class StudentController : Controller
    {
        Random random = new Random();
        MetricsHelper metricsHelper;
        public static long requestCount = 0;
        public static long CPU = 0;

        public StudentController(ActivitySource activitySource, MetricsHelper metricsHelper)
        {
            if (activitySource == null)
                throw new ArgumentNullException(nameof(activitySource));

            this.activitySource = activitySource;
            this.metricsHelper = metricsHelper;
        }

        ActivitySource activitySource;
        
        [HttpGet]
        public string get()
        {
            var activity = Activity.Current;
            activity?.SetTag("Get", "Get request made to M1.");
            requestCount++;

            metricsHelper.requestCounter.Add(1);

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

            metricsHelper.latency.Record(random.Next(10), KeyValuePair.Create<string, object?>("hello", "hi"));
            CPU = random.Next();

            return "post students";
        }

        void helper()
        {
            using var activity = activitySource.StartActivity("M1");
            activity?.SetBaggage("Done", "helper");
            
        }
    }
}
