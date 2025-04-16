using BitkiHastalikTahmini.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BitkiHastalikTahmini.Controllers
{
    public class AdminController : Controller
    {
        private readonly MongoDbContext _mongoContext;

        public AdminController(MongoDbContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // MongoDB'den tüm kullanıcıları getir
                var users = await _mongoContext.Users.Find(_ => true).ToListAsync();

                // Kullanıcıları view'a aktar
                ViewBag.Users = users;

                // İstatistikler
                ViewBag.TotalUsers = users.Count;
                ViewBag.ActiveUsers = users.Count; // Tüm kullanıcıları aktif kabul et

                // Analiz istatistikleri
                ViewBag.TotalAnalysis = 0; // Analiz koleksiyonunuz varsa burayı düzenleyin
                ViewBag.TodayAnalysis = 0;

                return View();
            }
            catch (Exception ex)
            {
                // Hatayı logla
                ViewBag.ErrorMessage = $"Veri yükleme sırasında bir hata oluştu: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser([FromBody] UserEditModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Id))
                {
                    return Json(new { success = false, message = "Geçersiz kullanıcı ID." });
                }

                var filter = Builders<User>.Filter.Eq(u => u.Id, model.Id);

                // Güncelleme tanımını oluştur
                var update = Builders<User>.Update
                    .Set(u => u.FirstName, model.FirstName)
                    .Set(u => u.LastName, model.LastName)
                    .Set(u => u.Email, model.Email);

                // Şifre yalnızca girildiği takdirde güncellenir
                if (!string.IsNullOrEmpty(model.Password))
                {
                    string hashedPassword = HashPassword(model.Password);
                    update = update.Set(u => u.Password, hashedPassword);
                }

                // Güncellemeyi çalıştır
                var result = await _mongoContext.Users.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return Json(new { success = true, message = "Kullanıcı başarıyla güncellendi." });
                }
                else
                {
                    return Json(new { success = false, message = "Kullanıcı güncellenemedi." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser([FromBody] UserDeleteModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Id))
                {
                    return Json(new { success = false, message = "Geçersiz kullanıcı ID." });
                }

                var filter = Builders<User>.Filter.Eq(u => u.Id, model.Id);
                var result = await _mongoContext.Users.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    return Json(new { success = true, message = "Kullanıcı başarıyla silindi." });
                }
                else
                {
                    return Json(new { success = false, message = "Kullanıcı bulunamadı veya silinemedi." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // Şifre hashleme yardımcı metodu
        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSettings(IFormCollection form)
        {
            try
            {
                // Form verilerini al
                int maxTrials = int.Parse(form["maxTrials"]);
                bool maintenanceMode = form["maintenanceMode"] == "on" || form["maintenanceMode"] == "true";

                // Resim dosyasını işle
                if (Request.Form.Files.Count > 0 && Request.Form.Files[0] != null)
                {
                    var backgroundFile = Request.Form.Files[0];

                    // Dosya kontrolü
                    if (backgroundFile.Length > 0)
                    {
                        // Dosya uzantısı kontrolü
                        var extension = Path.GetExtension(backgroundFile.FileName).ToLower();
                        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif")
                        {
                            // Resmi kaydedilecek dizin
                            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "background");

                            // Dizin yoksa oluştur
                            if (!Directory.Exists(uploadsFolder))
                                Directory.CreateDirectory(uploadsFolder);

                            // Benzersiz dosya adı oluştur
                            var fileName = "background" + extension;
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            // Eğer aynı isimde dosya varsa sil
                            if (System.IO.File.Exists(filePath))
                                System.IO.File.Delete(filePath);

                            // Dosyayı kaydet
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                backgroundFile.CopyTo(fileStream);
                            }

                            // Arka plan resim yolunu ayarlara kaydet
                            // Veritabanı veya appsettings.json kullanabilirsiniz
                            // Örnek: Burada session kullanıyorum
                            HttpContext.Session.SetString("BackgroundImage", "/images/background/" + fileName);
                        }
                    }
                }

                // Diğer ayarları kaydet
                // Veritabanı veya appsettings.json kullanabilirsiniz
                HttpContext.Session.SetInt32("MaxTrials", maxTrials);
                HttpContext.Session.SetString("MaintenanceMode", maintenanceMode.ToString());

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }  
        [HttpGet]
        public IActionResult GetBackgroundImages()
        {
            try
            {
                var backgroundsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "backgrounds");
                var images = new List<string>();

                if (Directory.Exists(backgroundsFolder))
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var files = Directory.GetFiles(backgroundsFolder)
                        .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLower()))
                        .Select(f => "/backgrounds/" + Path.GetFileName(f))
                        .ToList();

                    images.AddRange(files);
                }

                // Varsayılan arka planı da listeye ekle
                images.Add("/images/default-background.jpg");

                return Json(new { success = true, images });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SetBackgroundImage([FromBody] string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                HttpContext.Session.SetString("BackgroundImage", imagePath);
                return Json(new { success = true, imagePath });
            }
            return Json(new { success = false, message = "Geçersiz resim yolu." });
        }

        [HttpPost]
        public async Task<IActionResult> UploadBackgroundImage(IFormFile file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "backgrounds");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return Json(new { success = true, imagePath = "/backgrounds/" + fileName });
                }
                return Json(new { success = false, message = "Dosya yüklenemedi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    

        
    }

    // Kullanıcı düzenleme isteği için model
    public class UserEditModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Kullanıcı silme isteği için model
    public class UserDeleteModel
    {
        public string Id { get; set; }
    }
}