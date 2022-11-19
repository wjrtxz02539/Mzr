using Microsoft.Extensions.Configuration;
using Mzr.Share.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mzr.Service.Crawler.Utils
{
    internal class CrawlerConfiguration
    {
        public CrawlerServiceConfiguration Crawler { get; set; }
        public CrawlerConfiguration(IConfiguration configuration)
        {
            var crawler = configuration.GetRequiredSection("Crawler").Get<CrawlerServiceConfiguration>();
            if (crawler is null)
            {
                throw new NullReferenceException(nameof(crawler));
            }

            Crawler = crawler;
            Crawler.Validate();

        }
    }

    internal class CrawlerServiceConfiguration
    {
        public List<CrawlerServiceTaskConfiguration> Tasks { get; set; } = new();
        public CrawlerServiceConfiguration()
        { }

        public void Validate()
        {
            foreach (var task in Tasks)
                task.Validate();
        }
    }

    public class CrawlerServiceTaskConfiguration
    {
        public long UserId { get; set; }
        public int MaxDynamicConcurrency { get; set; } = 3;
        public int MaxReplyConcurrency { get; set; } = 5;
        public bool Force { get; set; } = false;
        public int Skip { get; set; } = 0;
        public int? MaxDepth { get; set; } = null;
        public int? MaxDays { get; set; } = null;

        public int RequestTimeout { get; set; } = 10;
        public int RestartMinutes { get; set; } = 5;

        public bool Repeat { get; set; } = true;

        public void Validate()
        {
            if (UserId <= 0 
                || MaxDynamicConcurrency < 0
                || MaxReplyConcurrency < 0
                || RequestTimeout < 0
                || Skip < 0
                || (MaxDepth.HasValue && MaxDepth.Value <= 0)
                || (MaxDays.HasValue && MaxDays.Value <= 0))
            {
                throw new ArgumentException("Failed to parse CrawlerServiceTaskConfiguration.");
            }
        }
    }
}
