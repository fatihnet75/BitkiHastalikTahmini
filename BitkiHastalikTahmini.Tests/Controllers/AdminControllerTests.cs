using BitkiHastalikTahmini.Controllers;
using BitkiHastalikTahmini.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BitkiHastalikTahmini.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<MongoDbContext> _mockMongoContext;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _mockMongoContext = new Mock<MongoDbContext>();
            _controller = new AdminController(_mockMongoContext.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithUsers()
        {
            // Arrange
            var mockUsers = new List<User>
            {
                new User { Email = "admin1@example.com", FirstName = "Admin", LastName = "1" },
                new User { Email = "admin2@example.com", FirstName = "Admin", LastName = "2" }
            };

            var mockCursor = new Mock<IAsyncCursor<User>>();
            mockCursor.Setup(x => x.Current).Returns(mockUsers);
            mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);

            _mockMongoContext.Setup(x => x.Users.Find(It.IsAny<FilterDefinition<User>>()))
                .Returns(mockCursor.Object);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["Users"]);
            Assert.Equal(2, ((List<User>)viewResult.ViewData["Users"]).Count);
        }

        [Fact]
        public async Task Index_WhenExceptionOccurs_ReturnsViewWithEmptyData()
        {
            // Arrange
            _mockMongoContext.Setup(x => x.Users.Find(It.IsAny<FilterDefinition<User>>()))
                .Throws(new Exception("Test exception"));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["ErrorMessage"]);
            Assert.Equal(0, viewResult.ViewData["TotalUsers"]);
            Assert.Equal(0, viewResult.ViewData["ActiveUsers"]);
        }

        [Fact]
        public async Task UpdateBackground_ValidFile_ReturnsRedirectToAction()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("test.jpg");

            // Act
            var result = await _controller.UpdateBackground(mockFile.Object);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task UpdateBackground_InvalidFile_ReturnsViewWithError()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _controller.UpdateBackground(mockFile.Object);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["ErrorMessage"]);
        }

        [Fact]
        public async Task UpdateBackground_InvalidFileExtension_ReturnsViewWithError()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("test.txt");

            // Act
            var result = await _controller.UpdateBackground(mockFile.Object);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["ErrorMessage"]);
        }
    }
} 