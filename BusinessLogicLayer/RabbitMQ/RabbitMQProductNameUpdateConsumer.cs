using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductNameUpdateConsumer : IRabbitMQProductNameUpdateConsumer, IDisposable
{
    private readonly ILogger<RabbitMQProductNameUpdateConsumer> _logger;
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQProductNameUpdateConsumer(IConfiguration configuration,
        ILogger<RabbitMQProductNameUpdateConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;

        string hostName = _configuration["RabbitMQ_HostName"]!;
        string userName = _configuration["RabbitMQ_UserName"]!;
        string password = _configuration["RabbitMQ_Password"]!;
        string port = _configuration["RabbitMQ_Port"]!;

        IConnectionFactory connectionFactory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = Convert.ToInt32(port)
        };

        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
    }
        
    public void Consume()
    {
        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
        
        string routingKey = "product.update.name";
        string queueName = "orders.product.update.name.queue";
        
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true);

        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        // with arguments (object mapping), you can provide x-message-ttl or x-max-length or x-expired 
        
        // Bind message queue to exchange
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey, arguments: null);

        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (sender, args) =>
        {
            byte[] bodyInBytes = args.Body.ToArray();
            string messageJson = Encoding.UTF8.GetString(bodyInBytes);

            if (messageJson != null)
            {
                ProductNameUpdateMessage? productNameUpdateMessage = JsonSerializer.Deserialize<ProductNameUpdateMessage>(messageJson);
                
                _logger.LogInformation($"Product name updated: {productNameUpdateMessage.ProductId}, New product name: {productNameUpdateMessage.NewName}");
            }
        };

        _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);
    }

    public void Dispose()
    {
        _connection.Dispose();
        _channel.Dispose();
    }
}