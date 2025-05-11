using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BitkiHastalikTahmini.Controllers
{
    public class AdminLogin : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            // Sabit kullanıcı adı ve şifre ile kontrol
            if (username == "fatihgurbuz7536@gmail.com" && password == "123456")
            {
                // Session ayarla
                HttpContext.Session.SetString("AdminId", "1");
                HttpContext.Session.SetString("AdminEmail", username);
                HttpContext.Session.SetString("AdminName", "Admin");
                
                // Admin sayfasına yönlendir
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                ViewData["LoginError"] = "Kullanıcı adı veya şifre hatalı.";
                return View();
            }
        }
    }
}