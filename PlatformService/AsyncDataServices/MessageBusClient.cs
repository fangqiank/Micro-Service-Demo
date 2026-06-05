using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IChannel? _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task EnsureConnectedAsync()
        {
            if (_connection != null && _connection.IsOpen) return;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);

                _connection.ConnectionShutdownAsync += (_, _) =>
                {
                    Console.WriteLine("--> RabbitMQ connection shutdown");
                    return Task.CompletedTask;
                };

                Console.WriteLine("--> Connected to MessageBus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the message bus: {ex.Message}");
            }
        }

        public async Task PublishNewPlatformAsync(PlatformPublishDto platform)
        {
            await EnsureConnectedAsync();

            var message = JsonSerializer.Serialize(platform);

            if (_connection?.IsOpen == true)
            {
                Console.WriteLine("--> RabbitMQ Connection open, sending message ...");
                var body = Encoding.UTF8.GetBytes(message);
                await _channel!.BasicPublishAsync(exchange: "trigger", routingKey: "", body: body);
                Console.WriteLine($"--> We have sent {message}");
            }
            else
            {
                Console.WriteLine("--> RabbitMQ Connection closed, not sending");
            }
        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("MessageBus disposed");

            if (_channel?.IsOpen == true)
            {
                await _channel.CloseAsync();
            }
            if (_connection?.IsOpen == true)
            {
                await _connection.CloseAsync();
            }
        }
    }
}
