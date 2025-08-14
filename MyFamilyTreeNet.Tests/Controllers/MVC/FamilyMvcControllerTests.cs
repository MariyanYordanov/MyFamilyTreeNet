using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MyFamilyTreeNet.Api.Controllers.MVC;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;
using System.Security.Claims;
using Xunit;
using FluentAssertions;

namespace MyFamilyTreeNet.Tests.Controllers.MVC;

public class FamilyMvcControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<FamilyMvcController>> _mockLogger;
    private readonly FamilyMvcController _controller;
    private readonly string _testUserId = "test-user-123";

    public FamilyMvcControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _mockLogger = new Mock<ILogger<FamilyMvcController>>();
        
        _controller = new FamilyMvcController(_context, _mockLogger.Object);
        
        // Setup user context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
    }

    [Fact]
    public async Task Index_ReturnsViewWithUserFamilies()
    {
        // Arrange
        var family1 = new Family
        {
            Id = 1,
            Name = "Smith Family",
            Description = "First family",
            CreatedByUserId = _testUserId,
            CreatedAt = DateTime.UtcNow,
            FamilyMembers = new List<FamilyMember>(),
            Photos = new List<Photo>(),
            Stories = new List<Story>()
        };
        
        var family2 = new Family
        {
            Id = 2,
            Name = "Johnson Family", 
            Description = "Second family",
            CreatedByUserId = "other-user",
            CreatedAt = DateTime.UtcNow,
            FamilyMembers = new List<FamilyMember>(),
            Photos = new List<Photo>(),
            Stories = new List<Story>()
        };

        _context.Families.AddRange(family1, family2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Family>>().Subject.ToList();
        model.Should().HaveCount(1);
        model[0].Name.Should().Be("Smith Family");
        model[0].CreatedByUserId.Should().Be(_testUserId);
    }

    [Fact]
    public async Task Details_WithValidId_ReturnsViewWithFamily()
    {
        // Arrange
        var family = new Family
        {
            Id = 1,
            Name = "Anderson Family",
            Description = "Sample family",
            CreatedByUserId = _testUserId,
            CreatedAt = DateTime.UtcNow,
            FamilyMembers = new List<FamilyMember>(),
            Photos = new List<Photo>(),
            Stories = new List<Story>()
        };

        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<Family>().Subject;
        model.Name.Should().Be("Anderson Family");
        model.Id.Should().Be(1);
    }

    [Fact]
    public async Task Details_WithInvalidId_RedirectsToHome()
    {
        // Act
        var result = await _controller.Details(999);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult?.ActionName.Should().Be("Index");
        redirectResult?.ControllerName.Should().Be("Home");
    }

    [Fact]
    public void Create_GET_ReturnsView()
    {
        // Act
        var result = _controller.Create();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}