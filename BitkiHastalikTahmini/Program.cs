using BitkiHastalikTahmini;
using BitkiHastalikTahmini.Models;
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
        var mongoContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        var collection = mongoContext.Users;

        // E�er hi� kullan�c� yoksa, bir kullan�c� ekle
        var userCount = collection.CountDocuments(FilterDefinition<User>.Empty);

        if (userCount == 0)
        {
            var defaultUser = new User
            {
                FirstName = "�rnek",
                LastName = "Kullan�c�",
                Email = "ornek@mail.com",
                Password = "12345"
            };

            collection.InsertOne(defaultUser);
            Console.WriteLine("Varsay�lan kullan�c� eklendi!");
        }

        Console.WriteLine($"Toplam {userCount} kullan�c� bulundu.");
        Console.WriteLine("MongoDB ba�lant�s� ba�ar�l�!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"MongoDB ba�lant� hatas�: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}");

app.Run();