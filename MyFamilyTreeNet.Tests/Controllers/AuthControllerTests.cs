using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyFamilyTreeNet.Api.Controllers;
using MyFamilyTreeNet.Api.DTOs;
using MyFamilyTreeNet.Data.Models;
using System.Security.Claims;
using Xunit;

namespace MyFamilyTreeNet.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockUserManager = CreateMockUserManager();
        _mockSignInManager = CreateMockSignInManager();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<AuthController>>();

        SetupConfiguration();

        _controller = new AuthController(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockConfiguration.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    private static Mock<UserManager<User>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
    }

    private static Mock<SignInManager<User>> CreateMockSignInManager()
    {
        var userManager = CreateMockUserManager();
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
        return new Mock<SignInManager<User>>(userManager.Object, contextAccessor.Object, userPrincipalFactory.Object, null, null, null, null);
    }

    private void SetupConfiguration()
    {
        _mockConfiguration.Setup(x => x["JwtSettings:SecretKey"]).Returns("ThisIsAVeryLongSecretKeyForJWTTokenGeneration12345");
        _mockConfiguration.Setup(x => x["JwtSettings:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(x => x["JwtSettings:Audience"]).Returns("TestAudience");
        _mockConfiguration.Setup(x => x["JwtSettings:ExpirationInHours"]).Returns("24");
    }

    [Fact]
    public async Task Register_WithValidModel_ReturnsOkResult()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            MiddleName = "Smith",
            LastName = "Doe"
        };

        var user = new User
        {
            Id = "test-id",
            Email = registerDto.Email,
            UserName = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName
        };

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName ?? string.Empty,
            LastName = user.LastName
        };

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(new List<string> { "User" });
        _mockMapper.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDto);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_WithNullModel_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Register(null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult?.Value.Should().Be("Model cannot be null");
    }

    [Fact]
    public async Task Register_WithIdentityFailure_ReturnsBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "John",
            MiddleName = "Smith",
            LastName = "Doe"
        };

        var identityError = new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" };
        var identityResult = IdentityResult.Failed(identityError);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Password123!",
            RememberMe = false
        };

        var user = new User
        {
            Id = "test-id",
            Email = loginDto.Email,
            UserName = loginDto.Email,
            FirstName = "John",
            LastName = "Doe"
        };

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName ?? string.Empty,
            LastName = user.LastName
        };

        _mockSignInManager.Setup(x => x.PasswordSignInAsync(
            loginDto.Email, loginDto.Password, loginDto.RememberMe, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });
        _mockMapper.Setup(x => x.Map<UserDto>(user))
            .Returns(userDto);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "WrongPassword",
            RememberMe = false
        };

        _mockSignInManager.Setup(x => x.PasswordSignInAsync(
            loginDto.Email, loginDto.Password, loginDto.RememberMe, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithNullModel_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Login(null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult?.Value.Should().Be("Model cannot be null");
    }

    [Fact]
    public async Task Logout_ReturnsOkResult()
    {
        // Arrange
        _mockSignInManager.Setup(x => x.SignOutAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.Value.Should().NotBeNull();
    }

    [Fact]
    public void Test_ReturnsOkResult()
    {
        // Act
        var result = _controller.Test();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.Value.Should().NotBeNull();
    }
}