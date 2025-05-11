using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Threading.Tasks;
using System;

namespace BitkiHastalikTahmini.Middleware
{
    public class MaxTrialsMiddleware
    {
        private readonly RequestDelegate _next;
        private const string MAX_TRIALS_KEY = "MaxTrials";

        public MaxTrialsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Hangi kontroller için işlem yapıldığını belirle
                string path = context.Request.Path.ToString().ToLower();
                
                // User controller için MaxTrials değeri
                if (path.StartsWith("/user"))
                {
                    context.Session.SetInt32(MAX_TRIALS_KEY, 2);
                }
                // Abone controller için MaxTrials değeri
                else if (path.StartsWith("/abone"))
                {
                    context.Session.SetInt32(MAX_TRIALS_KEY, 100);
                }
                
                // Middleware zincirinde sonraki adıma geç
                await _next(context);
            }
            catch (Exception ex)
            {
                // Hata oluşursa işlemi kesme, sonraki adıma geç
                Console.WriteLine($"MaxTrialsMiddleware Error: {ex.Message}");
                await _next(context);
            }
        }
    }
} 