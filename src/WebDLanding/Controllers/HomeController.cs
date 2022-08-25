using Microsoft.AspNetCore.Mvc;

namespace WebDLanding.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAppModelFinder _finder;

    public HomeController(IAppModelFinder finder, ILogger<HomeController> logger)
    {
        _finder = finder;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var hostName = Request.Host.Value;

        var vm = await _finder.FindByEntryHostAsync(hostName);

        if (vm is null)
        {
            return View("Error");
        }

        _logger.LogInformation("Request received for host: {host}. Serving App/System: {db}", hostName, vm.DatabaseName);

        return View(vm);
    }

    public async Task<IActionResult> Logoff()
    {
        // find the database from the referer [sic]
        var referrer = new Uri(Request.Headers.Referer.ToString());

        var vm = await _finder.FindByReferrerUriAsync(referrer);

        if (vm is null)
        {
            return View("Error");
        }

        // you'd think we want to redirect here, but we need to issue a javascript
        // `top.location.href = ` to escape the iframe inception we create for back button support/suppression
        return View(vm);
    }
}
