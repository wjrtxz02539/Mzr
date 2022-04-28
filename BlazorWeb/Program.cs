using BlazorWeb.Data;
using BlazorWeb.Models;
using BlazorWeb.Models.Configurations;
using MongoDB.Driver;
using MudBlazor.Services;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Repositories.Bilibili;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();


builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.IncludeScopes = true;
    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss]";
});

builder.Services.AddSingleton<WebConfiguration>((provider) =>
{
    var configRoot = provider.GetRequiredService<IConfiguration>();
    return configRoot.GetRequiredSection("WebConfiguration").Get<WebConfiguration>();
});

builder.Services.AddSingleton<IMongoDatabase>((provider) =>
{
    var config = provider.GetRequiredService<WebConfiguration>();
    var client = new MongoClient(config.Database.Url);
    return client.GetDatabase(config.Database.DatabaseName);
});

builder.Services.AddSingleton<IBiliUserRepository, BiliUserRepository>()
                .AddSingleton<IBiliDynamicRepository, BiliDynamicRepository>()
                .AddSingleton<IBiliReplyRepository, BiliReplyRepository>()
                .AddSingleton<IBiliDynamicRunRecordRepository, BiliDynamicRunRecordRepository>()
                .AddSingleton<GlobalStats>()
                .AddSingleton<BiliReplyService>()
                .AddSingleton<BiliUserService>()
                .AddSingleton<BiliDynamicService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
