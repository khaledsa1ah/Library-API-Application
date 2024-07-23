using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Library.Services
{
    public class RabbitMQService
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQService()
        {
            _factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "message_queue",
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }

        public void PublishMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                                  routingKey: "message_queue",
                                  basicProperties: null,
                                  body: body);
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}