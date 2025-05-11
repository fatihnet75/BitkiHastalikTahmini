using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Threading.Tasks;
using System;
using BitkiHastalikTahmini.Models;

namespace BitkiHastalikTahmini.Middleware
{
    public class BackgroundMiddleware
    {
        private readonly RequestDelegate _next;
        private const string BACKGROUND_SETTING_KEY = "BackgroundImage";

        public BackgroundMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, MongoDbContext dbContext)
        {
            try
            {
                // Session'da arka plan resmi yoksa veritabanından yükle
                if (string.IsNullOrEmpty(context.Session.GetString(BACKGROUND_SETTING_KEY)))
                {
                    // Veritabanından arka plan ayarını al
                    var filter = Builders<AppSetting>.Filter.Eq(s => s.Key, BACKGROUND_SETTING_KEY);
                    var setting = await dbContext.AppSettings.Find(filter).FirstOrDefaultAsync();

                    if (setting != null && !string.IsNullOrEmpty(setting.Value))
                    {
                        // Arka plan resmini session'a kaydet
                        context.Session.SetString(BACKGROUND_SETTING_KEY, setting.Value);
                    }
                    else
                    {
                        // Varsayılan arka plan resmini session'a kaydet
                        context.Session.SetString(BACKGROUND_SETTING_KEY, "/images/default-background.jpg");
                    }
                }

                // Middleware zincirinde sonraki adıma geç
                await _next(context);
            }
            catch (Exception ex)
            {
                // Hata oluşursa işlemi kesme, sonraki adıma geç
                Console.WriteLine($"BackgroundMiddleware Error: {ex.Message}");
                await _next(context);
            }
        }
    }
} 