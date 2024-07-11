using Newtonsoft.Json;
using RabbitMQ.Client;
using ServerMonitoringAndNotificationSystem.Config;
using ServerMonitoringAndNotificationSystem.ServerStats;
using System.Text;

namespace ServerMonitoringAndNotificationSystem.MessageQueue
{
    public class RabbitMqMessageQueue : IMessageQueue
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqMessageQueue(RabbitMqConfig config)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = config.HostName,
                    UserName = config.UserName,
                    Password = config.Password
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: "server_statistics", type: ExchangeType.Topic);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing RabbitMQ: {ex.Message}");
                throw;
            }
        }

        public void Publish(string topic, ServerStatistics stats)
        {
            try
            {
                var message = JsonConvert.SerializeObject(stats);
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(exchange: "server_statistics",
                                      routingKey: topic,
                                      basicProperties: null,
                                      body: body);
                Console.WriteLine($"[x] Sent {topic}: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing message: {ex.Message}");
            }
        }

        ~RabbitMqMessageQueue()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing RabbitMQ connection: {ex.Message}");
            }
        }
    }
}
