using Microsoft.AspNetCore.Mvc;

namespace BitkiHastalikTahmini.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
