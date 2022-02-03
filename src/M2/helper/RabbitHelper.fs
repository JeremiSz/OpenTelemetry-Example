namespace Helpers

open System
open System.Threading
open RabbitMQ.Client
open RabbitMQ.Client.Events


module RabbitHelper =
    let private HOST = "localhost"
    let Student_QUEUE = "data_stream"
    let Workflow_QUEUE = "students"
    let private connection = 
        let factory = ConnectionFactory()
        factory.HostName <- HOST
        factory.CreateConnection()

    let getChannel() =
        connection.CreateModel();

    let producer (channel:IModel) (publisher: unit -> byte[] * IBasicProperties) queue  = async {
        channel.QueueDeclare(queue = queue, durable = false, exclusive = false, autoDelete = false, arguments = null) |> ignore
        let body, basicProperties = publisher()
        channel.BasicPublish("", queue, basicProperties,body )
    }

    let consumer (channel:IModel) queue (handler :obj -> BasicDeliverEventArgs -> unit) (token: CancellationTokenSource) = async {
        let result = channel.QueueDeclare(queue = queue, durable = false, exclusive = false, autoDelete = false, arguments = null)

        let consumer = EventingBasicConsumer(channel)
        consumer.Received.AddHandler(new EventHandler<BasicDeliverEventArgs>(handler))

        let consumeResult = channel.BasicConsume(queue = queue, autoAck = true, consumer = consumer)


        while not token.IsCancellationRequested do
            Thread.Sleep(500)
    }

