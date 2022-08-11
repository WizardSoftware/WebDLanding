﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebDLanding.Models;

namespace WebDLanding.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var hostName = Request.Host.Value;

        var vms = new List<AppModel> {
            new AppModel
            {
                DatabaseName = "PJT_SYSTEM_SSODEV",
                HostName = "https://localhost:7026"
            },
            new AppModel
            {
                DatabaseName = "PJT_SYSTEM_SSODEV",
                HostName = "https://lade.wizardsoftware.net"
            },
        };

        var vm = vms.SingleOrDefault(h => h.HostName.Contains(hostName));

        if (vm is null)
        {
            return View("Error");
        }

        _logger.LogInformation("Request received for host: {host}. Serving App/System: {db}", hostName, vm.DatabaseName);

        return View(vm);
    }
}
