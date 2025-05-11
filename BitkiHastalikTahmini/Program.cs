using BitkiHastalikTahmini;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using BitkiHastalikTahmini.Models;
using BitkiHastalikTahmini.Middleware;

var builder = WebApplication.CreateBuilder(args);

// MongoDB yapılandırması
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddHttpClient();
// HttpContextAccessor yapılandırması (View'larda veya servislerde HttpContext erişimi için)
builder.Services.AddHttpContextAccessor();

// Session yapılandırması
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(6);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// MVC yapılandırması
builder.Services.AddControllersWithViews();

// Register Email Service
builder.Services.AddSingleton<EmailService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.UseSession(); // Session middleware burada aktif edilmeli

// Arka plan resmi middleware'i ekle
app.UseMiddleware<BackgroundMiddleware>();

// Deneme hakkı middleware'i ekle
app.UseMiddleware<MaxTrialsMiddleware>();

// AuthMiddleware'i ekle
app.UseMiddleware<AuthMiddleware>();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}");

app.Run();