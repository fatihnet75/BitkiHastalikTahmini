
// MongoDbContext.cs
using BitkiHastalikTahmini.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;

namespace BitkiHastalikTahmini
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            // Appsettings.json'daki ayarlarý almak için doðru anahtarlarý kullan
            var connectionString = configuration["MongoDBSettings:ConnectionString"];
            var databaseName = configuration["MongoDBSettings:DatabaseName"];

            // Baðlantý dizesi null kontrolü
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString),
                    "MongoDB baðlantý dizesi bulunamadý. Appsettings.json dosyasýný kontrol edin.");
            }

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}