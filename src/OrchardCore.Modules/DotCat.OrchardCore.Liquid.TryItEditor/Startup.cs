using DotCat.OrchardCore.Liquid.TryItEditor.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Liquid;
using OrchardCore.Liquid.Services;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;
using System;

namespace DotCat.OrchardCore.Liquid.TryItEditor
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;
        private readonly ILogger<Startup> _logger;

        public Startup(
            IOptions<AdminOptions> adminOptions,
            ILogger<Startup> logger
        )
        {
            _adminOptions = adminOptions.Value;
            _logger = logger;
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            try
            {
                var adminControllerName = typeof(AdminController).ControllerName();

                routes.MapAreaControllerRoute(
                    name: "TryItEditorIndex",
                    areaName: TryItEditorConstants.Features.TryItEditor,
                    pattern: _adminOptions.AdminUrlPrefix + "/TryItEditor",
                    defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
                );

                app.UseMiddleware<ScriptsMiddleware>();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddScoped<IPermissionProvider, Permissions>();
                services.AddScoped<ILiquidTemplateManager, LiquidTemplateManager>();
                services.AddScoped<INavigationProvider, AdminMenu>();
                services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }
    }
}
