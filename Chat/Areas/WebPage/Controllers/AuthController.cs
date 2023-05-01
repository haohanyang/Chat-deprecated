using Microsoft.AspNetCore.Mvc;

namespace Chat.Areas.WebPage.Controllers;

[Area("WebPage")]
public class AuthController : Controller
{
    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }
}