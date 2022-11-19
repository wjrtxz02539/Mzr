using BlazorWeb.Data;
using BlazorWeb.Models.Configurations;
using BlazorWeb.Workers;
using MongoDB.Driver;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MudBlazor.Services;
using Mzr.Share.Interfaces.Bilibili;
using Mzr.Share.Repositories.Bilibili;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using BlazorWeb.Repositories;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

var builder = WebApplication.CreateBuilder(args);

// Auth relate
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
});

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
});
builder.Services.AddServerSideBlazor().AddMicrosoftIdentityConsentHandler();
builder.Services.AddMudServices();
builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.IncludeScopes = true;
    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss]";
});

builder.Services.AddSingleton<WebConfiguration>((provider) =>
{
    var configRoot = provider.GetRequiredService<IConfiguration>();
    var webConfig = configRoot.GetRequiredSection("WebConfiguration").Get<WebConfiguration>();
    if (webConfig is null)
    {
        throw new NullReferenceException(nameof(webConfig));
    }
    return webConfig;
});

builder.Services.AddSingleton<IMongoDatabase>((provider) =>
{
    var config = provider.GetRequiredService<WebConfiguration>();
    var client = new MongoClient(config.Database.Url);
    return client.GetDatabase(config.Database.DatabaseName);
});

builder.Services.AddScoped<IBiliUserRepository, BiliUserRepository>()
                .AddScoped<IBiliDynamicRepository, BiliDynamicRepository>()
                .AddScoped<IBiliReplyRepository, BiliReplyRepository>()
                .AddScoped<IBiliDynamicRunRecordRepository, BiliDynamicRunRecordRepository>()
                .AddScoped<WebUserRepository>()
                .AddScoped<WebLogRepository>()
                .AddScoped<WebFileRepository>()
                .AddSingleton<StatusService>()
                .AddScoped<BiliReplyService>()
                .AddScoped<BiliUserService>()
                .AddScoped<BiliDynamicService>()
                .AddScoped<WebUserService>()
                .AddScoped<WebFileService>()
                .AddHostedService<FileExportWorker>()
                .AddHostedService<StatusUpdateWorker>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
