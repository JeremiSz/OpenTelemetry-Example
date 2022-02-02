namespace Rabbit

open System
open System.Text
open System.Threading
open RabbitMQ.Client
open RabbitMQ.Client.Events

module Hello =

    let producer hostname queue routingKey (token: CancellationTokenSource) = async {
        let factory = ConnectionFactory()
        factory.HostName <- hostname
        use connection = factory.CreateConnection()
        use channel = connection.CreateModel()
        let result = channel.QueueDeclare(queue = queue, durable = false, exclusive = false, autoDelete = false, arguments = null)

        let rand = Random()
  
        while not token.IsCancellationRequested do
            let message = sprintf "%f" (rand.NextDouble())
            let body = Encoding.UTF8.GetBytes(message)
            printfn "publish     : %s" message
            channel.BasicPublish(exchange = "", routingKey = routingKey, basicProperties = null, body = body)
            Thread.Sleep(500)
    }

    let consumer id hostname queue (token: CancellationTokenSource) = async {
        let factory = ConnectionFactory()
        factory.HostName <- hostname
        use connection = factory.CreateConnection()
        use channel = connection.CreateModel()
        let result = channel.QueueDeclare(queue = queue, durable = false, exclusive = false, autoDelete = false, arguments = null)

        let consumer = EventingBasicConsumer(channel)
        consumer.Received.AddHandler(new EventHandler<BasicDeliverEventArgs>(fun sender (data:BasicDeliverEventArgs) -> 
        let body = data.Body.ToArray()
        let message = Encoding.UTF8.GetString(body)
        printfn "consumed [%s]: %s" id message))

        let consumeResult = channel.BasicConsume(queue = queue, autoAck = true, consumer = consumer)

        while not token.IsCancellationRequested do
        Thread.Sleep(500)
    }

