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
        private Task _executingTask;

        public RequestCleanupJob(IConfiguration configuration)
        {
            _schedule = CrontabSchedule.Parse($"*/{configuration["RequestCleanerInMinutes"]} * * * *");
            _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Link these tokens so if it is requested from ASP.NET we can honor this request in StartAsync and stop async.
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _executingTask = ExecuteAsync(_cts.Token);

            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
            {
                return;
            }

            _cts.Cancel();
            await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));
            cancellationToken.ThrowIfCancellationRequested();
        }

        protected async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(1);

            while (!_cts.IsCancellationRequested)
            {
                while (DateTime.Now > _nextRun)
                {
                    // Clean requests
                    _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                }
            }
        }
    }
}
