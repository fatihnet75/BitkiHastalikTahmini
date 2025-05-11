using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace BitkiHastalikTahmini.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Korumalı controller'ları tanımla
                string path = context.Request.Path.ToString().ToLower();
                
                // Admin sayfasına doğrudan erişim
                if (path.StartsWith("/admin"))
                {
                    await _next(context);
                    return;
                }
                
                // Giriş sayfalarını direkt olarak geç
                if (path.Contains("/adminlogin") || path.Contains("/abone_login"))
                {
                    await _next(context);
                    return;
                }
                
                if (path.StartsWith("/abone"))
                {
                    // Abone sayfaları için yetki kontrolü
                    var userId = context.Session.GetString("UserId");
                    if (string.IsNullOrEmpty(userId))
                    {
                        // Yetki yoksa login sayfasına yönlendir
                        context.Response.Redirect("/Abone_Login");
                        return;
                    }
                }

                // Middleware zincirinde sonraki adıma geç
                await _next(context);
            }
            catch (Exception ex)
            {
                // Hata oluşursa süreci kesme, log alabiliriz
                Console.WriteLine($"AuthMiddleware Error: {ex.Message}");
                await _next(context);
            }
        }
    }
} 