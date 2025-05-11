using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BitkiHastalikTahmini.Models;
using MongoDB.Driver;

namespace BitkiHastalikTahmini.Controllers
{
    public class AdminController : Controller
    {
        private readonly MongoDbContext _mongoContext;
        private const string BACKGROUND_SETTING_KEY = "BackgroundImage";

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
                ViewBag.TotalAnalysis = 25; // Örnek değer
                ViewBag.TodayAnalysis = 3;  // Örnek değer

                return View();
            }
            catch (Exception ex)
            {
                // Hata durumunda boş liste ile devam et
                ViewBag.Users = new List<User>();
                ViewBag.TotalUsers = 0;
                ViewBag.ActiveUsers = 0;
                ViewBag.TotalAnalysis = 0;
                ViewBag.TodayAnalysis = 0;
                ViewBag.ErrorMessage = $"Kullanıcılar yüklenirken hata: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetBackgroundImage([FromBody] string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return Json(new { success = false, message = "Geçersiz resim yolu." });
            }
                
            try
            {
                // Arka plan ayarını veritabanında kaydet
                var filter = Builders<AppSetting>.Filter.Eq(s => s.Key, BACKGROUND_SETTING_KEY);
                var update = Builders<AppSetting>.Update.Set(s => s.Value, imagePath);
                
                var result = await _mongoContext.AppSettings.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
                
                // Session'a da kaydet
                HttpContext.Session.SetString(BACKGROUND_SETTING_KEY, imagePath);
                
                return Json(new { success = true, imagePath });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveSettings(IFormCollection form)
        {
            try
            {
                // Form verilerini al
                int maxTrials = int.Parse(form["maxTrials"]);
                bool maintenanceMode = form["maintenanceMode"] == "on" || form["maintenanceMode"] == "true";

                // Ayarları veritabanına kaydet
                await SaveSettingToDatabaseAsync("MaxTrials", maxTrials.ToString());
                await SaveSettingToDatabaseAsync("MaintenanceMode", maintenanceMode.ToString());

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

                            // Arka plan resim yolunu hem session'a hem de veritabanına kaydet
                            string backgroundImage = "/images/background/" + fileName;
                            HttpContext.Session.SetString(BACKGROUND_SETTING_KEY, backgroundImage);
                            await SaveSettingToDatabaseAsync(BACKGROUND_SETTING_KEY, backgroundImage);
                            
                            TempData["SuccessMessage"] = "Arka plan resmi başarıyla güncellendi.";
                        }
                    }
                }

                // Diğer ayarları session'a kaydet
                HttpContext.Session.SetInt32("MaxTrials", maxTrials);
                HttpContext.Session.SetString("MaintenanceMode", maintenanceMode.ToString());

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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
                        file.CopyTo(stream);
                    }

                    var imagePath = "/backgrounds/" + fileName;
                    
                    // Yüklenen resmi veritabanına ve session'a kaydet
                    await SaveSettingToDatabaseAsync(BACKGROUND_SETTING_KEY, imagePath);
                    HttpContext.Session.SetString(BACKGROUND_SETTING_KEY, imagePath);

                    return Json(new { success = true, imagePath });
                }
                return Json(new { success = false, message = "Dosya yüklenemedi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        // Ayarları veritabanına kaydetme yardımcı metodu
        private async Task SaveSettingToDatabaseAsync(string key, string value)
        {
            var filter = Builders<AppSetting>.Filter.Eq(s => s.Key, key);
            var update = Builders<AppSetting>.Update.Set(s => s.Value, value);
            
            await _mongoContext.AppSettings.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
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

                // Aynı şekilde images/background klasöründeki resimleri de ekle
                var bgFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "background");
                if (Directory.Exists(bgFolder))
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var files = Directory.GetFiles(bgFolder)
                        .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLower()))
                        .Select(f => "/images/background/" + Path.GetFileName(f))
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
    }
}