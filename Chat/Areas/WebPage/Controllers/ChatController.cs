using Microsoft.AspNetCore.Mvc;

namespace Chat.Areas.WebPage.Controllers;

[Area("WebPage")]
public class ChatController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}