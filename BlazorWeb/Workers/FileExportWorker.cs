using BlazorWeb.Data;
using BlazorWeb.Models.Configurations;
using BlazorWeb.Models.Web;
using BlazorWeb.Repositories;
using Microsoft.Extensions.Azure;
using MongoDB.Bson;
using Mzr.Share.Models.Bilibili;
using SharpCompress.Archives.Zip;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace BlazorWeb.Workers
{
    public class FileExportWorker : BackgroundService
    {
        private readonly WebConfiguration configuration;
        private readonly IServiceProvider serviceProvider;
        private readonly StatusService status;

        private readonly ILogger logger;

        public FileExportWorker(WebConfiguration configuration, ILogger<FileExportWorker> logger, IServiceProvider serviceProvider, StatusService statusService)
        {
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            status = statusService;
        }

        public async Task Dispatch(CancellationToken cancellation = default)
        {
            while (!cancellation.IsCancellationRequested)
            {
                if (status.ExportRunningDict.Count < configuration.FileExportConcurrency && status.ExportWaitingQueue.TryDequeue(out WebFile? file))
                {
                    switch (file.Function)
                    {
                        case WebFileFunction.ReplyExport:
                            await ExportReply(file, cancellation);
                            break;
                        default:
                            logger.LogError("Unknown WebFileFunction {function} for Id {id}.", file.Function, file.Id);
                            break;
                    }
                }
                else
                {
                    await Task.Delay(5000, cancellationToken: cancellation);
                }
            }
        }

        public async Task ExportReply(WebFile file, CancellationToken cancellation = default)
        {
            status.ExportRunningDict.TryAdd(file.Id, file);
            logger.LogInformation("Add export job, queue: {len}.", status.ExportRunningDict.Count);
            var task = new Task(async () => await ExportReplyImpl(file, cancellation: cancellation));
            task.Start();
            await task.WaitAsync(cancellation);
            
        }

        public async Task ExportReplyImpl(WebFile file, CancellationToken cancellation = default)
        {
            using var scope = serviceProvider.CreateScope();
            var fileService = scope.ServiceProvider.GetRequiredService<WebFileService>();
            var replyService = scope.ServiceProvider.GetRequiredService<BiliReplyService>();

            file.Status = WebFileStatusEnum.Runnning;
            await fileService.UpdateAsync(file);

            try
            {
                using var uploadStream = await fileService.OpenUploadStreamAsync(file, cancellation: cancellation);
                using var archive = new System.IO.Compression.ZipArchive(uploadStream, ZipArchiveMode.Create, true);
                var jsonFile = archive.CreateEntry("data.json");
                using var stream = jsonFile.Open();

                var skip = (int?)file.Parameters.GetValueOrDefault("page", null) ?? 0;
                var size = (int?)file.Parameters.GetValueOrDefault("size", null) ?? 100;
                var cursor = replyService.ExportAsync(
                    userId: (long?)file.Parameters.GetValueOrDefault("userId", null),
                    threadId: (long?)file.Parameters.GetValueOrDefault("threadId", null),
                    upId: (long?)file.Parameters.GetValueOrDefault("upId", null),
                    dialogId: (long?)file.Parameters.GetValueOrDefault("dialogId", null),
                    skip: skip,
                    size: size,
                    sort: (string?)file.Parameters.GetValueOrDefault("sort", null) ?? "-time",
                    contentQuery: (string?)file.Parameters.GetValueOrDefault("contentQuery", null),
                    startTime: (DateTime?)file.Parameters.GetValueOrDefault("startTime", null),
                    endTime: (DateTime?)file.Parameters.GetValueOrDefault("endTime", null),
                    root: (long?)file.Parameters.GetValueOrDefault("root", null),
                    parent: (long?)file.Parameters.GetValueOrDefault("parent", null),
                    ipQuery: (string?)file.Parameters.GetValueOrDefault("ipQuery", null),
                    cancellation: cancellation);

                var count = 0;
                var writeStart = true;

                await stream.WriteAsync(Encoding.UTF8.GetBytes("["), cancellation);
                await foreach (var item in cursor)
                {
                    if (writeStart)
                        writeStart = false;
                    else
                        await stream.WriteAsync(Encoding.UTF8.GetBytes(",\n"), cancellation);

                    await JsonSerializer.SerializeAsync(stream, item, options: new()
                    {
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                        WriteIndented = true,
                    }, cancellationToken: cancellation);
                    count++;
                    if (count % 1000 == 0)
                    {
                        file.Progress = (double)count / size;
                        await fileService.UpdateAsync(file);
                    }
                }
                await stream.WriteAsync(Encoding.UTF8.GetBytes("]"), cancellation);

                file.Progress = 1;
                file.Status = WebFileStatusEnum.Success;
                await fileService.UpdateAsync(file);
            }
            catch (Exception ex)
            {
                file.Status = WebFileStatusEnum.Failure;
                file.Error = ex.Message;
                await fileService.UpdateAsync(file);
                logger.LogError("Reply export failure: {ex}", ex.Message);
            }
            finally
            {
                status.ExportRunningDict.TryRemove(file.Id, out _);
                logger.LogInformation("Complete export job, queue: {len}.", status.ExportRunningDict.Count);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            var task = new Task(async () => await Dispatch(stoppingToken));
            task.Start();
            await task.WaitAsync(stoppingToken);
        }
    }
}
