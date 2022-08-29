using NLog.Web;
using WebDLanding;

var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddScoped<IAppModelFinder, AppSettingsAppModelFinder>();

    var siteSettings = builder.Configuration.GetSection("WebDLandingConfig").Get<WebDLanding.Models.SiteConfiguration>();
    builder.Services.AddSingleton(siteSettings);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    // show index.html from wwwroot
    app.UseDefaultFiles();

    app.UseStaticFiles();

    app.UseRouting();

    app.MapGet("/set-vars.js", async (IAppModelFinder finder, HttpContext context) =>
    {
        var model = await finder.FindByEntryHostAsync(context.Request.Host.Value);

        var jsToWrite = $"let singleServerMode = {siteSettings.SingleServerMode.ToString().ToLower()};";
        jsToWrite += $"let uri = '{model.WebDirectFullUri}';";
        jsToWrite += $"let backEventName = '{Globals.BackButtonPostEventName}';";
        var homeUri = siteSettings.SingleServerMode == true 
            ? $"{model.EntryHostScheme}://{model.EntryHostName}" 
            : $"{siteSettings.WebdLandingMainUrl}";
        jsToWrite += $"let homeUri = '{homeUri}';";

        context.Response.ContentType = "application/javascript; charset=utf-8";
        await context.Response.WriteAsync(jsToWrite);
    });

    app.MapGet("/logoff.js", async (IAppModelFinder finder, HttpContext context) =>
    {
        var model = await finder.FindByEntryHostAsync(context.Request.Host.Value);
        context.Response.ContentType = "application/javascript; charset=utf-8";
        await context.Response.WriteAsync($"top.location.href = '{model.EntryHostUri}';");
    });

    app.Run();
}
catch (Exception exception)
{
    //NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}