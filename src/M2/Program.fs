namespace M2
#nowarn "20"
open System
open System.Text
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open OpenTelemetry.Trace
open OpenTelemetry.Resources
open TraceProvider
open RabbitMQ.Client
open RabbitMQ.Client.Events

module Program =
    let exitCode = 0
    let serviceName = "CCS.OpenTelemetry.M2";
    let serviceVersion = "1.0.0";

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)
        builder.Services.AddOpenTelemetryTracing(fun builder ->
            builder
                .AddJaegerExporter()
                .AddSource(serviceName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName,serviceVersion=serviceVersion))
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddMongoDBInstrumentation()
            |> ignore
        )
        builder.Services.AddTransient<IActivityService, ActivityService>()
        builder.Services.AddControllers()
        

        let app = builder.Build()

        app.UseHttpsRedirection()

        app.UseAuthorization()

        let factory = new ConnectionFactory()
        do factory.HostName <- "localhost"
        //do factory.UserName <- "consumer"
        let exchangeName = "stonks"
        let queueName = "testing"
        let routingKey = "RabbitMQ"

        app.MapControllers()

        app.Run()

        let connection = factory.CreateConnection()
        let channel = connection.CreateModel()
        channel.ExchangeDeclare(exchangeName,ExchangeType.Fanout,false,false,null);
        do channel.QueueBind(queueName,exchangeName,routingKey,null);
        //do channel.QueueDeclare(queueName,false,false,false,null) |> ignore
        let consumer = new EventingBasicConsumer(channel)

        do consumer.Received.AddHandler(new EventHandler<BasicDeliverEventArgs>(fun sender (data:BasicDeliverEventArgs) -> 
            let body = data.Body.ToArray()
            let message = Encoding.UTF8.GetString(body)
            Console.WriteLine message));

        let consumeTesult = channel.BasicConsume(queueName,true,consumer)
        Console.WriteLine consumeTesult

        exitCode
