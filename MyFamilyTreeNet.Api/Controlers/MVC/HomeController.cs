using Microsoft.AspNetCore.Mvc;

namespace MyFamilyTreeNet.Api.Controlers.MVC
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Welcome to MyFamilyTreeNet";
            return View();
        }
    }
}