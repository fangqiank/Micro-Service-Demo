using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandService.EventProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _processor;
        private IConnection? _connection;
        private IChannel? _channel;
        private string _queueName = string.Empty;

        public MessageBusSubscriber(IConfiguration configuration, IEventProcessor processor)
        {
            _configuration = configuration;
            _processor = processor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"]!)
            };

            const int maxRetries = 5;
            const int delayMs = 5000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _connection = await factory.CreateConnectionAsync(stoppingToken);
                    _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                    await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout,
                        cancellationToken: stoppingToken);

                    var queueDeclareResult = await _channel.QueueDeclareAsync(cancellationToken: stoppingToken);
                    _queueName = queueDeclareResult.QueueName;

                    await _channel.QueueBindAsync(queue: _queueName, exchange: "trigger", routingKey: "",
                        cancellationToken: stoppingToken);

                    _connection.ConnectionShutdownAsync += (_, _) =>
                    {
                        Console.WriteLine("--> Connection Shutdown");
                        return Task.CompletedTask;
                    };

                    Console.WriteLine("--> Listening on the message bus...");

                    var consumer = new AsyncEventingBasicConsumer(_channel);

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        Console.WriteLine("--> Event received");

                        var body = ea.Body.ToArray();
                        var notificationMsg = Encoding.UTF8.GetString(body);

                        _processor.ProcessEvent(notificationMsg);
                    };

                    await _channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer,
                        cancellationToken: stoppingToken);

                    // Keep the background task alive until cancellation is requested
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                    return;
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("--> MessageBusSubscriber stopped");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> RabbitMQ connection attempt {attempt}/{maxRetries} failed: {ex.Message}");

                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"--> Retrying in {delayMs}ms...");
                        await Task.Delay(delayMs, stoppingToken);
                    }
                }
            }

            Console.WriteLine($"--> Could not connect to RabbitMQ after {maxRetries} attempts");
        }

        public override async void Dispose()
        {
            if (_channel?.IsOpen == true)
            {
                await _channel.CloseAsync();
            }
            if (_connection?.IsOpen == true)
            {
                await _connection.CloseAsync();
            }

            base.Dispose();
        }
    }
}
