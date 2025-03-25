// UserController.cs
using BitkiHastalikTahmini.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;

namespace BitkiHastalikTahmini.Controllers
{
    public class UserController : Controller
    {
        private readonly MongoDbContext _context;

        public UserController(MongoDbContext context)
        {
            _context = context;
        }

        // Kullanýcý listesini görüntüle
        public IActionResult Index()
        {
            var users = _context.Users.Find(_ => true).ToList();
            return View(users); // Index.cshtml dosyasýný açacak
        }



        // Yeni kullanýcý ekleme sayfasýný göster
        public IActionResult Create()
        {
            return View();
        }

        // Yeni kullanýcý kaydetme iþlemi
        [HttpPost]
        public IActionResult Create(User user)
        {
            try
            {
                _context.Users.InsertOne(user);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Kullanýcý kaydedilirken hata: {ex.Message}";
                return View(user);
            }
        }
    }
}