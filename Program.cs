using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServerMonitoringAndNotificationSystem.Config;
using ServerMonitoringAndNotificationSystem.MessageQueue;
using ServerMonitoringAndNotificationSystem.ServerStats;


namespace ServerMonitoringAndNotificationSystem
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var rabbitMqConfig = context.Configuration.GetSection("RabbitMQ").Get<RabbitMqConfig>();
                    services.AddSingleton(rabbitMqConfig);
                    var serverStatisticsConfig = context.Configuration.GetSection("ServerStatisticsConfig").Get<ServerStatisticsConfig>();

                    services.AddSingleton<IMessageQueue, RabbitMqMessageQueue>();
                    services.AddSingleton<IServerStatisticsService, ServerStatisticsService>();
                    services.AddSingleton(serverStatisticsConfig);
                });

            var host = builder.Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var serverStatisticsService = services.GetRequiredService<IServerStatisticsService>();
                var messageQueue = services.GetRequiredService<IMessageQueue>();
                var serverStatisticsConfig = services.GetRequiredService<ServerStatisticsConfig>();

                await serverStatisticsService.StartCollectingAsync(
                    intervalSeconds: serverStatisticsConfig.IntervalSeconds,
                    onStatisticsCollected: stats => messageQueue.Publish(serverStatisticsConfig.ServerIdentifier, stats)
                );
            }
        }
    }
}
