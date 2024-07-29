using Newtonsoft.Json;
using OrderService.DTO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OrderService.Utils
{
    public class RabbitMQUtils
    {
        public static void sendOrderNotification(IServiceProvider serviceProvider, SendOrderNotificationDTO newOrder)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var rabbitMqService = scope.ServiceProvider.GetRequiredService<RabbitMqService>();
                using (var channel = rabbitMqService.CreateChannel())
                {
                    // Declare the queue with detailed logging
                    channel.QueueDeclare(queue: "orderQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(newOrder));
                    Console.WriteLine("About to publish order notification: " + newOrder.ToString());
                    Console.WriteLine($"UserId: {newOrder.UserId}");
                    Console.WriteLine($"UserEmail: {newOrder.UserEmail}");
                    Console.WriteLine($"UserName: {newOrder.UserName}");
                    Console.WriteLine($"OrderDate: {newOrder.OrderDate}");
                    Console.WriteLine($"TotalAmount: {newOrder.TotalAmount}");
                    Console.WriteLine($"Product: {newOrder.Product}");
                    Console.WriteLine("Body: " + body);

                    // Publish the message with detailed logging
                    channel.BasicPublish(exchange: "", routingKey: "orderQueue", basicProperties: null, body: body);
                    Console.WriteLine("Message published to orderQueue successfully.");
                }
            }
        }

        public static async Task<UserDetailsDTO> requestUserDetailsAsync(IServiceProvider serviceProvider, int userId)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var rabbitMqService = scope.ServiceProvider.GetRequiredService<RabbitMqService>();
                using (var channel = rabbitMqService.CreateChannel())
                {
                    var correlationId = Guid.NewGuid().ToString();
                    var replyQueueName = $"reply_{correlationId}";

                    // Declare the reply queue
                    channel.QueueDeclare(queue: replyQueueName, durable: false, exclusive: true, autoDelete: true, arguments: null);

                    var props = channel.CreateBasicProperties();
                    props.CorrelationId = correlationId;
                    props.ReplyTo = replyQueueName;

                    var message = new { UserId = userId };
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                    // Send the request to the UserService request queue
                    channel.BasicPublish(exchange: "", routingKey: "userRequestQueue", basicProperties: props, body: body);

                    var tcs = new TaskCompletionSource<UserDetailsDTO>();

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        if (ea.BasicProperties.CorrelationId == correlationId)
                        {
                            var response = Encoding.UTF8.GetString(ea.Body.ToArray());
                            var userDetails = JsonConvert.DeserializeObject<UserDetailsDTO>(response);
                            tcs.SetResult(userDetails);
                        }
                    };

                    channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consumer);

                    return await tcs.Task;
                }
            }
        }
    }
}
