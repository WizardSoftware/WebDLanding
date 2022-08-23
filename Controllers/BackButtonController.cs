using Microsoft.AspNetCore.Mvc;

namespace WebDLanding.Controllers;

/// <summary>
/// Back Button Support Controller
/// </summary>
public class BackButtonController : Controller
{
    private readonly ILogger<BackButtonController> _logger;

    public BackButtonController(ILogger<BackButtonController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index() => View();
}
