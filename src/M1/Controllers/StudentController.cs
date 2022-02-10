using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using M1.helpers;
using Npgsql;
using System.Data;

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
        ILogger<StudentController> logger;

        public StudentController(ILogger<StudentController> logger, ActivitySource activitySource, MetricsHelper metricsHelper)
        {
            if (activitySource == null)
                throw new ArgumentNullException(nameof(activitySource));

            this.activitySource = activitySource;
            this.metricsHelper = metricsHelper;
            this.logger = logger;
        }

        ActivitySource activitySource;

        [HttpGet]
        public string get()
        {
            var activity = Activity.Current;
            activity?.SetTag("Get", "Get request made to M1.");
            requestCount++;

            metricsHelper.requestCounter.Add(1);

            logger.LogInformation("Get request has been made to /api/student in M1.", new string[] { });
            logger.LogError(new IOException(), "IO Exception thrown.", new string[] { });
            logger.LogWarning("This is a warning.", new string[] { });

            HttpClient client = new HttpClient();
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7225/backend");
            try
            {
                HttpResponseMessage httpResponse = client.Send(httpRequest);
            }
            catch (Exception ex)
            {
                activity?.AddEvent(new ActivityEvent(ex.ToString()));
            }

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

            helper();

            return "post students";
        }

        [HttpPut]
        public string put()
        {
            string connectionStr = "Host=host.docker.internal;Port=5432;Username=postgres;Password=mysecretpassword;Database=postgres;";
            var connection = new NpgsqlConnection(connectionStr);
            connection.Open();
            using (var command = new NpgsqlCommand("INSERT INTO test VALUES (1)", connection))
            {
                command.ExecuteNonQuery();
            }
            return "yey"; 
        }

        void helper()
        {
            using var activity = activitySource.StartActivity("M1");
            activity?.SetBaggage("Done", "helper");
            activity?.AddEvent(new ActivityEvent("Helper did work"));
        }

        [HttpOptions]
        public String options()
        {
            //logger.LogInformation("Hello I am logging");
            //logger.LogError(new EventId(1),"ahh i broke");
            //logger.LogWarning(new EventId(2), "you have been warned");

            return "logged";
        }
    }
}
