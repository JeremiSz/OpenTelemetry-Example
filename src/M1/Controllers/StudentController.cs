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

namespace M1.Controllers
{
   [ApiController]
   [Route("api/student")]
    public class StudentController : Controller
    {
        public StudentController(ActivitySource activitySource)
        {
            if(activitySource == null)
                throw new ArgumentNullException(nameof(activitySource));

            this.activitySource = activitySource;
        }

        ActivitySource activitySource;
        
        [HttpGet]
        public string get()
        {
            var activity = Activity.Current;
            activity?.SetTag("Get", "Get request made to M1.");

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

            helper();
            
            HttpClient client = new HttpClient();
            HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7225/backend");
            HttpResponseMessage httpResponse = client.Send(httpRequest); 
            
            return "post students";
        }

        void helper()
        {
            using var activity = activitySource.StartActivity("M1");
            activity?.SetBaggage("Done", "helper");
        }
    }
}
