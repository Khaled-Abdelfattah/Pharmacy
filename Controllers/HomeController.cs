using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PharmacySystem.Controllers;

public class HomeController : Controller
{
    public IActionResult Error()
    {
        return View();
    }
}
