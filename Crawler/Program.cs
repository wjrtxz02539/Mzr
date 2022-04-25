// See https://aka.ms/new-console-template for more information

using Mzr.Share.Utils;
using MzrConfiguration = Mzr.Share.Configuration.Configuration;
using Mzr.Share.Repositories.Bilibili;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Service.Crawler.Worker;
using Mzr.Share.Repositories.Bilibili.Web;
using Mzr.Share.Interfaces.Bilibili.Raw;
using Mzr.Share.Repositories.Bilibili.Raw;
using Mzr.Service.Crawler.Utils;
using Mzr.Share.Interfaces;
using Microsoft.AspNetCore.Builder;
using System.Net;
using MongoDB.Driver;

ServicePointManager.ReusePort = true;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.IncludeScopes = true;
    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss]";
});

builder.Services.AddSingleton<MzrConfiguration>()
                .AddSingleton<CrawlerConfiguration>()
                .AddSingleton((provider) =>
                {
                    var config = provider.GetRequiredService<MzrConfiguration>();
                    var client = new MongoClient(config.Database.Url);
                    return client.GetDatabase(config.Database.DatabaseName);
                })
                .AddSingleton<IBiliUserRepository, BiliUserRepository>()
                .AddSingleton<IBiliDynamicRepository, BiliDynamicRepository>()
                .AddSingleton<IBiliReplyRepository, BiliReplyRepository>()
                .AddSingleton<IBiliDynamicRunRecordRepository, BiliDynamicRunRecordRepository>()
                .AddTransient<IRawBiliDynamicRepository, RawBiliDynamicRepository>()
                .AddTransient<IRawBiliUserRepository, RawBiliUserRepository>()
                .AddSingleton<IProxyPool, SelfProxyPool>()
                .AddTransient<Request>()
                .AddTransient<WebBiliDynamicRepository>()
                .AddTransient<WebBiliReplyRepository>()
                .AddTransient<WebBiliUserRepository>()
                .AddSingleton<WorkerStats>()
                .AddHostedService<MainWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

