using MongoDB.Driver;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Repositories.Bilibili;
using Mzr.Web.Models;
using Mzr.Web.Models.Configurations;
using Mzr.Web.Services;
using Mzr.Web.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddServerSideBlazor();

// For Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddHostedService<MonitorUpdateWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
