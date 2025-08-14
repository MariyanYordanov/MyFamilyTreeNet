using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Api.Services;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;
using Xunit;

namespace MyFamilyTreeNet.Tests.Services;

public class MemberServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly MemberService _service;

    public MemberServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new MemberService(_context);
    }

    [Fact]
    public async Task GetFamilyMembersAsync_ReturnsCorrectMembers()
    {
        // Arrange
        var family = new Family
        {
            Name = "Test Family",
            Description = "Test Description",
            CreatedByUserId = "user1",
            CreatedAt = DateTime.UtcNow
        };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var members = new List<FamilyMember>
        {
            new FamilyMember
            {
                FirstName = "John",
                LastName = "Doe",
                FamilyId = family.Id,
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1980, 1, 1)
            },
            new FamilyMember
            {
                FirstName = "Jane",
                LastName = "Doe",
                FamilyId = family.Id,
                Gender = Gender.Female,
                DateOfBirth = new DateTime(1985, 5, 15)
            },
            new FamilyMember
            {
                FirstName = "Bob",
                LastName = "Smith",
                FamilyId = family.Id + 1, // Different family
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1975, 3, 10)
            }
        };

        _context.FamilyMembers.AddRange(members);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetFamilyMembersAsync(family.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(m => m.FamilyId == family.Id);
        result.Should().Contain(m => m.FirstName == "John");
        result.Should().Contain(m => m.FirstName == "Jane");
    }

    [Fact]
    public async Task GetFamilyMembersAsync_WithNoMembers_ReturnsEmptyCollection()
    {
        // Act
        var result = await _service.GetFamilyMembersAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMemberByIdAsync_WithValidId_ReturnsMember()
    {
        // Arrange
        var family = new Family
        {
            Name = "Test Family",
            Description = "Test Description",
            CreatedByUserId = "user1",
            CreatedAt = DateTime.UtcNow
        };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var member = new FamilyMember
        {
            FirstName = "John",
            LastName = "Doe",
            FamilyId = family.Id,
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1980, 1, 1),
            Biography = "Test biography"
        };

        _context.FamilyMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetMemberByIdAsync(member.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(member.Id);
        result.FirstName.Should().Be(member.FirstName);
        result.LastName.Should().Be(member.LastName);
        result.Family.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMemberByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _service.GetMemberByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateMemberAsync_WithValidMember_ReturnsCreatedMember()
    {
        // Arrange
        var family = new Family
        {
            Name = "Test Family",
            Description = "Test Description",
            CreatedByUserId = "user1",
            CreatedAt = DateTime.UtcNow
        };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var member = new FamilyMember
        {
            FirstName = "New",
            LastName = "Member",
            FamilyId = family.Id,
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1990, 1, 1),
            Biography = "New member biography"
        };

        // Act
        var result = await _service.CreateMemberAsync(member);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.FirstName.Should().Be(member.FirstName);
        result.LastName.Should().Be(member.LastName);

        var savedMember = await _context.FamilyMembers.FindAsync(result.Id);
        savedMember.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateMemberAsync_WithValidData_ReturnsUpdatedMember()
    {
        // Arrange
        var family = new Family
        {
            Name = "Test Family",
            Description = "Test Description",
            CreatedByUserId = "user1",
            CreatedAt = DateTime.UtcNow
        };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var member = new FamilyMember
        {
            FirstName = "Original",
            LastName = "Member",
            FamilyId = family.Id,
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1980, 1, 1),
            Biography = "Original biography"
        };

        _context.FamilyMembers.Add(member);
        await _context.SaveChangesAsync();

        var updateData = new FamilyMember
        {
            FirstName = "Updated",
            LastName = "Member",
            MiddleName = "Middle",
            Gender = Gender.Female,
            DateOfBirth = new DateTime(1985, 5, 15),
            DateOfDeath = new DateTime(2020, 12, 31),
            Biography = "Updated biography",
            PlaceOfBirth = "New York",
            PlaceOfDeath = "Los Angeles"
        };

        // Act
        var result = await _service.UpdateMemberAsync(member.Id, updateData);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be(updateData.FirstName);
        result.LastName.Should().Be(updateData.LastName);
        result.MiddleName.Should().Be(updateData.MiddleName);
        result.Gender.Should().Be(updateData.Gender);
        result.DateOfBirth.Should().Be(updateData.DateOfBirth);
        result.DateOfDeath.Should().Be(updateData.DateOfDeath);
        result.Biography.Should().Be(updateData.Biography);
        result.PlaceOfBirth.Should().Be(updateData.PlaceOfBirth);
        result.PlaceOfDeath.Should().Be(updateData.PlaceOfDeath);

        var savedMember = await _context.FamilyMembers.FindAsync(member.Id);
        savedMember!.FirstName.Should().Be(updateData.FirstName);
    }

    [Fact]
    public async Task UpdateMemberAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var updateData = new FamilyMember
        {
            FirstName = "Updated",
            LastName = "Member"
        };

        // Act
        var result = await _service.UpdateMemberAsync(999, updateData);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteMemberAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var family = new Family
        {
            Name = "Test Family",
            Description = "Test Description",
            CreatedByUserId = "user1",
            CreatedAt = DateTime.UtcNow
        };
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var member = new FamilyMember
        {
            FirstName = "To Delete",
            LastName = "Member",
            FamilyId = family.Id,
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1980, 1, 1)
        };

        _context.FamilyMembers.Add(member);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteMemberAsync(member.Id);

        // Assert
        result.Should().BeTrue();

        var deletedMember = await _context.FamilyMembers.FindAsync(member.Id);
        deletedMember.Should().BeNull();
    }

    [Fact]
    public async Task DeleteMemberAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteMemberAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}