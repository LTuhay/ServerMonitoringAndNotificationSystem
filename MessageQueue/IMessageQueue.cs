

using ServerMonitoringAndNotificationSystem.ServerStats;

namespace ServerMonitoringAndNotificationSystem.MessageQueue
{
    public interface IMessageQueue
    {
        void Publish(string topic, ServerStatistics statistics);
    }
}
