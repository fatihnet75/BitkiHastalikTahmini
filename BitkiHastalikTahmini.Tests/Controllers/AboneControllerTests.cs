using Microsoft.AspNetCore.Mvc;
using Xunit;
using BitkiHastalikTahmini.Controllers;
using Moq;
using Microsoft.Extensions.Options;

namespace BitkiHastalikTahmini.Tests.Controllers
{
    public class AboneControllerTests
    {
        private readonly Abone_LoginController _controller;
        private readonly Mock<MongoDbContext> _mockContext;

        public AboneControllerTests()
        {
            _mockContext = new Mock<MongoDbContext>();
            _controller = new Abone_LoginController(_mockContext.Object);
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }
} 