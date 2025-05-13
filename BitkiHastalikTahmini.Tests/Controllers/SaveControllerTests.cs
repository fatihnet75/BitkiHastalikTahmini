using BitkiHastalikTahmini.Controllers;
using BitkiHastalikTahmini.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BitkiHastalikTahmini.Tests.Controllers
{
    public class SaveControllerTests
    {
        private readonly Mock<MongoDbContext> _mockMongoContext;
        private readonly Mock<EmailService> _mockEmailService;
        private readonly SaveController _controller;

        public SaveControllerTests()
        {
            _mockMongoContext = new Mock<MongoDbContext>();
            _mockEmailService = new Mock<EmailService>();
            _controller = new SaveController(_mockMongoContext.Object, _mockEmailService.Object);
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
        public async Task Index_Post_ValidUser_RedirectsToIndex()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                Password = "password123",
                FirstName = "Test",
                LastName = "User"
            };

            // Act
            var result = await _controller.Index(user);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Index_Post_InvalidUser_ReturnsViewWithModel()
        {
            // Arrange
            var user = new User(); // Invalid user
            _controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _controller.Index(user);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(user, viewResult.Model);
        }

        [Fact]
        public async Task Index_Post_DuplicateEmail_ReturnsViewWithError()
        {
            // Arrange
            var user = new User
            {
                Email = "existing@example.com",
                Password = "password123",
                FirstName = "Test",
                LastName = "User"
            };

            var existingUser = new User { Email = "existing@example.com" };
            var mockCursor = new Mock<IAsyncCursor<User>>();
            mockCursor.Setup(x => x.Current).Returns(new List<User> { existingUser });
            mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);

            _mockMongoContext.Setup(x => x.Users.Find(It.IsAny<FilterDefinition<User>>()))
                .Returns(mockCursor.Object);

            // Act
            var result = await _controller.Index(user);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ErrorCount > 0);
        }
    }
} 