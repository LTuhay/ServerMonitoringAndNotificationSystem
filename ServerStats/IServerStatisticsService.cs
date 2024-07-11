
namespace ServerMonitoringAndNotificationSystem.ServerStats
{
    public interface IServerStatisticsService
    {
        ServerStatistics CollectStatistics();
        Task StartCollectingAsync(int intervalSeconds, Action<ServerStatistics> onStatisticsCollected);
    }
}
