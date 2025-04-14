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
            // Appsettings.json'daki ayarlar? almak için do?ru anahtarlar? kullan
            var connectionString = configuration["MongoDBSettings:ConnectionString"];
            var databaseName = configuration["MongoDBSettings:DatabaseName"];

            // Ba?lant? dizesi null kontrolü
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString),
                    "MongoDB ba?lant? dizesi bulunamad?. Appsettings.json dosyas?n? kontrol edin.");
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