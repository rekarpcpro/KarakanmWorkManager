using Microsoft.AspNetCore.Mvc;

namespace KWM.UI.Controllers;
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Task");
    }
    public IActionResult Privacy()
    {
        return View();
    }
}
