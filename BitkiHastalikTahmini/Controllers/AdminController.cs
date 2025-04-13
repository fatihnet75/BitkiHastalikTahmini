using Microsoft.AspNetCore.Mvc;

namespace BitkiHastalikTahmini.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
