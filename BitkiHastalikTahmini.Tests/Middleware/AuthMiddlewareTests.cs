using BitkiHastalikTahmini.Middleware;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BitkiHastalikTahmini.Tests.Middleware
{
    public class AuthMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _mockNext;
        private readonly AuthMiddleware _middleware;

        public AuthMiddlewareTests()
        {
            _mockNext = new Mock<RequestDelegate>();
            _middleware = new AuthMiddleware(_mockNext.Object);
        }

        [Fact]
        public async Task InvokeAsync_AdminPath_ContinuesToNextMiddleware()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/admin";

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _mockNext.Verify(x => x(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_AdminLoginPath_ContinuesToNextMiddleware()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/adminlogin";

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _mockNext.Verify(x => x(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_AbonePathWithoutSession_RedirectsToLogin()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/abone";
            context.Session = new Mock<ISession>().Object;

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(302, context.Response.StatusCode);
            Assert.Equal("/Abone_Login", context.Response.Headers["Location"].ToString());
        }

        [Fact]
        public async Task InvokeAsync_AbonePathWithSession_ContinuesToNextMiddleware()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/abone";
            var session = new Mock<ISession>();
            session.Setup(x => x.GetString("UserId")).Returns("test-user-id");
            context.Session = session.Object;

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _mockNext.Verify(x => x(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WhenExceptionOccurs_ContinuesToNextMiddleware()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/abone";
            _mockNext.Setup(x => x(context)).Throws(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _middleware.InvokeAsync(context));
        }

        [Fact]
        public async Task InvokeAsync_PublicPath_ContinuesToNextMiddleware()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/home";

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            _mockNext.Verify(x => x(context), Times.Once);
        }
    }
} 