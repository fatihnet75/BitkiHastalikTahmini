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
        private readonly EmailService _emailService;

        public Abone_LoginController(MongoDbContext mongoContext, EmailService emailService)
        {
            _mongoContext = mongoContext;
            _emailService = emailService;
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
                    ViewData["LoginError"] = "E-posta veya şifre hatalı. (Kullanıcı bulunamadı)";
                    return View();
                }

                // E-posta doğrulaması zorunlu
                if (!user.IsEmailVerified)
                {
                    ViewData["LoginError"] = "Lütfen önce email adresinizi doğrulayın. Doğrulama kodunu içeren e-posta size gönderildi.";
                    return View();
                }

                // Şifre kontrolü - Debug için hashleri yazdıracağız
                Console.WriteLine("Login İşlemi Debug Bilgileri:");
                Console.WriteLine($"Kullanıcı ID: {user.Id}");
                Console.WriteLine($"Email: {email}");
                Console.WriteLine($"Girilen Şifre: {password}");
                var hashedPassword = HashPassword(password);
                Console.WriteLine($"Giriş için hesaplanan hash: {hashedPassword}");
                Console.WriteLine($"Veri tabanındaki hash: {user.Password}");
                Console.WriteLine($"Hash uzunlukları - Girilen: {hashedPassword.Length}, DB: {user.Password.Length}");
                
                if (user.Password != hashedPassword)
                {
                    ViewData["LoginError"] = "E-posta veya şifre hatalı. (Şifre eşleşmedi)";
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
        
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewData["ErrorMessage"] = "E-posta adresi gereklidir";
                return View();
            }

            try
            {
                // E-posta ile kullanıcıyı bul
                var user = await _mongoContext.Users
                    .Find(u => u.Email == email)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    // Güvenlik nedeniyle kullanıcı bulunamasa bile aynı mesajı veriyoruz
                    ViewData["SuccessMessage"] = "Şifre sıfırlama talimatları e-posta adresinize gönderildi.";
                    return View();
                }

                // Rastgele 6 haneli doğrulama kodu oluştur
                var verificationCode = new Random().Next(100000, 999999).ToString();
                
                // Doğrulama kodunu ve son kullanma süresini kullanıcı bilgilerine ekle
                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                var update = Builders<User>.Update
                    .Set(u => u.VerificationCode, verificationCode)
                    .Set(u => u.VerificationCodeExpiry, DateTime.UtcNow.AddMinutes(10));
                
                await _mongoContext.Users.UpdateOneAsync(filter, update);

                // Şifre sıfırlama e-postası gönder
                await _emailService.SendPasswordResetEmail(email, verificationCode);

                // Şifre sıfırlama için e-posta ile birlikte resettoken sayfasına yönlendir
                TempData["ResetEmail"] = email;
                return RedirectToAction("ResetToken");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}";
                return View();
            }
        }
        
        [HttpGet]
        public IActionResult ResetToken()
        {
            // TempData'dan e-posta alınıyor
            string email = TempData["ResetEmail"] as string;
            
            if (string.IsNullOrEmpty(email))
            {
                // E-posta yoksa şifremi unuttum sayfasına yönlendir
                return RedirectToAction("ForgotPassword");
            }
            
            ViewData["Email"] = email;
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetToken(string email, string verificationCode)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(verificationCode))
            {
                ViewData["ErrorMessage"] = "E-posta ve doğrulama kodu gereklidir";
                ViewData["Email"] = email;
                return View();
            }

            try
            {
                // E-posta ve doğrulama kodu ile kullanıcıyı bul
                var user = await _mongoContext.Users
                    .Find(u => u.Email == email && u.VerificationCode == verificationCode)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    ViewData["ErrorMessage"] = "Geçersiz doğrulama kodu";
                    ViewData["Email"] = email;
                    return View();
                }

                // Kodun süresi dolmuş mu kontrol et
                if (user.VerificationCodeExpiry < DateTime.UtcNow)
                {
                    ViewData["ErrorMessage"] = "Doğrulama kodunun süresi dolmuş";
                    ViewData["Email"] = email;
                    return View();
                }

                // Doğrulama başarılı, yeni şifre oluşturma sayfasına yönlendir
                TempData["ResetEmail"] = email;
                TempData["VerificationCode"] = verificationCode;
                return RedirectToAction("ResetPassword");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}";
                ViewData["Email"] = email;
                return View();
            }
        }
        
        [HttpGet]
        public IActionResult ResetPassword()
        {
            // TempData'dan bilgileri al
            string email = TempData["ResetEmail"] as string;
            string verificationCode = TempData["VerificationCode"] as string;
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(verificationCode))
            {
                // Gerekli bilgiler yoksa şifremi unuttum sayfasına yönlendir
                return RedirectToAction("ForgotPassword");
            }
            
            ViewData["Email"] = email;
            ViewData["VerificationCode"] = verificationCode;
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string verificationCode, string newPassword, string confirmPassword)
        {
            ViewData["Email"] = email;
            ViewData["VerificationCode"] = verificationCode;
            
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewData["ErrorMessage"] = "Yeni şifre ve şifre onayı gereklidir";
                return View();
            }
            
            if (newPassword != confirmPassword)
            {
                ViewData["ErrorMessage"] = "Şifreler eşleşmiyor";
                return View();
            }
            
            if (newPassword.Length < 6)
            {
                ViewData["ErrorMessage"] = "Şifre en az 6 karakter olmalıdır";
                return View();
            }

            try
            {
                // E-posta ve doğrulama kodu ile kullanıcıyı bul
                var user = await _mongoContext.Users
                    .Find(u => u.Email == email && u.VerificationCode == verificationCode)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    ViewData["ErrorMessage"] = "Geçersiz doğrulama kodu";
                    return View();
                }

                // Kodun süresi dolmuş mu kontrol et
                if (user.VerificationCodeExpiry < DateTime.UtcNow)
                {
                    ViewData["ErrorMessage"] = "Doğrulama kodunun süresi dolmuş";
                    return View();
                }

                // Şifreyi güncelle
                var hashedPassword = HashPassword(newPassword);
                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                var update = Builders<User>.Update
                    .Set(u => u.Password, hashedPassword)
                    .Set(u => u.VerificationCode, "") // Doğrulama kodunu temizle
                    .Set(u => u.VerificationCodeExpiry, DateTime.MinValue); // Süreyi sıfırla
                
                await _mongoContext.Users.UpdateOneAsync(filter, update);

                // Başarı mesajı ve giriş sayfasına yönlendir
                TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi. Yeni şifrenizle giriş yapabilirsiniz.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}";
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