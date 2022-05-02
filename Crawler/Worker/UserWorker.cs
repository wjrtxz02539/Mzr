using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Mzr.Service.Crawler.Utils;
using Mzr.Share.Models.Bilibili;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Utils;
using Mzr.Share.Repositories.Bilibili.Web;
using System.Threading.Tasks.Dataflow;
using Mzr.Share.Models.Bilibili.Raw;
using System.Xml.Xsl;

namespace Mzr.Service.Crawler.Worker
{
    public class UserWorker
    {
        private readonly ILogger logger;
        private readonly IHost host;
        private readonly CrawlerServiceTaskConfiguration configuration;
        private string logPrefix;

        public StatusDetail Status { get; set; }
        public bool Running { get; set; } = false;

        public UserWorker(ILogger<UserWorker> logger, IHost host, CrawlerServiceTaskConfiguration configuration)
        {
            this.logger = logger; 
            this.host = host;
            this.configuration = configuration;

            logPrefix = $"[{configuration.UserId}]";

            Status = new StatusDetail() { UserId = configuration.UserId };
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var webUser = host.Services.GetRequiredService<WebBiliUserRepository>();
            var dynamicRepo = host.Services.GetRequiredService<IBiliDynamicRepository>();
            var userRepo = host.Services.GetRequiredService<IBiliUserRepository>();

            var up = userRepo.Collection.Find(f => f.UserId == configuration.UserId).FirstOrDefault(stoppingToken);
            if (up == null)
            {
                up = await webUser.FromIdAsync(configuration.UserId);
                if (up == null)
                    throw new Exception($"User not found: {configuration.UserId}.");
            }
            Status.Username = up.Username ?? string.Empty;

            logPrefix = $"[{up.Username}]";

            var replyBlock = new ActionBlock<Func<Task<BiliReply>>>(
                async func =>
                {
                    Interlocked.Increment(ref Status.ReplyRunning);
                    try { await func(); }
                    catch (Exception ex)
                    {
                        logger.LogError("{ex}", ex);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref Status.ReplyRunning);
                        Interlocked.Increment(ref Status.ReplyProcessed);
                    }
                },
                dataflowBlockOptions: new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = configuration.MaxReplyConcurrency });

            var threadBlock = new ActionBlock<BiliDynamic>(
                async dynamic =>
                {
                    try
                    {
                        Interlocked.Increment(ref Status.DynamicRunning);
                        var webReply = host.Services.GetRequiredService<WebBiliReplyRepository>();
                        await foreach (var rawReply in webReply.FromDynamicAsync(dynamic, stoppingToken, force: configuration.Force, requestTimeout: configuration.RequestTimeout))
                        {
                            while (Status.ReplyRunning >= configuration.MaxReplyConcurrency)
                                await Task.Delay(5000);

                            Interlocked.Increment(ref Status.ReplyFound);
                            var postResult = false;
                            do
                            {
                                postResult = replyBlock.Post(() => webReply.FromRawAsync(rawReply, up, dynamic, requestTimeout: configuration.RequestTimeout));
                                if(postResult == false)
                                    await Task.Delay(1000);
                            } while (postResult != true);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("{ex}", ex);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref Status.DynamicRunning);
                        Interlocked.Increment(ref Status.DynamicProcessed);
                        Status.RunningDynamic.TryRemove(dynamic.DynamicId, out _);
                    }
                },
                dataflowBlockOptions: new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism= configuration.MaxDynamicConcurrency });

            var dynamicBlock = new ActionBlock<long>(
                async input =>
                {
                    Running = true;
                    logger.LogInformation("{logPrefix} Start.", logPrefix);
                    var webDynamic = host.Services.GetRequiredService<WebBiliDynamicRepository>();
                    
                    await foreach (var dynamic in webDynamic.FromUserIdAsync(input, 
                        skip: configuration.Skip,
                        maxDepth: configuration.MaxDepth,
                        maxDays: configuration.MaxDays,
                        force: configuration.Force,
                        requestTimeout: configuration.RequestTimeout))
                    {
                        while (Status.DynamicRunning >= configuration.MaxDynamicConcurrency)
                            await Task.Delay(5000);

                        Interlocked.Increment(ref Status.DynamicFound);
                        if (await dynamicRepo.Collection.Find(f => f.DynamicId == dynamic.DynamicId).Limit(1).FirstOrDefaultAsync() is BiliDynamic db)
                        {
                            db.Like = dynamic.Like;
                            db.View = dynamic.View;
                            db.Time = dynamic.Time;
                            await dynamicRepo.UpdateAsync(db);
                            logger.LogDebug("{logPrefix} Update {dynamicId}.", logPrefix, dynamic.DynamicId);
                        }
                        else
                        {
                            await dynamicRepo.InsertAsync(dynamic);
                            logger.LogDebug("{logPrefix} Insert {dynamicId}.", logPrefix, dynamic.DynamicId);
                        }

                        if (Status.RunningDynamic.ContainsKey(dynamic.DynamicId))
                        {
                            Interlocked.Increment(ref Status.DynamicProcessed);
                            continue;
                        }
                        else
                            Status.RunningDynamic.TryAdd(dynamic.DynamicId, dynamic);

                        var postResult = false;
                        do
                        {
                            postResult = threadBlock.Post(dynamic);
                            if (postResult == false)
                                await Task.Delay(1000);
                        } while (postResult != true);
                    }
                    Running = false;
                    logger.LogInformation("{logPrefix} Stop.", logPrefix);
                });

            try
            {

                do
                {
                    if (!Running)
                    {
                        dynamicBlock.Post(configuration.UserId);
                        if (!configuration.Repeat)
                            break;
                    }

                    await Task.Delay(TimeSpan.FromMinutes(configuration.RestartMinutes), cancellationToken: stoppingToken);
                } while (!stoppingToken.IsCancellationRequested);
            }
            catch(TaskCanceledException)
            {
                return;
            }
        }
    }
}
