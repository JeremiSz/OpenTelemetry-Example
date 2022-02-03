namespace Helpers

open System
open System.Text
open System.Threading
open RabbitMQ.Client
open RabbitMQ.Client.Events

module RabbitHelper =
    let private HOST = "localhost"
    let Student_QUEUE = "data_stream"
    //let Strudent_ROUTING_KEY = "data_stream"
    let Workflow_QUEUE = "students"
    //let Workflow_ROUTING_KEY = "stu dents"

    let producer queue  = async {
        let factory = ConnectionFactory()
        factory.HostName <- HOST
        use connection = factory.CreateConnection()
        use channel = connection.CreateModel()
        let result = channel.QueueDeclare(queue = queue, durable = false, exclusive = false, autoDelete = false, arguments = null)

        let rand = Random()
        
        let message = sprintf "%f" (rand.NextDouble())
        let body = Encoding.UTF8.GetBytes(message)
        printfn "publish     : %s" message
        channel.BasicPublish(exchange = "", routingKey = queue, basicProperties = null, body = body)
    }

    let consumer queue (handler :obj -> BasicDeliverEventArgs -> unit) (token: CancellationTokenSource) = async {
        let factory = ConnectionFactory()
        factory.HostName <- HOST
        use connection = factory.CreateConnection()
        use channel = connection.CreateModel()
        let result = channel.QueueDeclare(queue = queue, durable = false, exclusive = false, autoDelete = false, arguments = null)

        let consumer = EventingBasicConsumer(channel)
        consumer.Received.AddHandler(new EventHandler<BasicDeliverEventArgs>(handler))

        let consumeResult = channel.BasicConsume(queue = queue, autoAck = true, consumer = consumer)


        while not token.IsCancellationRequested do
            Thread.Sleep(500)
    }

