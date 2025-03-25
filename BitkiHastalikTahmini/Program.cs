using BitkiHastalikTahmini;
using BitkiHastalikTahmini.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// MongoDB yap�land�rmas�n� servislere ekle
builder.Services.AddSingleton<MongoDbContext>();

// MVC servisini ekle
builder.Services.AddControllersWithViews();

var app = builder.Build();

// MongoDB ba�lant�s�n� test etme
using (var scope = app.Services.CreateScope())
{
    try
    {
        // MongoDbContext servisini al
        var mongoContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

        // Ba�lant� kontrol� yapmak i�in bir koleksiyon �a��r�yoruz
        var collection = mongoContext.GetCollection<User>("Users");
        Console.WriteLine("MongoDB ba�lant�s� ba�ar�l�!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"MongoDB ba�lant� hatas�: {ex.Message}");
        // Hata ay�klamak i�in detayl� stack trace yazd�r
        Console.WriteLine(ex.StackTrace);
    }
}

if (!app.Environment.IsDevelopment())
{   
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  // Bu sat�rda gereksiz bir `IApplicationBuilder` tan�mlamas� vard�, kald�rd�m
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();  // Authorization i�levini ekledim ancak burada do�ru kullan�m� sa�lad�m.

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
