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

        // Kullan�c� listesini g�r�nt�le
        public IActionResult Index()
        {
            var users = _context.Users.Find(_ => true).ToList();
            return View(users); // Index.cshtml dosyas�n� a�acak
        }



        // Yeni kullan�c� ekleme sayfas�n� g�ster
        public IActionResult Create()
        {
            return View();
        }

        // Yeni kullan�c� kaydetme i�lemi
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
                ViewBag.ErrorMessage = $"Kullan�c� kaydedilirken hata: {ex.Message}";
                return View(user);
            }
        }
    }
}