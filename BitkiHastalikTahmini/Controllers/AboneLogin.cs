using BitkiHastalikTahmini.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BitkiHastalikTahmini.Controllers
{
    public class Abone_LoginController : Controller
    {
        private readonly MongoDbContext _mongoContext;

        public Abone_LoginController(MongoDbContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewData["LoginError"] = "E-posta ve şifre gereklidir";
                return View();
            }

            try
            {
                // Önce e-posta ile kullanıcıyı bul
                var user = await _mongoContext.Users
                    .Find(u => u.Email == email)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    ViewData["LoginError"] = "E-posta veya şifre hatalı.";
                    return View();
                }

                // Şifre kontrolü
                var hashedPassword = HashPassword(password);
                if (user.Password != hashedPassword)
                {
                    ViewData["LoginError"] = "E-posta veya şifre hatalı.";
                    return View();
                }

                // Giriş başarılı
                HttpContext.Session.SetString("UserId", user.Id);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

                return RedirectToAction("Index", "Abone");
            }
            catch (Exception ex)
            {
                ViewData["LoginError"] = $"Bir hata oluştu: {ex.Message}";
                return View();
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }
}