namespace CreditCard.Infrastructure.Messaging;

using System.Text;
using System.Text.Json;

public class RabbitMqPublisher : IMessagePublisher
{
    public Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default) where T : class
    {
        var messageJson = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(messageJson);
        
        Console.WriteLine($"[RabbitMQ] Publicando mensaje en: {routingKey}");
        Console.WriteLine($"[RabbitMQ] Mensaje: {messageJson}");
        
        return Task.CompletedTask;
    }
}
