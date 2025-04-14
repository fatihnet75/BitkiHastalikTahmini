using BitkiHastalikTahmini.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BitkiHastalikTahmini.Controllers
{
    public class SaveController : Controller
    {
        private readonly MongoDbContext _mongoContext;

        public SaveController(MongoDbContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        // GET: /Save/Index (Kayıt sayfası)
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Save/Index (Kayıt işlemi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(User user)
        {
            try
            {
                // MongoDB için id ataması, bu şekilde "The Id field is required" hatası çözülür
                user.Id = ObjectId.GenerateNewId().ToString();

                // E-posta kontrolü
                var existingUser = await _mongoContext.Users
                    .Find(u => u.Email == user.Email)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Bu e-posta zaten kayıtlı");
                    return View(user);
                }

                // Kullanıcı ID ataması
                var lastUser = await _mongoContext.Users
                    .Find(_ => true)
                    .SortByDescending(u => u.UserId)
                    .FirstOrDefaultAsync();

                user.UserId = (lastUser?.UserId ?? 0) + 1;

                // Şifre hash'leme
                user.Password = HashPassword(user.Password);

                // Kullanıcıyı veritabanına ekleme
                await _mongoContext.Users.InsertOneAsync(user);

                TempData["SuccessMessage"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction("Index", "Abone_Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Kayıt sırasında bir hata oluştu: {ex.Message}");
                return View(user);
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