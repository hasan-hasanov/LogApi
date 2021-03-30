using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NCrontab;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LogApi
{
    public class RequestCleanupJob : IHostedService
    {
        private readonly CrontabSchedule _schedule;
        private DateTime _nextRun;

        public RequestCleanupJob(IConfiguration configuration)
        {
            _schedule = CrontabSchedule.Parse($"*/{configuration["RequestCleanerInMinutes"]} * * * *");
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                while (DateTime.Now > _nextRun)
                {
                    // Clean requests

                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
