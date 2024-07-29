using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UserService.Configuration;
using OrderService.DTO;

namespace UserService.Services
{
    public class NotificationServiceListener
    {
        private readonly RabbitMqService _rabbitMqService;
        private readonly ILogger<NotificationServiceListener> _logger;

        public NotificationServiceListener(RabbitMqService rabbitMqService, ILogger<NotificationServiceListener> logger)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        public void Start()
        {
            _logger.LogInformation("Starting NotificationServiceListener...");

            var channel = _rabbitMqService.Channel;

            channel.QueueDeclare(queue: "orderQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<SendOrderNotificationDTO>(message);

                _logger.LogInformation(@"
                    -------------------------------------
                    Mocked sending notification to user:
                    -------------------------------------
                    UserId: {UserId}
                    UserEmail: {UserEmail}
                    UserName: {UserName}
                    OrderDate: {OrderDate}
                    TotalAmount: {TotalAmount:C}
                    Product: {Product}
                    -------------------------------------
                    ",
                    request.UserId,
                    request.UserEmail,
                    request.UserName,
                    request.OrderDate,
                    request.TotalAmount,
                    request.Product
                );
            };

            channel.BasicConsume(queue: "orderQueue", autoAck: true, consumer: consumer);

            _logger.LogInformation("NotificationServiceListener started.");
        }
    }
}
