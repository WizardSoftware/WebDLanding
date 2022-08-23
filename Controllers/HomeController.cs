using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebDLanding.Models;

namespace WebDLanding.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    readonly List<AppModel> VMs = new()
    {
            new AppModel
            {
                ServerName = "lade.wizardsoftware.net",
                DatabaseName = "PJT_SYSTEM_SSODEV",
                HostName = "https://localhost:7026"
            },
            new AppModel
            {
                ServerName = "lade.wizardsoftware.net",
                DatabaseName = "PJT_SYSTEM_SSODEV",
                HostName = "https://lade.wizardsoftware.net"
            },
            new AppModel
            {
                ServerName = "lade.wizardsoftware.net",
                DatabaseName = "PJT_SYSTEM_SSODEV",
                HostName = "https://webdlanding.wizardsoftware.net"
            },
        };

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var hostName = Request.Host.Value;

        var vm = VMs.SingleOrDefault(h => h.HostName.Contains(hostName));

        if (vm is null)
        {
            return View("Error");
        }

        _logger.LogInformation("Request received for host: {host}. Serving App/System: {db}", hostName, vm.DatabaseName);

        return View(vm);
    }

    public IActionResult BackButton()
    {
        return View();
    }

    public IActionResult Logoff()
    {
        // find the database from the referer [sic]
        var referrer = new Uri(Request.Headers.Referer.ToString());
        var localReferrerPath = referrer.LocalPath.ToString();
        var startSub = localReferrerPath.LastIndexOf("/") + 1;
        var subLength = localReferrerPath.Length - startSub;
        var database = localReferrerPath.Substring(startSub, subLength);

        // match our database to a host entry
        var vm = VMs.SingleOrDefault(h => h.DatabaseName.Equals(database, StringComparison.OrdinalIgnoreCase));

        if (vm is null)
        {
            return View("Error");
        }

        // you'd think we want to redirect here, but we need to issue a javascript
        // `top.location.href = ` to escape the iframe inception we create for back button support/suppression
        return View(vm);
    }
}
