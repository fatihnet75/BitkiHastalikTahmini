using BitkiHastalikTahmini.Controllers;
using BitkiHastalikTahmini.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace BitkiHastalikTahmini.Tests.Controllers
{
    public class SaveControllerTests
    {
        private readonly Mock<MongoDbContext> _mockContext;
        private readonly SaveController _controller;

        public SaveControllerTests()
        {
            _mockContext = new Mock<MongoDbContext>();
            _controller = new SaveController(_mockContext.Object);
        }

        [Fact]
        public void Index_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_Post_ValidUser_RedirectsToLogin()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Test",
                LastName = "Kullanıcı",
                Email = "test@test.com",
                Password = "123456"
            };

            _mockContext.Setup(x => x.Users.Find(It.IsAny<FilterDefinition<User>>()))
                .Returns(Mock.Of<IFindFluent<User, User>>());

            // Act
            var result = await _controller.Index(user) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Abone_Login", result.ControllerName);
        }

        [Fact]
        public async Task Index_Post_ExistingEmail_ReturnsViewWithError()
        {
            // Arrange
            var user = new User { Email = "mevcut@email.com" };
            var existingUser = new User();

            var mockFind = new Mock<IFindFluent<User, User>>();
            mockFind.Setup(x => x.FirstOrDefaultAsync())
                .ReturnsAsync(existingUser);

            _mockContext.Setup(x => x.Users.Find(It.IsAny<FilterDefinition<User>>()))
                .Returns(mockFind.Object);

            // Act
            var result = await _controller.Index(user) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(result.ViewData.ModelState.IsValid);
            Assert.Contains("Email", result.ViewData.ModelState.Keys);
        }

        [Theory]
        [InlineData("123")] // Çok kısa şifre
        [InlineData("123456789012345678901")] // Çok uzun şifre
        [InlineData("password")] // Basit şifre
        public async Task Index_Post_InvalidPasswordFormat_ReturnsViewWithError(string password)
        {
            // Arrange
            var user = new User
            {
                FirstName = "Test",
                LastName = "Kullanıcı",
                Email = "test@test.com",
                Password = password
            };

            // Act
            var result = await _controller.Index(user) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(result.ViewData.ModelState.IsValid);
            Assert.Contains("Password", result.ViewData.ModelState.Keys);
        }

        [Theory]
        [InlineData("", "Kullanıcı", "test@test.com", "123456")] // Eksik isim
        [InlineData("Test", "", "test@test.com", "123456")]      // Eksik soyisim
        [InlineData("Test", "Kullanıcı", "", "123456")]          // Eksik email
        [InlineData("Test", "Kullanıcı", "test@test.com", "")]   // Eksik şifre
        public async Task Index_Post_MissingInformation_ReturnsViewWithError(
            string firstName, string lastName, string email, string password)
        {
            // Arrange
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password
            };

            // Act
            var result = await _controller.Index(user) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(result.ViewData.ModelState.IsValid);
        }

        [Theory]
        [InlineData("test@test.com", true)]  // Geçerli email
        [InlineData("test.com", false)]      // @ işareti yok
        [InlineData("test@", false)]         // Domain yok
        [InlineData("test@test.", false)]    // Geçersiz domain
        [InlineData("@test.com", false)]     // Kullanıcı adı yok
        public async Task Index_Post_EmailFormatValidation_ChecksFormat(string email, bool isValid)
        {
            // Arrange
            var user = new User
            {
                FirstName = "Test",
                LastName = "Kullanıcı",
                Email = email,
                Password = "123456"
            };

            // Act
            var result = await _controller.Index(user) as ViewResult;

            // Assert
            if (!isValid)
            {
                Assert.NotNull(result);
                Assert.False(result.ViewData.ModelState.IsValid);
                Assert.Contains("Email", result.ViewData.ModelState.Keys);
            }
        }

        [Theory]
        [InlineData("Test123", true)]        // Alfanumerik karakterler
        [InlineData("Test@123", false)]      // Özel karakter (@)
        [InlineData("Test#User", false)]     // Özel karakter (#)
        [InlineData("Test User", false)]     // Boşluk
        [InlineData("Test-User", false)]     // Tire
        public async Task Index_Post_NameSpecialCharacterValidation_ChecksFormat(string firstName, bool isValid)
        {
            // Arrange
            var user = new User
            {
                FirstName = firstName,
                LastName = "Kullanıcı",
                Email = "test@test.com",
                Password = "123456"
            };

            // Act
            var result = await _controller.Index(user) as ViewResult;

            // Assert
            if (!isValid)
            {
                Assert.NotNull(result);
                Assert.False(result.ViewData.ModelState.IsValid);
                Assert.Contains("FirstName", result.ViewData.ModelState.Keys);
            }
        }
    }
} 