using Microsoft.AspNetCore.Mvc;
using Xunit;
using BitkiHastalikTahmini.Controllers;
using Moq;

namespace BitkiHastalikTahmini.Tests.Controllers
{
    public class AdminLoginTests
    {
        private readonly AdminController _controller;
        private readonly Mock<MongoDbContext> _mockContext;

        public AdminLoginTests()
        {
            _mockContext = new Mock<MongoDbContext>();
            _controller = new AdminController(_mockContext.Object);
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
        public async Task Index_Post_ValidCredentials_RedirectsToAdminIndex()
        {
            // Arrange
            var loginModel = new LoginModel 
            { 
                Email = "fatihgurbuz7536@gmail.com",
                Password = "123456"
            };

            // Act
            var result = await _controller.Index(loginModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Admin", result.ControllerName);
        }

        [Theory]
        [InlineData("wrong@email.com", "123456")]
        [InlineData("fatihgurbuz7536@gmail.com", "wrongpass")]
        [InlineData("", "")]
        public async Task Index_Post_InvalidCredentials_ReturnsViewWithError(string email, string password)
        {
            // Arrange
            var loginModel = new LoginModel { Email = email, Password = password };

            // Act
            var result = await _controller.Index(loginModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Kullanıcı adı veya şifre hatalı.", result.ViewData["LoginError"]);
        }
    }
} 