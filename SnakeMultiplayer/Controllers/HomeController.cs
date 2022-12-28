using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

using SnakeMultiplayer.Models;

namespace SnakeMultiplayer.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
    [HttpPost]
    public IActionResult Error(string errorMessage =
        "An error has occured.\n We are already taking action to prevent this error from happening.")
    {
        ViewData["ErrorMessage"] = errorMessage;
        return View("Views/Home/Index.cshtml");
    }

    public IActionResult About()
    {
        ViewData["Message"] = "Your application description page.";

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}