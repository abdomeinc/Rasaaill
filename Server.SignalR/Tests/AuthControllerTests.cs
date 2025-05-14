using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Server.SignalR.Controllers;
using Shared.Services.Interfaces;
using Xunit;

namespace Server.SignalR.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<ILogger<AuthController>> _loggerMock = new();
        private readonly Mock<UserManager<Entities.Models.User>> _userManagerMock;
        private readonly Mock<IConfiguration> _configMock = new();
        private readonly Mock<IVerificationService> _verificationServiceMock = new();
        private readonly Entities.ApplicationDbContext _dbContext;

        public AuthControllerTests()
        {
            var store = new Mock<IUserStore<Entities.Models.User>>();
            _userManagerMock = new Mock<UserManager<Entities.Models.User>>(store.Object, null, null, null, null, null, null, null, null);

            var options = new DbContextOptionsBuilder<Entities.ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _dbContext = new Entities.ApplicationDbContext(options);

            // Setup minimal JWT config
            _configMock.Setup(c => c["Jwt:Secret"]).Returns("supersecretkey1234567890");
            _configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        }

        private AuthController CreateController()
        {
            return new AuthController(
                _loggerMock.Object,
                _userManagerMock.Object,
                _configMock.Object,
                _dbContext,
                _verificationServiceMock.Object
            );
        }

        [Fact]
        public async Task RequestCode_ReturnsBadRequest_WhenUserNotFound()
        {
            // Arrange
            _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((Entities.Models.User)null!);
            var controller = CreateController();
            var dto = new Entities.Dtos.EmailDto { Email = "notfound@example.com" };

            // Act
            var result = await controller.RequestCode(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email not found.", badRequest.Value);
        }

        [Fact]
        public async Task RequestCode_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var user = new Entities.Models.User { Email = "user@example.com" };
            _userManagerMock.Setup(m => m.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);
            _verificationServiceMock.Setup(v => v.SendVerificationCodeAsync(user.Email))
                .ReturnsAsync("123456");
            var controller = CreateController();
            var dto = new Entities.Dtos.EmailDto { Email = user.Email };

            // Act
            var result = await controller.RequestCode(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Verification code sent.", okResult.Value);
        }

        [Fact]
        public async Task VerifyVerificationCode_ReturnsBadRequest_WhenVerificationFails()
        {
            // Arrange
            _verificationServiceMock.Setup(v => v.VerifyCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, "Invalid code", null));
            var controller = CreateController();
            var dto = new Entities.Dtos.VerifyCodeDto { Email = "user@example.com", Code = "badcode" };

            // Act
            var result = await controller.VerifyVerificationCode(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid code", badRequest.Value);
        }

        [Fact]
        public async Task VerifyVerificationCode_ReturnsOk_WithToken_WhenVerificationSucceeds()
        {
            // Arrange
            var user = new Entities.Models.User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                DisplayName = "Test User",
                IsApproved = true
            };
            _verificationServiceMock.Setup(v => v.VerifyCodeAsync(user.Email, "goodcode"))
                .ReturnsAsync((true, null, user));
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string>());
            _userManagerMock.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync(new List<Claim>());
            var controller = CreateController();
            var dto = new Entities.Dtos.VerifyCodeDto { Email = user.Email, Code = "goodcode" };

            // Act
            var result = await controller.VerifyVerificationCode(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<Entities.Dtos.VerifyVerificationCodeResultDto>(okResult.Value);
            Assert.False(string.IsNullOrEmpty(data.Token));
            Assert.False(string.IsNullOrEmpty(data.RefreshToken));
        }

        [Fact]
        public async Task RefreshLoginToken_ReturnsUnauthorized_WhenTokenInvalid()
        {
            // Arrange
            var controller = CreateController();
            var dto = new Entities.Dtos.TokenRefreshRequestDto
            {
                AccessToken = "invalid",
                RefreshToken = "invalid"
            };

            // Act & Assert
            await Assert.ThrowsAsync<Microsoft.IdentityModel.Tokens.SecurityTokenException>(() =>
                controller.RefreshLoginToken(dto));
        }

        [Fact]
        public async Task RefreshLoginToken_ReturnsOk_WhenTokenValid()
        {
            // Arrange
            var user = new Entities.Models.User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                DisplayName = "Test User",
                IsApproved = true
            };
            // Generate a valid token and refresh token
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string>());
            _userManagerMock.Setup(m => m.GetClaimsAsync(user)).ReturnsAsync(new List<Claim>());
            _userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

            var controller = CreateController();
            // Generate tokens using the controller's private method via reflection
            var method = typeof(AuthController).GetMethod("GenerateJwtToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<(string, Entities.Dtos.RefreshTokenDto)>)method!.Invoke(controller, new object[] { user })!;
            await task;
            var (accessToken, refreshTokenDto) = task.Result;

            // Add refresh token to DB with correct JwtId
            var jwtHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = jwtHandler.ReadJwtToken(accessToken);
            var jti = jwt.Id;
            var dbToken = new Entities.Models.RefreshToken
            {
                Token = refreshTokenDto.Token,
                Expires = DateTime.UtcNow.AddDays(7),
                JwtId = jti,
                UserId = user.Id
            };
            _dbContext.RefreshTokens.Add(dbToken);
            _dbContext.SaveChanges();

            var dto = new Entities.Dtos.TokenRefreshRequestDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenDto.Token
            };

            // Act
            var result = await controller.RefreshLoginToken(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void Validate_ReturnsOk()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Validate();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(new { valid = true }.ToString(), okResult.Value.ToString());
        }
    }
}
