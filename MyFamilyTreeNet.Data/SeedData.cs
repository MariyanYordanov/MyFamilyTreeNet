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
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                MiddleName = "System",
                LastName = "User",
                DateOfBirth = new DateTime(1990, 1, 1),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                // Admin user created successfully
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    // Error creating admin: {error.Description}
                }
            }
        }
        else
        {
            // Admin user already exists
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

                var createResult = await userManager.CreateAsync(user, "Demo123!");
                if (createResult.Succeeded)
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
            var foundAdminUser = await userManager.FindByEmailAsync(adminEmail);
            var adminId = foundAdminUser?.Id ?? "";
            
            var doeFamily = new Family
            {
                Name = "Семейство Доу",
                Description = "Добре дошли в родословното дърво на семейство Доу! Ние сме любящо семейство, обхващащо три поколения.",
                CreatedByUserId = demoUsers.FirstOrDefault()?.Id ?? adminId,
                CreatedAt = DateTime.UtcNow,
                IsPublic = true
            };

            var smithFamily = new Family
            {
                Name = "Семейство Смит",
                Description = "Наследството на семейство Смит - съхраняване на нашето наследство и спомени.",
                CreatedByUserId = demoUsers.Skip(4).FirstOrDefault()?.Id ?? adminId,
                CreatedAt = DateTime.UtcNow,
                IsPublic = true
            };

            var royalFamily = new Family
            {
                Name = "Кралското родословно дърво",
                Description = "Сложно семейство с 9 членове, демонстриращо всички възможни видове връзки, включително бракове между братовчеди, мащехи/заварени деца и множество поколения.",
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
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Крал", MiddleName = "Едуард", LastName = "Роял I", DateOfBirth = new DateTime(1900, 1, 1), DateOfDeath = new DateTime(1980, 1, 1), Gender = Gender.Male, Biography = "Основател на династията Роял. Създал семейното кралство.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Кралица", MiddleName = "Елизабет", LastName = "Роял I", DateOfBirth = new DateTime(1905, 3, 15), DateOfDeath = new DateTime(1985, 3, 15), Gender = Gender.Female, Biography = "Обичана матриарх. Известна със своята мъдрост и дипломатически умения.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                

                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Принц", MiddleName = "Чарлз", LastName = "Роял", DateOfBirth = new DateTime(1925, 4, 21), DateOfDeath = new DateTime(2005, 4, 21), Gender = Gender.Male, Biography = "Най-големият син на крал Едуард I. Военен герой и държавник.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Принцеса", MiddleName = "Даяна", LastName = "Нобъл-Роял", DateOfBirth = new DateTime(1930, 7, 12), DateOfDeath = new DateTime(2010, 7, 12), Gender = Gender.Female, Biography = "Омъжена за принц Чарлз, създавайки съюз.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                

                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Крал", MiddleName = "Уилям", LastName = "Роял II", DateOfBirth = new DateTime(1950, 8, 15), Gender = Gender.Male, Biography = "Настоящ управляващ монарх. Син на принц Чарлз и принцеса Даяна.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Кралица", MiddleName = "Изабела", LastName = "Кеймбридж-Роял", DateOfBirth = new DateTime(1955, 12, 25), Gender = Gender.Female, Biography = "Кралица консорт. От благородническото семейство Кеймбридж.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                
             
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Престолонаследник", MiddleName = "Александър", LastName = "Роял", DateOfBirth = new DateTime(1975, 2, 14), Gender = Gender.Male, Biography = "Наследник на трона. Син на крал Уилям II и кралица Изабела.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Престолонаследничка", MiddleName = "София", LastName = "Хабсбург-Роял", DateOfBirth = new DateTime(1978, 8, 25), Gender = Gender.Female, Biography = "Съпруга на престолонаследник Александър. От австрийското семейство Хабсбург.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow },
                
              
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Принц", MiddleName = "Едуард", LastName = "Роял III", DateOfBirth = new DateTime(2000, 3, 21), Gender = Gender.Male, Biography = "Бъдещ крал. Син на престолонаследник Александър и престолонаследничка София.", AddedByUserId = adminId, CreatedAt = DateTime.UtcNow }
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
                    Title = "Нашият сватбен ден",
                    Content = "Беше прекрасен летен ден, когато с Джейн си казахме „Да“. Приятели и семейство се събраха, за да празнуват нашата любов...",
                    CreatedAt = DateTime.UtcNow.AddDays(-365)
                },
                new Story
                {
                    FamilyId = smithFamily.Id,
                    AuthorUserId = demoUsers.Skip(4).FirstOrDefault()?.Id ?? adminId,
                    Title = "50 години заедно",
                    Content = "С Мери отпразнувахме златната си сватба, заобиколени от нашите деца и внуци. Какво пътешествие беше това...",
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new Story
                {
                    FamilyId = royalFamily.Id,
                    AuthorUserId = adminId,
                    Title = "Основаването на нашата династия",
                    Content = "Крал Едуард Роял I установи управлението на нашето семейство през 1900 г. Роден в бурни времена, той обедини кралството чрез мъдрост и сила.",
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
                    Title = "Семейна Коледа 2023",
                    Description = "Нашето годишно семейно събиране около коледната елха",
                    ImageUrl = "/uploads/christmas2023.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-40),
                    DateTaken = new DateTime(2023, 12, 25)
                },
                new Photo
                {
                    FamilyId = smithFamily.Id,
                    UploadedByUserId = demoUsers.Skip(5).FirstOrDefault()?.Id ?? adminId,
                    Title = "Златна сватба",
                    Description = "50 чудесни години заедно",
                    ImageUrl = "/uploads/anniversary50.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-10),
                    DateTaken = DateTime.UtcNow.AddDays(-10)
                },
                new Photo
                {
                    FamilyId = royalFamily.Id,
                    UploadedByUserId = adminId,
                    Title = "Коронясване на крал Едуард I",
                    Description = "Основополагащият момент на нашата династия - коронясването на крал Едуард Роял I през 1925 г.",
                    ImageUrl = "/uploads/royal/coronation_edward_i.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-120),
                    DateTaken = new DateTime(1925, 6, 15),
                    Location = "Кралска катедрала, Столица"
                }
            };

            context.Photos.AddRange(photos);
            await context.SaveChangesAsync();

         
            // Seed data initialized successfully
            // Families count logged
            // Members count logged
            // Relationships count logged
            // Stories count logged
            // Photos count logged
            // Users count logged
        }
        else
        {

            // Database already contains seed data - skipping initialization
            // Current families count logged
            // Current members count logged
            // Current users count logged
        }
    }
}