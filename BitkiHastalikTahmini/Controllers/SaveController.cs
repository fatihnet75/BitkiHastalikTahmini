using BitkiHastalikTahmini.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
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
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            try
            {
                var existingUser = await _mongoContext.Users
                    .Find(u => u.Email == user.Email)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Bu e-posta zaten kayıtlı");
                    return View(user);
                }

                // Ensure the ID is set
                if (string.IsNullOrEmpty(user.Id))
                {
                    user.Id = ObjectId.GenerateNewId().ToString();
                }

                // Determine the next UserId (if you need incremental UserIds)
                var lastUser = await _mongoContext.Users
                    .Find(_ => true)
                    .SortByDescending(u => u.UserId)
                    .FirstOrDefaultAsync();

                user.UserId = (lastUser?.UserId ?? 0) + 1;
                user.Password = HashPassword(user.Password);

                await _mongoContext.Users.InsertOneAsync(user);
                TempData["SuccessMessage"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction("Abone_Login", "Save");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Kayıt sırasında bir hata oluştu: {ex.Message}");
                return View(user);
            }
        }

        // GET: /Save/Abone_Login (Giriş sayfası)
        [HttpGet]
        public IActionResult Abone_Login()
        {
            return View();
        }

        // POST: /Save/Abone_Login (Giriş işlemi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abone_Login(User model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("", "E-posta ve şifre gereklidir");
                return View(model);
            }

            try
            {
                var hashedPassword = HashPassword(model.Password);
                var user = await _mongoContext.Users
                    .Find(u => u.Email == model.Email && u.Password == hashedPassword)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    TempData["ErrorMessage"] = "E-posta veya şifre hatalı";
                    return View(model);
                }

                // Oturum açma işlemleri burada yapılabilir
                // Örneğin: Session veya Cookie kullanarak kullanıcı bilgilerini saklama
                return RedirectToAction("Index", "User"); // Kullanıcı anasayfasına yönlendir
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Giriş sırasında bir hata oluştu: {ex.Message}";
                return View(model);
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