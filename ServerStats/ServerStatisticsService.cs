
using System.Diagnostics;


namespace ServerMonitoringAndNotificationSystem.ServerStats
{
    public class ServerStatisticsService : IServerStatisticsService
    {
        public ServerStatistics CollectStatistics()
        {
            var process = Process.GetCurrentProcess();

            var memoryUsage = process.WorkingSet64 / 1024.0 / 1024.0;
            var availableMemory = GetAvailableMemoryInMB();
            var cpuUsage = GetCpuUsagePercentage();

            return new ServerStatistics
            {
                MemoryUsage = memoryUsage,
                AvailableMemory = availableMemory,
                CpuUsage = cpuUsage,
                Timestamp = DateTime.Now
            };
        }

        private double GetAvailableMemoryInMB()
        {
            using (var pc = new PerformanceCounter("Memory", "Available MBytes"))
            {
                return pc.NextValue();
            }
        }

        private double GetCpuUsagePercentage()
        {
            using (var pc = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
            {
                pc.NextValue();
                Thread.Sleep(1000);
                return pc.NextValue();
            }
        }

        public async Task StartCollectingAsync(int intervalSeconds, Action<ServerStatistics> onStatisticsCollected)
        {
            while (true)
            {
                var stats = CollectStatistics();
                onStatisticsCollected(stats);
                await Task.Delay(intervalSeconds * 1000);
            }
        }
    }
}
