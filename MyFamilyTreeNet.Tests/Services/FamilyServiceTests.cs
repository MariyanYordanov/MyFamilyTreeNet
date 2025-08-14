using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Api.Services;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;
using Xunit;

namespace MyFamilyTreeNet.Tests.Services;

public class FamilyServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly FamilyService _service;

    public FamilyServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new FamilyService(_context);
    }

    [Fact]
    public async Task GetAllFamiliesAsync_ReturnsAllFamilies()
    {
        // Arrange
        var families = new List<Family>
        {
            new Family
            {
                Name = "Family 1",
                Description = "Description 1",
                CreatedByUserId = "user1",
                CreatedAt = DateTime.UtcNow,
                FamilyMembers = new List<FamilyMember>(),
                Photos = new List<Photo>(),
                Stories = new List<Story>()
            },
            new Family
            {
                Name = "Family 2",
                Description = "Description 2",
                CreatedByUserId = "user2",
                CreatedAt = DateTime.UtcNow,
                FamilyMembers = new List<FamilyMember>(),
                Photos = new List<Photo>(),
                Stories = new List<Story>()
            }
        };

        _context.Families.AddRange(families);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllFamiliesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(f => f.FamilyMembers != null && f.Photos != null && f.Stories != null);
    }

    [Fact]
    public async Task GetFamilyByIdAsync_WithValidId_ReturnsFamily()
    {
        // Arrange
        var family = new Family
        {
            Name = "Test Family",
            Description = "Test Description",
            CreatedByUserId = "user1",
            CreatedAt = DateTime.UtcNow,
            FamilyMembers = new List<FamilyMember>(),
            Photos = new List<Photo>(),
            Stories = new List<Story>()
        };

        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetFamilyByIdAsync(family.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(family.Id);
        result.Name.Should().Be(family.Name);
        result.FamilyMembers.Should().NotBeNull();
        result.Photos.Should().NotBeNull();
        result.Stories.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFamilyByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _service.GetFamilyByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateFamilyAsync_WithValidFamily_ReturnsCreatedFamily()
    {
        // Arrange
        var family = new Family
        {
            Name = "New Family",
            Description = "New Description",
            CreatedByUserId = "user1",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _service.CreateFamilyAsync(family);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be(family.Name);
        result.Description.Should().Be(family.Description);

        var savedFamily = await _context.Families.FindAsync(result.Id);
        savedFamily.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateFamilyAsync_WithValidData_ReturnsUpdatedFamily()
    {
        // Arrange
        var family = new Family
        {
            Name = "Original Family",
            Description = "Original Description",
            CreatedByUserId = "user1",
            CreatedAt = DateTime.UtcNow
        };

        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var updateData = new Family
        {
            Name = "Updated Family",
            Description = "Updated Description"
        };

        // Act
        var result = await _service.UpdateFamilyAsync(family.Id, updateData);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(updateData.Name);
        result.Description.Should().Be(updateData.Description);

        var savedFamily = await _context.Families.FindAsync(family.Id);
        savedFamily!.Name.Should().Be(updateData.Name);
        savedFamily.Description.Should().Be(updateData.Description);
    }

    [Fact]
    public async Task UpdateFamilyAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var updateData = new Family
        {
            Name = "Updated Family",
            Description = "Updated Description"
        };

        // Act
        var result = await _service.UpdateFamilyAsync(999, updateData);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteFamilyAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var family = new Family
        {
            Name = "Family to Delete",
            Description = "Description",
            CreatedByUserId = "user1",
            CreatedAt = DateTime.UtcNow
        };

        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteFamilyAsync(family.Id);

        // Assert
        result.Should().BeTrue();

        var deletedFamily = await _context.Families.FindAsync(family.Id);
        deletedFamily.Should().BeNull();
    }

    [Fact]
    public async Task DeleteFamilyAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteFamilyAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserOwnsFamilyAsync_WithCorrectUser_ReturnsTrue()
    {
        // Arrange
        var userId = "user1";
        var family = new Family
        {
            Name = "User's Family",
            Description = "Description",
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.UserOwnsFamilyAsync(family.Id, userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserOwnsFamilyAsync_WithIncorrectUser_ReturnsFalse()
    {
        // Arrange
        var family = new Family
        {
            Name = "Someone's Family",
            Description = "Description",
            CreatedByUserId = "user1",
            CreatedAt = DateTime.UtcNow
        };

        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.UserOwnsFamilyAsync(family.Id, "user2");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UserOwnsFamilyAsync_WithInvalidFamilyId_ReturnsFalse()
    {
        // Act
        var result = await _service.UserOwnsFamilyAsync(999, "user1");

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}