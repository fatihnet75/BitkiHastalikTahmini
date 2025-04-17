using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace BitkiHastalikTahmini.Tests.Services
{
    public class MongoDbContextTests
    {
        private readonly Mock<IOptions<MongoDbSettings>> _mockOptions;
        private readonly MongoDbSettings _settings;

        public MongoDbContextTests()
        {
            _settings = new MongoDbSettings
            {
                ConnectionString = "mongodb+srv://Ahmet2002:Ahmet2002@cluster0.dd8h3.mongodb.net/BitkiTespitiDB?authSource=admin",
                DatabaseName = "BitkiTespitiDB"
            };

            _mockOptions = new Mock<IOptions<MongoDbSettings>>();
            _mockOptions.Setup(x => x.Value).Returns(_settings);
        }

        [Fact]
        public void Constructor_ValidSettings_CreatesMongoClient()
        {
            // Act
            var context = new MongoDbContext(_mockOptions.Object);

            // Assert
            Assert.NotNull(context);
        }

        [Fact]
        public void Users_Collection_IsAccessible()
        {
            // Arrange
            var context = new MongoDbContext(_mockOptions.Object);

            // Act & Assert
            Assert.NotNull(context.Users);
        }
    }

    public class TestModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
} 