var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// MongoDB bağlantısını test etme
using (var scope = app.Services.CreateScope())
{
    try
    {
        // MongoDbContext servisini al
        var mongoContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

        // Bağlantı kontrolü yapmak için bir koleksiyon çağırıyoruz
        var collection = mongoContext.GetCollection<User>("Users");
        Console.WriteLine("MongoDB bağlantısı başarılı!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"MongoDB bağlantı hatası: {ex.Message}");
        // Hata ayıklamak için detaylı stack trace yazdır
        Console.WriteLine(ex.StackTrace);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  // Bu satırda gereksiz bir tanımlama vardı, kaldırdım
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();  // Bu satırda gereksiz bir `IApplicationBuilder` kullanımı vardı, onu kaldırdım

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
