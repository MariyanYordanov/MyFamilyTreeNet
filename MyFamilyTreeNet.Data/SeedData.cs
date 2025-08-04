using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Data;

public static class SeedData
{
    public static async Task Initialize(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminEmail = "admin@myfamilytreenet.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                MiddleName = "System",
                LastName = "User",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        var demoUsers = new List<User>();
        var demoUserData = new[]
        {
            ("john@demo.com", "John", "Demo", "Doe", new DateTime(1975, 5, 15)),
            ("jane@demo.com", "Jane", "Marie", "Doe", new DateTime(1978, 8, 22)),
            ("mike@demo.com", "Michael", "James", "Doe", new DateTime(2000, 3, 10)),
            ("sarah@demo.com", "Sarah", "Elizabeth", "Doe", new DateTime(2002, 11, 5)),
            ("bob@demo.com", "Robert", "William", "Smith", new DateTime(1950, 2, 28)),
            ("mary@demo.com", "Mary", "Ann", "Smith", new DateTime(1952, 7, 14))
        };

        foreach (var (email, firstName, middleName, lastName, dateOfBirth) in demoUserData)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new User
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,
                    DateOfBirth = dateOfBirth
                };

                var result = await userManager.CreateAsync(user, "Demo123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                    demoUsers.Add(user);
                }
            }
            else
            {
                var existingUser = await userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    demoUsers.Add(existingUser);
                }
            }
        }

        if (!await context.Families.AnyAsync())
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            var adminId = adminUser?.Id ?? "";
            
            var doeFamily = new Family
            {
                Name = "The Doe Family",
                Description = "Welcome to the Doe family tree! We're a loving family spanning three generations.",
                CreatedByUserId = demoUsers.FirstOrDefault()?.Id ?? adminId,
                CreatedAt = DateTime.UtcNow,
                IsPublic = true
            };

            var smithFamily = new Family
            {
                Name = "The Smith Family",
                Description = "The Smith family legacy - preserving our heritage and memories.",
                CreatedByUserId = demoUsers.Skip(4).FirstOrDefault()?.Id ?? adminId,
                CreatedAt = DateTime.UtcNow,
                IsPublic = true
            };

            var royalFamily = new Family
            {
                Name = "The Royal Family Tree",
                Description = "A complex family demonstrating all possible relationship types including cousin marriages, step-relationships, and multiple generations.",
                CreatedByUserId = adminId,
                CreatedAt = DateTime.UtcNow,
                IsPublic = true
            };

            context.Families.AddRange(doeFamily, smithFamily, royalFamily);
            await context.SaveChangesAsync();

            var familyMembers = new List<FamilyMember>
            {
                
                new FamilyMember { FamilyId = doeFamily.Id, FirstName = "John", MiddleName = "Demo", LastName = "Doe", DateOfBirth = new DateTime(1975, 5, 15), Gender = Gender.Male, AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new FamilyMember { FamilyId = doeFamily.Id, FirstName = "Jane", MiddleName = "Marie", LastName = "Doe", DateOfBirth = new DateTime(1978, 8, 22), Gender = Gender.Female, AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new FamilyMember { FamilyId = doeFamily.Id, FirstName = "Michael", MiddleName = "James", LastName = "Doe", DateOfBirth = new DateTime(2000, 3, 10), Gender = Gender.Male, AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new FamilyMember { FamilyId = doeFamily.Id, FirstName = "Sarah", MiddleName = "Elizabeth", LastName = "Doe", DateOfBirth = new DateTime(2002, 11, 5), Gender = Gender.Female, AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                
               
                new FamilyMember { FamilyId = smithFamily.Id, FirstName = "Robert", MiddleName = "William", LastName = "Smith", DateOfBirth = new DateTime(1950, 2, 28), Gender = Gender.Male, AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new FamilyMember { FamilyId = smithFamily.Id, FirstName = "Mary", MiddleName = "Ann", LastName = "Smith", DateOfBirth = new DateTime(1952, 7, 14), Gender = Gender.Female, AddedByUserId = adminId, CreatedAt = DateTime.UtcNow }
            };

            var royalMembers = new List<FamilyMember>
            {
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "King", MiddleName = "Edward", LastName = "Royal I", DateOfBirth = new DateTime(1900, 1, 1), DateOfDeath = new DateTime(1980, 1, 1), Gender = Gender.Male, Biography = "Founder of the Royal dynasty. Established the family kingdom.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Queen", MiddleName = "Elizabeth", LastName = "Royal I", DateOfBirth = new DateTime(1905, 3, 15), DateOfDeath = new DateTime(1985, 3, 15), Gender = Gender.Female, Biography = "Beloved matriarch. Known for her wisdom and diplomatic skills.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                

                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Prince", MiddleName = "Charles", LastName = "Royal", DateOfBirth = new DateTime(1925, 4, 21), DateOfDeath = new DateTime(2005, 4, 21), Gender = Gender.Male, Biography = "Eldest son of King Edward I. Military hero and statesman.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Princess", MiddleName = "Diana", LastName = "Noble-Royal", DateOfBirth = new DateTime(1930, 7, 12), DateOfDeath = new DateTime(2010, 7, 12), Gender = Gender.Female, Biography = "Married Prince Charles, creating alliance.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                

                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "King", MiddleName = "William", LastName = "Royal II", DateOfBirth = new DateTime(1950, 8, 15), Gender = Gender.Male, Biography = "Current reigning monarch. Son of Prince Charles and Princess Diana.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Queen", MiddleName = "Isabella", LastName = "Cambridge-Royal", DateOfBirth = new DateTime(1955, 12, 25), Gender = Gender.Female, Biography = "Queen consort. From the Cambridge noble family.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                
             
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Crown Prince", MiddleName = "Alexander", LastName = "Royal", DateOfBirth = new DateTime(1975, 2, 14), Gender = Gender.Male, Biography = "Heir to the throne. Son of King William II and Queen Isabella.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Crown Princess", MiddleName = "Sophia", LastName = "Habsburg-Royal", DateOfBirth = new DateTime(1978, 8, 25), Gender = Gender.Female, Biography = "Wife of Crown Prince Alexander. From Austrian Habsburg family.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                
              
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Prince", MiddleName = "Edward", LastName = "Royal III", DateOfBirth = new DateTime(2000, 3, 21), Gender = Gender.Male, Biography = "Future king. Son of Crown Prince Alexander and Crown Princess Sophia.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow }
            };

            familyMembers.AddRange(royalMembers);
            context.FamilyMembers.AddRange(familyMembers);
            await context.SaveChangesAsync();

         
            var relationships = new List<Relationship>
            {
            
                new Relationship { PrimaryMemberId = familyMembers[0].Id, RelatedMemberId = familyMembers[1].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = demoUsers.FirstOrDefault()?.Id ?? adminId, CreatedAt = DateTime.UtcNow },
                new Relationship { PrimaryMemberId = familyMembers[0].Id, RelatedMemberId = familyMembers[2].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = demoUsers.FirstOrDefault()?.Id ?? adminId, CreatedAt = DateTime.UtcNow },
                new Relationship { PrimaryMemberId = familyMembers[0].Id, RelatedMemberId = familyMembers[3].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = demoUsers.FirstOrDefault()?.Id ?? adminId, CreatedAt = DateTime.UtcNow },
                new Relationship { PrimaryMemberId = familyMembers[1].Id, RelatedMemberId = familyMembers[2].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = demoUsers.Skip(1).FirstOrDefault()?.Id ?? adminId, CreatedAt = DateTime.UtcNow },
                new Relationship { PrimaryMemberId = familyMembers[1].Id, RelatedMemberId = familyMembers[3].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = demoUsers.Skip(1).FirstOrDefault()?.Id ?? adminId, CreatedAt = DateTime.UtcNow },
                new Relationship { PrimaryMemberId = familyMembers[2].Id, RelatedMemberId = familyMembers[3].Id, RelationshipType = RelationshipType.Sibling, CreatedByUserId = demoUsers.Skip(2).FirstOrDefault()?.Id ?? adminId, CreatedAt = DateTime.UtcNow },
                
             
                new Relationship { PrimaryMemberId = familyMembers[4].Id, RelatedMemberId = familyMembers[5].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = demoUsers.Skip(4).FirstOrDefault()?.Id ?? adminId, CreatedAt = DateTime.UtcNow },
        
                new Relationship { PrimaryMemberId = familyMembers[6].Id, RelatedMemberId = familyMembers[7].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId, CreatedAt = DateTime.UtcNow }, // King Edward I & Queen Elizabeth I
                new Relationship { PrimaryMemberId = familyMembers[8].Id, RelatedMemberId = familyMembers[9].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId, CreatedAt = DateTime.UtcNow }, // Prince & Princess
                new Relationship { PrimaryMemberId = familyMembers[10].Id, RelatedMemberId = familyMembers[11].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId, CreatedAt = DateTime.UtcNow }, // King William II & Queen Isabella
                new Relationship { PrimaryMemberId = familyMembers[12].Id, RelatedMemberId = familyMembers[13].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId, CreatedAt = DateTime.UtcNow }, // Crown Prince & Princess

                new Relationship { PrimaryMemberId = familyMembers[6].Id, RelatedMemberId = familyMembers[8].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new Relationship { PrimaryMemberId = familyMembers[7].Id, RelatedMemberId = familyMembers[8].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new Relationship { PrimaryMemberId = familyMembers[8].Id, RelatedMemberId = familyMembers[10].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new Relationship { PrimaryMemberId = familyMembers[9].Id, RelatedMemberId = familyMembers[10].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new Relationship { PrimaryMemberId = familyMembers[10].Id, RelatedMemberId = familyMembers[12].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new Relationship { PrimaryMemberId = familyMembers[11].Id, RelatedMemberId = familyMembers[12].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new Relationship { PrimaryMemberId = familyMembers[12].Id, RelatedMemberId = familyMembers[14].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new Relationship { PrimaryMemberId = familyMembers[13].Id, RelatedMemberId = familyMembers[14].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId, CreatedAt = DateTime.UtcNow }
            };

            context.Relationships.AddRange(relationships);
            await context.SaveChangesAsync();

            var stories = new List<Story>
            {
                new Story
                {
                    FamilyId = doeFamily.Id,
                    AuthorUserId = demoUsers.FirstOrDefault()?.Id ?? adminId,
                    Title = "Our Wedding Day",
                    Content = "It was a beautiful summer day when Jane and I tied the knot. Friends and family gathered to celebrate our love...",
                    CreatedAt = DateTime.UtcNow.AddDays(-365)
                },
                new Story
                {
                    FamilyId = smithFamily.Id,
                    AuthorUserId = demoUsers.Skip(4).FirstOrDefault()?.Id ?? adminId,
                    Title = "50 Years Together",
                    Content = "Mary and I celebrated our golden anniversary surrounded by our children and grandchildren. What a journey it has been...",
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new Story
                {
                    FamilyId = royalFamily.Id,
                    AuthorUserId = adminId,
                    Title = "The Foundation of Our Dynasty",
                    Content = "King Edward Royal I established our family's reign in 1900. Born during turbulent times, he united the kingdom through wisdom and strength.",
                    CreatedAt = DateTime.UtcNow.AddDays(-100)
                }
            };

            context.Stories.AddRange(stories);
            await context.SaveChangesAsync();

            var photos = new List<Photo>
            {
                new Photo
                {
                    FamilyId = doeFamily.Id,
                    UploadedByUserId = demoUsers.Skip(1).FirstOrDefault()?.Id ?? adminId,
                    Title = "Family Christmas 2023",
                    Description = "Our annual family gathering around the Christmas tree",
                    ImageUrl = "/uploads/christmas2023.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-40),
                    DateTaken = new DateTime(2023, 12, 25)
                },
                new Photo
                {
                    FamilyId = smithFamily.Id,
                    UploadedByUserId = demoUsers.Skip(5).FirstOrDefault()?.Id ?? adminId,
                    Title = "Golden Anniversary",
                    Description = "50 wonderful years together",
                    ImageUrl = "/uploads/anniversary50.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-10),
                    DateTaken = DateTime.UtcNow.AddDays(-10)
                },
                new Photo
                {
                    FamilyId = royalFamily.Id,
                    UploadedByUserId = adminId,
                    Title = "King Edward I Coronation",
                    Description = "The founding moment of our dynasty - King Edward Royal I's coronation in 1925",
                    ImageUrl = "/uploads/royal/coronation_edward_i.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-120),
                    DateTaken = new DateTime(1925, 6, 15),
                    Location = "Royal Cathedral, Capital City"
                }
            };

            context.Photos.AddRange(photos);
            await context.SaveChangesAsync();

         
            Console.WriteLine("SeedData initialization completed successfully!");
            Console.WriteLine($"Total Families: {await context.Families.CountAsync()}");
            Console.WriteLine($"Total Family Members: {await context.FamilyMembers.CountAsync()}");
            Console.WriteLine($"Total Relationships: {await context.Relationships.CountAsync()}");
            Console.WriteLine($"Total Stories: {await context.Stories.CountAsync()}");
            Console.WriteLine($"Total Photos: {await context.Photos.CountAsync()}");
            Console.WriteLine($"Total Users: {await context.Users.CountAsync()}");
        }
        else
        {

            Console.WriteLine("Database already contains seed data - skipping initialization");
            Console.WriteLine($"Current Families: {await context.Families.CountAsync()}");
            Console.WriteLine($"Current Family Members: {await context.FamilyMembers.CountAsync()}");
            Console.WriteLine($"Current Users: {await context.Users.CountAsync()}");
        }
    }
}