using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BitkiHastalikTahmini.Controllers
{
    public class LogoutController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // Session'ı temizle
            HttpContext.Session.Clear();

            // Anasayfaya yönlendir
            return RedirectToAction("Index", "User");
        }
    }
} 