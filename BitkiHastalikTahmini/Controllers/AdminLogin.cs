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
            // Sadece belirli e-posta ve şifre ile girişe izin ver
            if (username == "fatihgurbuz7536@gmail.com" && password == "123456")
            {
                // Giriş başarılı, admin paneline yönlendir
                return RedirectToAction("Index", "Admin");

            }
            else
            {
                // Hatalı giriş, ViewData ile hata mesajı gönder
                ViewData["LoginError"] = "Kullanıcı adı veya şifre hatalı.";
                return View();
            }
        }

        public IActionResult Panel()
        {
            return View(); // Admin Paneli sayfası
        }
    }
}