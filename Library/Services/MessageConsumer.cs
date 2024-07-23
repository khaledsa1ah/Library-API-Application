using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Library.DTOs;
using Serilog;

namespace Library.Services
{
    public class MessageConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly Serilog.Core.Logger _logger;

        public MessageConsumer()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "message_queue",
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            // Configure Serilog to log to a file
            _logger = new LoggerConfiguration()
                .WriteTo.File("logs/received_messages.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var jsonMessage = Encoding.UTF8.GetString(body);

                try
                {
                    var messageDto = JsonSerializer.Deserialize<MessageDto>(jsonMessage);
                    if (messageDto != null)
                    {
                        _logger.Information("Received message content: {Content}", messageDto.Content);
                    }
                    else
                    {
                        _logger.Warning("Received message could not be deserialized.");
                    }
                }
                catch (JsonException)
                {
                    _logger.Error("Received message is not in the expected JSON format.");
                }
            };

            _channel.BasicConsume(queue: "message_queue",
                                  autoAck: true,
                                  consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            _logger.Dispose();
            base.Dispose();
        }
    }
}