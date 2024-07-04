using Microsoft.AspNetCore.Authorization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Liquid;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace DotCat.OrchardCore.Liquid.TryItEditor.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IStringLocalizer S;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ILiquidTemplateManager liquidTemplateManager,
            IStringLocalizer<AdminController> localizer,
            HtmlEncoder htmlEncoder,
            IAuthorizationService authorizationService,
            ILogger<AdminController> logger
        )
        {
            _liquidTemplateManager = liquidTemplateManager;
            S = localizer;
            _htmlEncoder = htmlEncoder;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (!await _authorizationService.AuthorizeAsync(User, Permissions.UseTryItEditor))
                {
                    return Forbid();
                }

                return View();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(string liquid)
        {
            string result = null; 
            try
            {
                if (!await _authorizationService.AuthorizeAsync(User, Permissions.UseTryItEditor))
                {
                    return Forbid();
                }

                if (!string.IsNullOrWhiteSpace(liquid) && !_liquidTemplateManager.Validate(liquid, out var errors))
                {
                    result = S["The Liquid input doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]?.Value;
                }
                else
                {
                    result = await _liquidTemplateManager.RenderStringAsync(liquid, _htmlEncoder, null, null);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                result = e.ToString();
            }

            return PartialView("_Output", result);
        }
    }
}