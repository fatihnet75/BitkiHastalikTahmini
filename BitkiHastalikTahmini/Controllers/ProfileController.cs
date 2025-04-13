using Microsoft.AspNetCore.Mvc;

namespace BitkiHastalikTahmini.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
