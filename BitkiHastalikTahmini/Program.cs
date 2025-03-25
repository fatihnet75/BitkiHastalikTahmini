using BitkiHastalikTahmini;
using BitkiHastalikTahmini.Models;
using Microsoft.Extensions.DependencyInjection;
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
        // MongoDbContext servisini al
        var mongoContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

        // Baðlantý kontrolü yapmak için bir koleksiyon çaðýrýyoruz
        var collection = mongoContext.GetCollection<User>("Users");
        Console.WriteLine("MongoDB baðlantýsý baþarýlý!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"MongoDB baðlantý hatasý: {ex.Message}");
        // Hata ayýklamak için detaylý stack trace yazdýr
        Console.WriteLine(ex.StackTrace);
    }
}

if (!app.Environment.IsDevelopment())
{   
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  // Bu satýrda gereksiz bir `IApplicationBuilder` tanýmlamasý vardý, kaldýrdým
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();  // Authorization iþlevini ekledim ancak burada doðru kullanýmý saðladým.

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
