using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using M1.helpers;
using Npgsql;
using System.Net;

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
        HttpClient client;

        public StudentController(ILogger<StudentController> logger, ActivitySource activitySource, MetricsHelper metricsHelper)
        {
            if (activitySource == null)
                throw new ArgumentNullException(nameof(activitySource));

            this.activitySource = activitySource;
            this.metricsHelper = metricsHelper;
            this.logger = logger;
            this.client = new HttpClient();
        }

        ActivitySource activitySource;

        [HttpGet]
        public async Task<string>? get()
        {
            var activity = Activity.Current;
            activity?.SetTag("Get", "Get request made to M1.");
            requestCount++;

            metricsHelper.requestCounter.Add(1);

            logger.LogInformation("Get request has been made to /api/student in M1.", new string[] { });
            logger.LogError(new IOException(), "IO Exception thrown.", new string[] { });
            logger.LogWarning("This is a warning.", new string[] { });

            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7225/backend");
            
            try
            {
                return await (await client.SendAsync(httpRequest)).Content.ReadAsStringAsync();

            }
            catch (Exception ex)
            {
                activity?.AddEvent(new ActivityEvent(ex.ToString()));
                return null;
            }
        }

        [HttpPost]
        public Task<HttpResponseMessage>? post()
        {
            var activity = Activity.Current;
            activity?.SetTag("Post", "Post request made to M1.");

            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7225/backend");
            Task<HttpResponseMessage>? task;
            try
            {
                task = client.SendAsync(httpRequest);
            }
            catch (Exception ex)
            {
                activity?.AddEvent(new ActivityEvent(ex.ToString()));
                task = null;
            }

            metricsHelper.latency.Record(random.Next(10), KeyValuePair.Create<string, object?>("hello", "hi"));
            CPU = random.Next();

            helper();

            return task;
        }

        [HttpPut]
        public string put()
        {
            string connectionStr = "Host=host.docker.internal;Port=5432;Username=postgres;Password=mysecretpassword;Database=postgres;";

            var conn = new NpgsqlConnection(connectionStr);
            conn.Open();

            using (var command = new NpgsqlCommand("INSERT INTO test VALUES (1)", conn))
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
    }
}
