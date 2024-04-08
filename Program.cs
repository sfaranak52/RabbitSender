using System.Text;
using RabbitMQ.Client;

ConnectionFactory factory = new();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
factory.ClientProvidedName = "RabbitSender";

IConnection cnn = factory.CreateConnection();
IModel channel = cnn.CreateModel();

var exchangeName = "TcpExchange";
var routingKey = "tcp-routing-key";
var queueName = "TcpQueue";

channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
channel.QueueDeclare(queueName,true,false,false,null);
channel.QueueBind(queueName, exchangeName,routingKey, null);

for(int i = 0; i < 6; i++)
{
    Console.WriteLine($"Sending TcpListener {i} ");
    byte[] messageBodyBytes = Encoding.UTF8.GetBytes($"TcpListener {i} ");
    var properties = channel.CreateBasicProperties();
    properties.Timestamp = new AmqpTimestamp((long)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    properties.Headers = new Dictionary<string, object>
    {
        { "Milliseconds", DateTimeOffset.UtcNow.Millisecond }
    };
    //var properties = channel.CreateBasicProperties();
    //properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    channel.BasicPublish(exchangeName, routingKey, properties, messageBodyBytes);
    Thread.Sleep(1000);
}

channel.Close();
cnn.Close();