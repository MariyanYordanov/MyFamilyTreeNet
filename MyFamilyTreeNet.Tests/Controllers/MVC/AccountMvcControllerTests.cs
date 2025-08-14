using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MyFamilyTreeNet.Api.Controllers.MVC;
using MyFamilyTreeNet.Api.DTOs;
using MyFamilyTreeNet.Data.Models;
using Xunit;
using FluentAssertions;

namespace MyFamilyTreeNet.Tests.Controllers.MVC;

public class AccountControllerTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly Mock<ILogger<AccountController>> _mockLogger;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        _mockUserManager = CreateMockUserManager();
        _mockSignInManager = CreateMockSignInManager();
        _mockLogger = new Mock<ILogger<AccountController>>();

        _controller = new AccountController(
            _mockUserManager.Object,
            _mockSignInManager.Object,
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

    [Fact]
    public void Login_GET_ReturnsView()
    {
        // Act
        var result = _controller.Login();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Register_GET_ReturnsView()
    {
        // Act
        var result = _controller.Register();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

}