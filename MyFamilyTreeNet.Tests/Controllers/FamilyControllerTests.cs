using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyFamilyTreeNet.Api.Contracts;
using MyFamilyTreeNet.Api.Controllers;
using MyFamilyTreeNet.Api.DTOs;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;
using System.Security.Claims;
using Xunit;

namespace MyFamilyTreeNet.Tests.Controllers;

public class FamilyControllerTests
{
    private readonly Mock<IFamilyService> _mockFamilyService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly FamilyController _controller;
    private readonly AppDbContext _context;

    public FamilyControllerTests()
    {
        _mockFamilyService = new Mock<IFamilyService>();
        _mockMapper = new Mock<IMapper>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _controller = new FamilyController(_mockFamilyService.Object, _mockMapper.Object, _context);
    }

    [Fact]
    public async Task GetAllFamilies_ReturnsOkResultWithFamilies()
    {
        // Arrange
        var families = new List<Family>
        {
            new Family
            {
                Id = 1,
                Name = "Test Family",
                Description = "Test Description",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = "user1",
                FamilyMembers = new List<FamilyMember>(),
                Photos = new List<Photo>(),
                Stories = new List<Story>()
            }
        };

        _mockFamilyService.Setup(x => x.GetAllFamiliesAsync())
            .ReturnsAsync(families);

        // Act
        var result = await _controller.GetAllFamilies();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFamilyById_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var familyId = 1;
        var family = new Family
        {
            Id = familyId,
            Name = "Test Family",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = "user1",
            FamilyMembers = new List<FamilyMember>(),
            Photos = new List<Photo>(),
            Stories = new List<Story>()
        };

        _mockFamilyService.Setup(x => x.GetFamilyByIdAsync(familyId))
            .ReturnsAsync(family);

        // Act
        var result = await _controller.GetFamilyById(familyId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFamilyById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var familyId = 999;
        _mockFamilyService.Setup(x => x.GetFamilyByIdAsync(familyId))
            .ReturnsAsync((Family?)null);

        // Act
        var result = await _controller.GetFamilyById(familyId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateFamily_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var createFamilyDto = new CreateFamilyDto
        {
            Name = "New Family",
            Description = "New Description"
        };

        var userId = "test-user-id";
        var createdFamily = new Family
        {
            Id = 1,
            Name = createFamilyDto.Name,
            Description = createFamilyDto.Description,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _mockFamilyService.Setup(x => x.CreateFamilyAsync(It.IsAny<Family>()))
            .ReturnsAsync(createdFamily);

        // Setup user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.CreateFamily(createFamilyDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult?.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateFamily_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var createFamilyDto = new CreateFamilyDto
        {
            Name = "New Family",
            Description = "New Description"
        };

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await _controller.CreateFamily(createFamilyDto);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task UpdateFamily_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var familyId = 1;
        var userId = "test-user-id";
        var updateFamilyDto = new UpdateFamilyDto
        {
            Name = "Updated Family",
            Description = "Updated Description",
            IsPublic = true
        };

        var updatedFamily = new Family
        {
            Id = familyId,
            Name = updateFamilyDto.Name,
            Description = updateFamilyDto.Description,
            IsPublic = updateFamilyDto.IsPublic
        };

        _mockFamilyService.Setup(x => x.UserOwnsFamilyAsync(familyId, userId))
            .ReturnsAsync(true);
        _mockFamilyService.Setup(x => x.UpdateFamilyAsync(familyId, It.IsAny<Family>()))
            .ReturnsAsync(updatedFamily);

        // Setup user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.UpdateFamily(familyId, updateFamilyDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateFamily_WithUnauthorizedUser_ReturnsForbid()
    {
        // Arrange
        var familyId = 1;
        var userId = "test-user-id";
        var updateFamilyDto = new UpdateFamilyDto
        {
            Name = "Updated Family",
            Description = "Updated Description",
            IsPublic = true
        };

        _mockFamilyService.Setup(x => x.UserOwnsFamilyAsync(familyId, userId))
            .ReturnsAsync(false);

        // Setup user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.UpdateFamily(familyId, updateFamilyDto);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task DeleteFamily_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var familyId = 1;
        var userId = "test-user-id";

        _mockFamilyService.Setup(x => x.UserOwnsFamilyAsync(familyId, userId))
            .ReturnsAsync(true);
        _mockFamilyService.Setup(x => x.DeleteFamilyAsync(familyId))
            .ReturnsAsync(true);

        // Setup user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.DeleteFamily(familyId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteFamily_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var familyId = 999;
        var userId = "test-user-id";

        _mockFamilyService.Setup(x => x.UserOwnsFamilyAsync(familyId, userId))
            .ReturnsAsync(true);
        _mockFamilyService.Setup(x => x.DeleteFamilyAsync(familyId))
            .ReturnsAsync(false);

        // Setup user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.DeleteFamily(familyId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetFamilyTreeData_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var familyId = 1;
        var family = new Family
        {
            Id = familyId,
            Name = "Test Family",
            FamilyMembers = new List<FamilyMember>
            {
                new FamilyMember
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    DateOfBirth = new DateTime(1980, 1, 1),
                    Gender = Gender.Male
                }
            }
        };

        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetFamilyTreeData(familyId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFamilyTreeData_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var familyId = 999;

        // Act
        var result = await _controller.GetFamilyTreeData(familyId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}