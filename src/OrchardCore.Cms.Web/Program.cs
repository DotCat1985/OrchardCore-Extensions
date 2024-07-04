using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Logging;
using Serilog;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

// imposta il logging
builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration));

// attiva response caching
builder.Services.AddResponseCaching();

// rimuove l'header Kestrel dalle response e request HTTP
builder.WebHost.UseKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});
builder.WebHost.UseIIS();

// imposta il nome del cookie TempData
builder.Services.Configure<CookieTempDataProviderOptions>(options => options.Cookie.Name = "MyTempDataCookie");
builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.SameSite = SameSiteMode.None;
});

// rimuove i cookie per la localizzazione
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var requestProvider = options.RequestCultureProviders.OfType<AcceptLanguageHeaderRequestCultureProvider>()
        .First();
    options.RequestCultureProviders.Remove(requestProvider);
});

// attiva OrchardCore
builder.Services.AddOrchardCms()
    .AddSetupFeatures("OrchardCore.AutoSetup")
    .Configure(app =>
    {
        app.UseAuthentication();
        app.UseAuthorization();
    });

// If an instance of OrchardCoreBuilder exists reuse it,
// so we can call AddOrchardCore several times.
if (builder.Services.LastOrDefault(d => d.ServiceType == typeof(OrchardCoreBuilder))?.ImplementationInstance is OrchardCoreBuilder orchardCoreBuilder)
{
    AddAntiForgery(orchardCoreBuilder, "Antiforgery");
}

var app = builder.Build();

Log.Information($"EnvironmentName: {app.Environment.EnvironmentName}");
Log.Information($"ApplicationName: {app.Environment.ApplicationName}");
Log.Information($"ContentRootPath: {app.Environment.ContentRootPath}");

//IMPOSTA GLI HEADER DEI RESPONSE HTTP
// app.Use(async (context, next) =>
// {
//     context.Response.OnStarting(() =>
//     {
//         string[] headersToRemove =
//         {
//              ""
//         };
//         foreach (var item in headersToRemove)
//         {
//             if (context.Response.Headers.ContainsKey(item))
//             {
//                 context.Response.Headers.Remove(item);
//             }
//         }
//         return Task.FromResult(0);
//     });
//     await next();
// });

if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

var requestLocalizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
if (requestLocalizationOptions?.Value != null)
{
    app.UseRequestLocalization();
}

app.UseOrchardCore(x => x.UseSerilogTenantNameLogging());
app.UsePoweredByOrchardCore(false);
app.Run();


static void AddAntiForgery(OrchardCoreBuilder builder, string cookiePrefixName)
{
    builder.ApplicationServices.AddAntiforgery();

    builder.ConfigureServices((services, serviceProvider) =>
    {
        var settings = serviceProvider.GetRequiredService<ShellSettings>();
        var environment = serviceProvider.GetRequiredService<IHostEnvironment>();

        var cookieName = $"{cookiePrefixName}_{HttpUtility.UrlEncode(settings.Name + environment.ContentRootPath)}";

        // If uninitialized, we use the host services.
        if (settings.State == TenantState.Uninitialized)
        {
            // And delete a cookie that may have been created by another instance.
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

            // Use case when creating a container without ambient context.
            if (httpContextAccessor.HttpContext == null)
            {
                return;
            }

            // Use case when creating a container in a deferred task.
            if (httpContextAccessor.HttpContext.Response.HasStarted)
            {
                return;
            }

            httpContextAccessor.HttpContext.Response.Cookies.Delete(cookieName);

            return;
        }

        // Re-register the antiforgery  services to be tenant-aware.
        services.AddAntiforgery(options =>
        {
            options.Cookie.Name = cookieName;

            // Don't set the cookie builder 'Path' so that it uses the 'IAuthenticationFeature' value
            // set by the pipeline and comming from the request 'PathBase' which already ends with the
            // tenant prefix but may also start by a path related e.g to a virtual folder.
        });
    });
}