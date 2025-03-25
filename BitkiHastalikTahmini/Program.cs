using BitkiHastalikTahmini;
using BitkiHastalikTahmini.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// MongoDB yapýlandýrmasýný servislere ekle
builder.Services.AddSingleton<MongoDbContext>();

// MVC servisini ekle
builder.Services.AddControllersWithViews();

var app = builder.Build();

// MongoDB baðlantýsýný test etme
using (var scope = app.Services.CreateScope())
{
    try
    {
        var mongoContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
        var collection = mongoContext.Users;

        // Eðer hiç kullanýcý yoksa, bir kullanýcý ekle
        var userCount = collection.CountDocuments(FilterDefinition<User>.Empty);

        if (userCount == 0)
        {
            var defaultUser = new User
            {
                FirstName = "Örnek",
                LastName = "Kullanýcý",
                Email = "ornek@mail.com",
                Password = "12345"
            };

            collection.InsertOne(defaultUser);
            Console.WriteLine("Varsayýlan kullanýcý eklendi!");
        }

        Console.WriteLine($"Toplam {userCount} kullanýcý bulundu.");
        Console.WriteLine("MongoDB baðlantýsý baþarýlý!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"MongoDB baðlantý hatasý: {ex.Message}");
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