using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UserService.Configuration;
using OrderService.DTO;
using UserService.Persistence;

namespace UserService.Services
{
    public class UserServiceListener
    {
        private readonly RabbitMqService _rabbitMqService;
        private readonly ILogger<UserServiceListener> _logger;
        private readonly AppDbContext _appDbContext;

        public UserServiceListener(RabbitMqService rabbitMqService, ILogger<UserServiceListener> logger, AppDbContext dbContext)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
            _appDbContext = dbContext;
        }

        public void Start()
        {
            _logger.LogInformation("Starting UserServiceListener...");

            var channel = _rabbitMqService.Channel;

            channel.QueueDeclare(queue: "userRequestQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<RequestMessage>(message);

                _logger.LogInformation($"Received request for UserId: {request.UserId}");

                var userDetails = _appDbContext.Users.FirstOrDefault(e => e.Id == request.UserId);

                var responseProps = channel.CreateBasicProperties();
                responseProps.CorrelationId = ea.BasicProperties.CorrelationId;

                var responseBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(userDetails));

                channel.BasicPublish(exchange: "", routingKey: ea.BasicProperties.ReplyTo, basicProperties: responseProps, body: responseBody);

                _logger.LogInformation($"Sent response for UserId: {request.UserId}");
            };

            channel.BasicConsume(queue: "userRequestQueue", autoAck: true, consumer: consumer);

            _logger.LogInformation("UserServiceListener started.");
        }
    }

    public class RequestMessage
    {
        public int UserId { get; set; }
    }
}
