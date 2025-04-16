using BitkiHastalikTahmini.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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