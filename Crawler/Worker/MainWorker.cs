using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mzr.Service.Crawler.Utils;
using Mzr.Share.Interfaces;
using Mzr.Share.Utils;

namespace Mzr.Service.Crawler.Worker
{
    internal class MainWorker : BackgroundService
    {
        private readonly IHost host;
        private readonly IHostApplicationLifetime lifetime;
        private readonly CrawlerServiceConfiguration crawlerConfiguration;
        private List<Task> tasks = new();
        public MainWorker(IHost host, IHostApplicationLifetime hostApplicationLifetime)
        {
            this.host = host;
            lifetime = hostApplicationLifetime;
            crawlerConfiguration = host.Services.GetRequiredService<CrawlerConfiguration>().Crawler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var stats = host.Services.GetRequiredService<WorkerStats>();
            tasks = new List<Task>();
            foreach (var task in crawlerConfiguration.Tasks)
            {
                var worker = new UserWorker(host.Services.GetRequiredService<ILogger<UserWorker>>(), host, task);
                stats.Workers.TryAdd(task.UserId, worker);
                tasks.Add(new Task(async () => await worker.ExecuteAsync(stoppingToken)));
            }

            var monitorTask = new Task(async () => await MonitorTask(stoppingToken));
            monitorTask.Start();
            await monitorTask.WaitAsync(stoppingToken);
        }

        private async Task MonitorTask(CancellationToken stoppingToken)
        {
            foreach (var task in tasks)
                task.Start();

            foreach (var task in tasks)
                await task.WaitAsync(stoppingToken);
        }
    }
}
