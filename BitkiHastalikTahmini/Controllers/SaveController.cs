using BitkiHastalikTahmini.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace BitkiHastalikTahmini.Controllers
{
    public class SaveController : Controller
    {
        private readonly MongoDbContext _mongoContext;
        private readonly EmailService _emailService;

        public SaveController(MongoDbContext mongoContext, EmailService emailService)
        {
            _mongoContext = mongoContext;
            _emailService = emailService;
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
                // E-posta kontrolü
                var existingUser = await _mongoContext.Users
                    .Find(u => u.Email == user.Email)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Bu e-posta zaten kayıtlı");
                    return View(user);
                }

                // Doğrulama kodu oluşturma
                string verificationCode = GenerateVerificationCode();
                
                // MongoDB için id ataması
                user.Id = ObjectId.GenerateNewId().ToString();
                
                // Kullanıcı ID ataması
                var lastUser = await _mongoContext.Users
                    .Find(_ => true)
                    .SortByDescending(u => u.UserId)
                    .FirstOrDefaultAsync();

                user.UserId = (lastUser?.UserId ?? 0) + 1;

                // Şifre hash'leme
                user.Password = HashPassword(user.Password);
                Console.WriteLine($"Kayıt için oluşturulan hash: {user.Password}");
                Console.WriteLine($"Şifre hash uzunluğu: {user.Password.Length}");
                
                // Doğrulama bilgilerini kullanıcı nesnesine ekle
                user.VerificationCode = verificationCode;
                user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10);
                user.IsEmailVerified = false;
                
                // Kullanıcı bilgilerini Session'a JSON olarak kaydedelim
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                var userJson = JsonSerializer.Serialize(user, options);
                HttpContext.Session.SetString("PendingUser", userJson);
                
                // Doğrulama e-postası gönder
                await _emailService.SendVerificationEmail(user.Email, verificationCode);
                
                // Doğrulama kodunu girmesi için bilgileri TempData'ya aktar
                TempData["VerificationPending"] = true;
                TempData["Email"] = user.Email;
                
                return View(user);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Kayıt sırasında bir hata oluştu: {ex.Message}");
                return View(user);
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(string verificationCode)
        {
            if (string.IsNullOrEmpty(verificationCode))
            {
                ModelState.AddModelError("", "Doğrulama kodu gereklidir");
                return View("Index");
            }
            
            // Session'dan kullanıcı bilgilerini al
            var pendingUserJson = HttpContext.Session.GetString("PendingUser");
            if (string.IsNullOrEmpty(pendingUserJson))
            {
                return RedirectToAction("Index");
            }
            
            // JSON'u User nesnesine dönüştür
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            User user = JsonSerializer.Deserialize<User>(pendingUserJson, options);
            
            if (user == null || string.IsNullOrEmpty(user.Id) || string.IsNullOrEmpty(user.VerificationCode))
            {
                ModelState.AddModelError("", "Kullanıcı bilgileri eksik veya geçersiz");
                return View("Index");
            }
            
            // Doğrulama kodunu kontrol et
            if (user.VerificationCode != verificationCode)
            {
                TempData["VerificationPending"] = true;
                TempData["Email"] = user.Email;
                ModelState.AddModelError("", "Geçersiz doğrulama kodu");
                return View("Index");
            }
            
            // Doğrulama kodu süresini kontrol et
            if (DateTime.UtcNow > user.VerificationCodeExpiry)
            {
                TempData["VerificationPending"] = true;
                TempData["Email"] = user.Email;
                ModelState.AddModelError("", "Doğrulama kodunun süresi dolmuş");
                return View("Index");
            }
            
            // Doğrulama başarılı, kullanıcı durumunu güncelle
            user.IsEmailVerified = true;
            
            try
            {
                // ID ve diğer zorunlu alanların dolu olduğundan emin ol
                if (string.IsNullOrEmpty(user.Id))
                {
                    user.Id = ObjectId.GenerateNewId().ToString();
                }
                
                if (string.IsNullOrEmpty(user.VerificationCode))
                {
                    user.VerificationCode = verificationCode;
                }
                
                // Kullanıcıyı veritabanına ekleme
                Console.WriteLine("Veritabanına kaydedilen kullanıcı bilgileri:");
                Console.WriteLine($"ID: {user.Id}");
                Console.WriteLine($"Email: {user.Email}");
                Console.WriteLine($"Password Hash: {user.Password}");
                Console.WriteLine($"Password Hash Uzunluğu: {user.Password.Length}");
                Console.WriteLine($"IsEmailVerified: {user.IsEmailVerified}");
                
                await _mongoContext.Users.InsertOneAsync(user);
                
                // Session'daki geçici kullanıcı bilgisini temizle
                HttpContext.Session.Remove("PendingUser");

                TempData["SuccessMessage"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction("Index", "Abone_Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Kullanıcı kaydı sırasında bir hata oluştu: {ex.Message}");
                return View("Index");
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationCode()
        {
            // Session'dan kullanıcı bilgilerini al
            var pendingUserJson = HttpContext.Session.GetString("PendingUser");
            if (string.IsNullOrEmpty(pendingUserJson))
            {
                return RedirectToAction("Index");
            }
            
            // JSON'u User nesnesine dönüştür
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            User user = JsonSerializer.Deserialize<User>(pendingUserJson, options);
            
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return RedirectToAction("Index");
            }
            
            // Yeni doğrulama kodu oluştur
            string newVerificationCode = GenerateVerificationCode();
            user.VerificationCode = newVerificationCode;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10);
            
            // Güncellenmiş kullanıcıyı session'a kaydet
            HttpContext.Session.SetString("PendingUser", JsonSerializer.Serialize(user, options));
            
            // Yeni doğrulama e-postası gönder
            await _emailService.SendVerificationEmail(user.Email, newVerificationCode);
            
            // Doğrulama kodunu girmesi için bilgileri TempData'ya aktar
            TempData["VerificationPending"] = true;
            TempData["Email"] = user.Email;
            
            TempData["SuccessMessage"] = "Yeni doğrulama kodu e-posta adresinize gönderildi.";
            return View("Index");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
        
        private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}