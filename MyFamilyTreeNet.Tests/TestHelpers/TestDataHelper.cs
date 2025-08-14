using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Tests.TestHelpers;

public static class TestDataHelper
{
    public static async Task<User> CreateTestUserAsync(UserManager<User> userManager, string email = "test@example.com")
    {
        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(user, "Password123!");
        await userManager.AddToRoleAsync(user, "User");
        return user;
    }

    public static Family CreateTestFamily(string userId = "test-user-id")
    {
        return new Family
        {
            Name = "Test Family",
            Description = "Test family description",
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsPublic = true
        };
    }

    public static FamilyMember CreateTestMember(int familyId)
    {
        return new FamilyMember
        {
            FirstName = "John",
            LastName = "Doe",
            FamilyId = familyId,
            Gender = Gender.Male,
            DateOfBirth = new DateTime(1980, 1, 1),
            Biography = "Test biography"
        };
    }

    public static async Task SeedTestDataAsync(AppDbContext context)
    {
        if (await context.Families.AnyAsync())
            return;

        var families = new List<Family>
        {
            new Family
            {
                Name = "Smith Family",
                Description = "The Smith family tree",
                CreatedByUserId = "user1",
                CreatedAt = DateTime.UtcNow,
                IsPublic = true
            },
            new Family
            {
                Name = "Johnson Family",
                Description = "The Johnson family tree",
                CreatedByUserId = "user2",
                CreatedAt = DateTime.UtcNow,
                IsPublic = false
            }
        };

        context.Families.AddRange(families);
        await context.SaveChangesAsync();

        var members = new List<FamilyMember>
        {
            new FamilyMember
            {
                FirstName = "John",
                LastName = "Smith",
                FamilyId = families[0].Id,
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1950, 3, 15),
                Biography = "Patriarch of the Smith family"
            },
            new FamilyMember
            {
                FirstName = "Mary",
                LastName = "Smith",
                FamilyId = families[0].Id,
                Gender = Gender.Female,
                DateOfBirth = new DateTime(1955, 8, 22),
                Biography = "Matriarch of the Smith family"
            },
            new FamilyMember
            {
                FirstName = "Robert",
                LastName = "Johnson",
                FamilyId = families[1].Id,
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1960, 12, 1),
                Biography = "Head of the Johnson family"
            }
        };

        context.FamilyMembers.AddRange(members);
        await context.SaveChangesAsync();
    }

    public static AppDbContext CreateInMemoryContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}