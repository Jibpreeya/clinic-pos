using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace ClinicPOS.Infrastructure.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string exchange, string routingKey, T message) where T : class;
}

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly IConfiguration _config;

    public RabbitMqPublisher(IConfiguration config) => _config = config;

    private async Task EnsureConnectedAsync()
    {
        if (_channel is not null) return;
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"] ?? "rabbitmq",
            UserName = _config["RabbitMQ:Username"] ?? "clinicpos",
            Password = _config["RabbitMQ:Password"] ?? "clinicpos123",
        };
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
    }

    public async Task PublishAsync<T>(string exchange, string routingKey, T message) where T : class
    {
        await EnsureConnectedAsync();
        await _channel!.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties { Persistent = true };
        await _channel.BasicPublishAsync(exchange, routingKey, false, props, body);
    }

    public void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _connection?.CloseAsync().GetAwaiter().GetResult();
    }
}
