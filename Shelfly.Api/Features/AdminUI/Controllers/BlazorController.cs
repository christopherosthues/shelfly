using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Constants;

namespace Shelfly.Api.Features.AdminUI.Controllers;

/// <summary>
/// Controller to serve the Blazor admin UI.
/// </summary>
[Authorize(Policy = AuthorizationSchemes.AdminOnly)]
public class BlazorController : Controller
{
    /// <summary>
    /// Serves the main HTML page for Blazor Server.
    /// </summary>
    public IActionResult Index() => View("~/Features/AdminUI/wwwroot/index.html");
}
