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
        private CancellationTokenSource _cts;

        public RequestCleanupJob(IConfiguration configuration)
        {
            _schedule = CrontabSchedule.Parse($"*/{configuration["RequestCleanerInMinutes"]} * * * *");
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Link these tokens so if it is requested from ASP.NET we can honor this request in StartAsync and stop async.
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            while (!_cts.IsCancellationRequested)
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
            _cts.Cancel();
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }
}
