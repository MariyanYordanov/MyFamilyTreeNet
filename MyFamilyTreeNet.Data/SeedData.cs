using Microsoft.AspNetCore.Identity;
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

            var result = await userManager.CreateAsync(adminUser, "Admin123");
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

                var result = await userManager.CreateAsync(user, "Demo123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                    demoUsers.Add(user);
                }
            }
            else
            {
                demoUsers.Add(await userManager.FindByEmailAsync(email));
            }
        }

        if (!context.Families.Any())
        {
            var adminId = (await userManager.FindByEmailAsync(adminEmail))?.Id ?? "";
            
            var doeFamily = new Family
            {
                Name = "The Doe Family",
                Description = "Welcome to the Doe family tree! We're a loving family spanning three generations.",
                CreatedByUserId = demoUsers[0]?.Id ?? adminId,
                CreatedAt = DateTime.UtcNow
            };

            var smithFamily = new Family
            {
                Name = "The Smith Family",
                Description = "The Smith family legacy - preserving our heritage and memories.",
                CreatedByUserId = demoUsers[4]?.Id ?? adminId,
                CreatedAt = DateTime.UtcNow
            };

            var royalFamily = new Family
            {
                Name = "The Royal Family Tree",
                Description = "A complex family demonstrating all possible relationship types including cousin marriages, step-relationships, and multiple generations.",
                CreatedByUserId = adminId,
                CreatedAt = DateTime.UtcNow
            };

            context.Families.AddRange(doeFamily, smithFamily, royalFamily);
            await context.SaveChangesAsync();

            // Add family members
            var familyMembers = new List<FamilyMember>
            {
                // Doe family members
                new FamilyMember { FamilyId = doeFamily.Id,  FirstName = "John", MiddleName = "Demo", LastName = "Doe", DateOfBirth = new DateTime(1975, 5, 15), Gender = Gender.Male,  AddedByUserId = adminId,  },
                new FamilyMember { FamilyId = doeFamily.Id,  FirstName = "Jane", MiddleName = "Marie", LastName = "Doe", DateOfBirth = new DateTime(1978, 8, 22), Gender = Gender.Female,  AddedByUserId = adminId,  },
                new FamilyMember { FamilyId = doeFamily.Id,  FirstName = "Michael", MiddleName = "James", LastName = "Doe", DateOfBirth = new DateTime(2000, 3, 10), Gender = Gender.Male,  AddedByUserId = adminId,  },
                new FamilyMember { FamilyId = doeFamily.Id,  FirstName = "Sarah", MiddleName = "Elizabeth", LastName = "Doe", DateOfBirth = new DateTime(2002, 11, 5), Gender = Gender.Female,  AddedByUserId = adminId,  },
                
                // Smith family members
                new FamilyMember { FamilyId = smithFamily.Id,  FirstName = "Robert", MiddleName = "William", LastName = "Smith", DateOfBirth = new DateTime(1950, 2, 28), Gender = Gender.Male,  AddedByUserId = adminId,  },
                new FamilyMember { FamilyId = smithFamily.Id,  FirstName = "Mary", MiddleName = "Ann", LastName = "Smith", DateOfBirth = new DateTime(1952, 7, 14), Gender = Gender.Female,  AddedByUserId = adminId,  }
            };

            // Royal Family members - complex multi-generational tree
            var royalMembers = new List<FamilyMember>
            {
                // Generation 1 - Great-Grandparents (Founders)
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "King", MiddleName = "Edward", LastName = "Royal I", DateOfBirth = new DateTime(1900, 1, 1), DateOfDeath = new DateTime(1980, 1, 1), Gender = Gender.Male, Biography = "Founder of the Royal dynasty. Established the family kingdom.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Queen", MiddleName = "Elizabeth", LastName = "Royal I", DateOfBirth = new DateTime(1905, 3, 15), DateOfDeath = new DateTime(1985, 3, 15), Gender = Gender.Female, Biography = "Beloved matriarch. Known for her wisdom and diplomatic skills.", AddedByUserId = adminId },
                
                // Generation 1b - Great-Grandparents from another branch
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Duke", MiddleName = "William", LastName = "Noble", DateOfBirth = new DateTime(1898, 6, 10), DateOfDeath = new DateTime(1978, 6, 10), Gender = Gender.Male, Biography = "Head of the Noble house. Allied with the Royal family.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Duchess", MiddleName = "Margaret", LastName = "Noble", DateOfBirth = new DateTime(1902, 9, 20), DateOfDeath = new DateTime(1982, 9, 20), Gender = Gender.Female, Biography = "Famous for her charitable work and patronage of the arts.", AddedByUserId = adminId },
                
                // Generation 2 - Grandparents
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Prince", MiddleName = "Charles", LastName = "Royal", DateOfBirth = new DateTime(1925, 4, 21), DateOfDeath = new DateTime(2005, 4, 21), Gender = Gender.Male, Biography = "Eldest son of King Edward I. Military hero and statesman.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Princess", MiddleName = "Diana", LastName = "Noble-Royal", DateOfBirth = new DateTime(1930, 7, 12), DateOfDeath = new DateTime(2010, 7, 12), Gender = Gender.Female, Biography = "Daughter of Duke William. Married Prince Charles, creating alliance.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Prince", MiddleName = "Andrew", LastName = "Royal", DateOfBirth = new DateTime(1928, 11, 3), DateOfDeath = new DateTime(2008, 11, 3), Gender = Gender.Male, Biography = "Second son of King Edward I. Known for his adventures and explorations.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Lady", MiddleName = "Catherine", LastName = "Windsor", DateOfBirth = new DateTime(1932, 2, 14), Gender = Gender.Female, Biography = "From the Windsor family. Married Prince Andrew.", AddedByUserId = adminId },
                
                // Generation 3 - Parents
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "King", MiddleName = "William", LastName = "Royal II", DateOfBirth = new DateTime(1950, 8, 15), Gender = Gender.Male, Biography = "Current reigning monarch. Son of Prince Charles and Princess Diana.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Queen", MiddleName = "Isabella", LastName = "Cambridge-Royal", DateOfBirth = new DateTime(1955, 12, 25), Gender = Gender.Female, Biography = "Queen consort. From the Cambridge noble family.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Prince", MiddleName = "Henry", LastName = "Royal", DateOfBirth = new DateTime(1953, 6, 8), Gender = Gender.Male, Biography = "Brother of King William II. Military officer and diplomat.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Princess", MiddleName = "Sophie", LastName = "Royal", DateOfBirth = new DateTime(1958, 10, 30), Gender = Gender.Female, Biography = "Wife of Prince Henry. Known for humanitarian work.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Duke", MiddleName = "James", LastName = "Royal", DateOfBirth = new DateTime(1951, 3, 18), Gender = Gender.Male, Biography = "Son of Prince Andrew. Cousin to King William II.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Duchess", MiddleName = "Emma", LastName = "Royal", DateOfBirth = new DateTime(1954, 5, 22), Gender = Gender.Female, Biography = "Wife of Duke James. Former ambassador.", AddedByUserId = adminId },
                
                // Generation 4 - Current generation (children/cousins)
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Crown Prince", MiddleName = "Alexander", LastName = "Royal", DateOfBirth = new DateTime(1975, 2, 14), Gender = Gender.Male, Biography = "Heir to the throne. Son of King William II and Queen Isabella.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Princess", MiddleName = "Victoria", LastName = "Royal", DateOfBirth = new DateTime(1977, 9, 3), Gender = Gender.Female, Biography = "Daughter of King William II. Active in environmental causes.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Prince", MiddleName = "George", LastName = "Royal", DateOfBirth = new DateTime(1980, 11, 11), Gender = Gender.Male, Biography = "Youngest son of King William II. Entrepreneur and philanthropist.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Lord", MiddleName = "Thomas", LastName = "Royal", DateOfBirth = new DateTime(1976, 7, 20), Gender = Gender.Male, Biography = "Son of Prince Henry. Cousin to Crown Prince Alexander.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Lady", MiddleName = "Charlotte", LastName = "Royal", DateOfBirth = new DateTime(1978, 4, 8), Gender = Gender.Female, Biography = "Daughter of Prince Henry. Acclaimed artist and cultural patron.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Earl", MiddleName = "Frederick", LastName = "Royal", DateOfBirth = new DateTime(1974, 12, 1), Gender = Gender.Male, Biography = "Son of Duke James. Second cousin to Crown Prince Alexander.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Countess", MiddleName = "Olivia", LastName = "Royal", DateOfBirth = new DateTime(1979, 6, 18), Gender = Gender.Female, Biography = "Daughter of Duke James. Renowned scientist and researcher.", AddedByUserId = adminId },
                
                // Generation 4b - Spouses (showing marriages between cousins)
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Crown Princess", MiddleName = "Sophia", LastName = "Habsburg-Royal", DateOfBirth = new DateTime(1978, 8, 25), Gender = Gender.Female, Biography = "Wife of Crown Prince Alexander. From Austrian Habsburg family.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Duke", MiddleName = "Nicholas", LastName = "Bourbon", DateOfBirth = new DateTime(1975, 1, 30), Gender = Gender.Male, Biography = "Husband of Princess Victoria. From French Bourbon family.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Lady", MiddleName = "Beatrice", LastName = "York-Royal", DateOfBirth = new DateTime(1981, 5, 12), Gender = Gender.Female, Biography = "Wife of Prince George. From York noble family.", AddedByUserId = adminId },
                
                // Generation 5 - Children/Next generation
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Prince", MiddleName = "Edward", LastName = "Royal III", DateOfBirth = new DateTime(2000, 3, 21), Gender = Gender.Male, Biography = "Future king. Son of Crown Prince Alexander and Crown Princess Sophia.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Princess", MiddleName = "Elizabeth", LastName = "Royal II", DateOfBirth = new DateTime(2002, 7, 4), Gender = Gender.Female, Biography = "Daughter of Crown Prince Alexander. Talented equestrian.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Lord", MiddleName = "Louis", LastName = "Bourbon-Royal", DateOfBirth = new DateTime(2001, 10, 15), Gender = Gender.Male, Biography = "Son of Princess Victoria and Duke Nicholas.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Lady", MiddleName = "Marie", LastName = "Bourbon-Royal", DateOfBirth = new DateTime(2003, 12, 8), Gender = Gender.Female, Biography = "Daughter of Princess Victoria. Aspiring musician.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Prince", MiddleName = "Philip", LastName = "Royal", DateOfBirth = new DateTime(2005, 2, 28), Gender = Gender.Male, Biography = "Son of Prince George and Lady Beatrice.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Lord", MiddleName = "David", LastName = "Royal", DateOfBirth = new DateTime(2004, 9, 14), Gender = Gender.Male, Biography = "Son of Lord Thomas. Grandnephew of King William II.", AddedByUserId = adminId },
                new FamilyMember { FamilyId = royalFamily.Id, FirstName = "Lady", MiddleName = "Grace", LastName = "Royal", DateOfBirth = new DateTime(2006, 11, 22), Gender = Gender.Female, Biography = "Daughter of Lady Charlotte. Young prodigy in mathematics.", AddedByUserId = adminId }
            };

            familyMembers.AddRange(royalMembers);

            context.FamilyMembers.AddRange(familyMembers);
            await context.SaveChangesAsync();

            // Add relationships
            var relationships = new List<Relationship>
            {
                // Doe family relationships
                new Relationship { PrimaryMemberId = familyMembers[0].Id, RelatedMemberId = familyMembers[1].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = demoUsers[0].Id },
                new Relationship { PrimaryMemberId = familyMembers[0].Id, RelatedMemberId = familyMembers[2].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = demoUsers[0].Id },
                new Relationship { PrimaryMemberId = familyMembers[0].Id, RelatedMemberId = familyMembers[3].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = demoUsers[0].Id },
                new Relationship { PrimaryMemberId = familyMembers[1].Id, RelatedMemberId = familyMembers[2].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = demoUsers[1].Id },
                new Relationship { PrimaryMemberId = familyMembers[1].Id, RelatedMemberId = familyMembers[3].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = demoUsers[1].Id },
                new Relationship { PrimaryMemberId = familyMembers[2].Id, RelatedMemberId = familyMembers[3].Id, RelationshipType = RelationshipType.Sibling, CreatedByUserId = demoUsers[2].Id },
                
                // Smith family relationships
                new Relationship { PrimaryMemberId = familyMembers[4].Id, RelatedMemberId = familyMembers[5].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = demoUsers[4].Id }
            };

            // Royal Family relationships - complex multi-generational structure
            // Members start at index 6 (after Doe family [0-3] and Smith family [4-5])
            var royalRelationships = new List<Relationship>();
            
            // Generation 1 - Great-Grandparents marriages
            // King Edward I (6) ♥ Queen Elizabeth I (7)
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[6].Id, RelatedMemberId = familyMembers[7].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId });
            // Duke William (8) ♥ Duchess Margaret (9)
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[8].Id, RelatedMemberId = familyMembers[9].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId });
            
            // Generation 2 - Grandparents relationships
            // Prince Charles (10) ♥ Princess Diana (11) - cousin marriage (Diana is daughter of Duke William)
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[10].Id, RelatedMemberId = familyMembers[11].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId });
            // Prince Andrew (12) ♥ Lady Catherine (13)
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[12].Id, RelatedMemberId = familyMembers[13].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId });
            
            // Parent-child relationships Generation 1 → 2
            // King Edward I → Prince Charles & Prince Andrew
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[6].Id, RelatedMemberId = familyMembers[10].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[6].Id, RelatedMemberId = familyMembers[12].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Queen Elizabeth I → Prince Charles & Prince Andrew
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[7].Id, RelatedMemberId = familyMembers[10].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[7].Id, RelatedMemberId = familyMembers[12].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Duke William → Princess Diana
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[8].Id, RelatedMemberId = familyMembers[11].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[9].Id, RelatedMemberId = familyMembers[11].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            
            // Siblings Generation 2
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[10].Id, RelatedMemberId = familyMembers[12].Id, RelationshipType = RelationshipType.Sibling, CreatedByUserId = adminId });
            
            // Generation 3 - Parents marriages
            // King William II (14) ♥ Queen Isabella (15)
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[14].Id, RelatedMemberId = familyMembers[15].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId });
            // Prince Henry (16) ♥ Princess Sophie (17)
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[16].Id, RelatedMemberId = familyMembers[17].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId });
            // Duke James (18) ♥ Duchess Emma (19)
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[18].Id, RelatedMemberId = familyMembers[19].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId });
            
            // Parent-child relationships Generation 2 → 3
            // Prince Charles → King William II & Prince Henry
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[10].Id, RelatedMemberId = familyMembers[14].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[10].Id, RelatedMemberId = familyMembers[16].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Princess Diana → King William II & Prince Henry
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[11].Id, RelatedMemberId = familyMembers[14].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[11].Id, RelatedMemberId = familyMembers[16].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Prince Andrew → Duke James
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[12].Id, RelatedMemberId = familyMembers[18].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[13].Id, RelatedMemberId = familyMembers[18].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            
            // Siblings Generation 3
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[14].Id, RelatedMemberId = familyMembers[16].Id, RelationshipType = RelationshipType.Sibling, CreatedByUserId = adminId });
            // Cousins Generation 3 (King William II and Duke James are cousins)
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[14].Id, RelatedMemberId = familyMembers[18].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            
            // Generation 4 - Current generation marriages
            // Crown Prince Alexander (20) ♥ Crown Princess Sophia (27)
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[20].Id, RelatedMemberId = familyMembers[27].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId });
            // Princess Victoria (21) ♥ Duke Nicholas (28)
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[21].Id, RelatedMemberId = familyMembers[28].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId });
            // Prince George (22) ♥ Lady Beatrice (29)
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[22].Id, RelatedMemberId = familyMembers[29].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId });
            
            // Parent-child relationships Generation 3 → 4
            // King William II → Crown Prince Alexander, Princess Victoria, Prince George
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[14].Id, RelatedMemberId = familyMembers[20].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[14].Id, RelatedMemberId = familyMembers[21].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[14].Id, RelatedMemberId = familyMembers[22].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Queen Isabella → Crown Prince Alexander, Princess Victoria, Prince George
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[15].Id, RelatedMemberId = familyMembers[20].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[15].Id, RelatedMemberId = familyMembers[21].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[15].Id, RelatedMemberId = familyMembers[22].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Prince Henry → Lord Thomas, Lady Charlotte
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[16].Id, RelatedMemberId = familyMembers[23].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[16].Id, RelatedMemberId = familyMembers[24].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Princess Sophie → Lord Thomas, Lady Charlotte
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[17].Id, RelatedMemberId = familyMembers[23].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[17].Id, RelatedMemberId = familyMembers[24].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Duke James → Earl Frederick, Countess Olivia
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[18].Id, RelatedMemberId = familyMembers[25].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[18].Id, RelatedMemberId = familyMembers[26].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Duchess Emma → Earl Frederick, Countess Olivia
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[19].Id, RelatedMemberId = familyMembers[25].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[19].Id, RelatedMemberId = familyMembers[26].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            
            // Siblings Generation 4
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[20].Id, RelatedMemberId = familyMembers[21].Id, RelationshipType = RelationshipType.Sibling, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[20].Id, RelatedMemberId = familyMembers[22].Id, RelationshipType = RelationshipType.Sibling, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[21].Id, RelatedMemberId = familyMembers[22].Id, RelationshipType = RelationshipType.Sibling, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[23].Id, RelatedMemberId = familyMembers[24].Id, RelationshipType = RelationshipType.Sibling, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[25].Id, RelatedMemberId = familyMembers[26].Id, RelationshipType = RelationshipType.Sibling, CreatedByUserId = adminId });
            
            // Cousins Generation 4 (first cousins)
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[20].Id, RelatedMemberId = familyMembers[23].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[20].Id, RelatedMemberId = familyMembers[24].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[21].Id, RelatedMemberId = familyMembers[23].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[21].Id, RelatedMemberId = familyMembers[24].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[22].Id, RelatedMemberId = familyMembers[23].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[22].Id, RelatedMemberId = familyMembers[24].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            
            // Second cousins Generation 4 (Earl Frederick and Countess Olivia with Crown Prince Alexander etc.)
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[20].Id, RelatedMemberId = familyMembers[25].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[20].Id, RelatedMemberId = familyMembers[26].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            
            // SPECIAL RELATIONSHIPS - Marriages between relatives (common in royal families)
            // Lady Charlotte (24) married to Earl Frederick (25) - they are second cousins
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[24].Id, RelatedMemberId = familyMembers[25].Id, RelationshipType = RelationshipType.Spouse, CreatedByUserId = adminId });
            // This creates a double relationship - they are both cousins AND spouses
            
            // Generation 5 - Children relationships
            // Parent-child relationships Generation 4 → 5
            // Crown Prince Alexander → Prince Edward III, Princess Elizabeth II
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[20].Id, RelatedMemberId = familyMembers[30].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[20].Id, RelatedMemberId = familyMembers[31].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Crown Princess Sophia → Prince Edward III, Princess Elizabeth II
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[27].Id, RelatedMemberId = familyMembers[30].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[27].Id, RelatedMemberId = familyMembers[31].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Princess Victoria → Lord Louis, Lady Marie
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[21].Id, RelatedMemberId = familyMembers[32].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[21].Id, RelatedMemberId = familyMembers[33].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Duke Nicholas → Lord Louis, Lady Marie
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[28].Id, RelatedMemberId = familyMembers[32].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[28].Id, RelatedMemberId = familyMembers[33].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Prince George → Prince Philip
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[22].Id, RelatedMemberId = familyMembers[34].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[29].Id, RelatedMemberId = familyMembers[34].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Lord Thomas → Lord David
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[23].Id, RelatedMemberId = familyMembers[35].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            // Lady Charlotte → Lady Grace
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[24].Id, RelatedMemberId = familyMembers[36].Id, RelationshipType = RelationshipType.Parent, CreatedByUserId = adminId });
            
            // Siblings Generation 5
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[30].Id, RelatedMemberId = familyMembers[31].Id, RelationshipType = RelationshipType.Sibling, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[32].Id, RelatedMemberId = familyMembers[33].Id, RelationshipType = RelationshipType.Sibling, CreatedByUserId = adminId });
            
            // Cousins Generation 5
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[30].Id, RelatedMemberId = familyMembers[32].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[30].Id, RelatedMemberId = familyMembers[33].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[30].Id, RelatedMemberId = familyMembers[34].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[31].Id, RelatedMemberId = familyMembers[32].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[31].Id, RelatedMemberId = familyMembers[33].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            royalRelationships.Add(new Relationship { PrimaryMemberId = familyMembers[31].Id, RelatedMemberId = familyMembers[34].Id, RelationshipType = RelationshipType.Cousin, CreatedByUserId = adminId });
            
            relationships.AddRange(royalRelationships);

            context.Relationships.AddRange(relationships);
            await context.SaveChangesAsync();

            // Add sample stories
            var stories = new List<Story>
            {
                new Story
                {
                    FamilyId = doeFamily.Id,
                    AuthorUserId = demoUsers[0].Id,
                    Title = "Our Wedding Day",
                    Content = "It was a beautiful summer day when Jane and I tied the knot. Friends and family gathered to celebrate our love...",
                    CreatedAt = DateTime.UtcNow.AddDays(-365)
                },
                new Story
                {
                    FamilyId = doeFamily.Id,
                    AuthorUserId = demoUsers[2].Id,
                    Title = "My First Day at College",
                    Content = "I still remember the excitement and nervousness I felt stepping onto campus for the first time...",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Story
                {
                    FamilyId = smithFamily.Id,
                    AuthorUserId = demoUsers[4].Id,
                    Title = "50 Years Together",
                    Content = "Mary and I celebrated our golden anniversary surrounded by our children and grandchildren. What a journey it has been...",
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                
                // Royal Family Stories
                new Story
                {
                    FamilyId = royalFamily.Id,
                    AuthorUserId = adminId,
                    Title = "The Foundation of Our Dynasty",
                    Content = "King Edward Royal I established our family's reign in 1900. Born during turbulent times, he united the kingdom through wisdom and strength. His marriage to Queen Elizabeth I brought together two powerful noble houses, creating the foundation of our enduring dynasty. Their love story became legendary - she was not only his queen but his closest advisor and confidante.",
                    CreatedAt = DateTime.UtcNow.AddDays(-100)
                },
                new Story
                {
                    FamilyId = royalFamily.Id,
                    AuthorUserId = adminId,
                    Title = "The Great Alliance",
                    Content = "The marriage between Prince Charles and Princess Diana in 1955 was more than a union of hearts - it was the merging of the Royal and Noble bloodlines. Diana, daughter of Duke William Noble, brought her family's ancient heritage into our line. Their wedding was attended by royalty from across Europe, cementing alliances that would last generations.",
                    CreatedAt = DateTime.UtcNow.AddDays(-80)
                },
                new Story
                {
                    FamilyId = royalFamily.Id,
                    AuthorUserId = adminId,
                    Title = "A Modern Monarchy",
                    Content = "King William Royal II ascended to the throne in 1975, bringing fresh perspectives to an ancient institution. His coronation marked a new era of openness and connection with the people. Queen Isabella, with her diplomatic background, helped modernize the monarchy while preserving its cherished traditions.",
                    CreatedAt = DateTime.UtcNow.AddDays(-50)
                },
                new Story
                {
                    FamilyId = royalFamily.Id,
                    AuthorUserId = adminId,
                    Title = "The Royal Wedding of the Century",
                    Content = "Crown Prince Alexander's wedding to Princess Sophia Habsburg in 2000 was hailed as the wedding of the century. The ceremony united the Royal family with the ancient Habsburg line of Austria. Thousands lined the streets to witness the procession, and the celebration lasted for three days across the kingdom.",
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new Story
                {
                    FamilyId = royalFamily.Id,
                    AuthorUserId = adminId,
                    Title = "The Controversial Union",
                    Content = "The marriage between Lady Charlotte and Earl Frederick raised eyebrows across the court - they were second cousins, continuing the royal tradition of keeping bloodlines close. While some criticized the union, their love was genuine and their partnership became one of the strongest in the family. Their marriage strengthened ties between different branches of the royal house.",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Story
                {
                    FamilyId = royalFamily.Id,
                    AuthorUserId = adminId,
                    Title = "The Future King",
                    Content = "Prince Edward Royal III, born in 2000, represents the future of our monarchy. From a young age, he has shown the wisdom of his great-great-grandfather King Edward I and the compassion of his mother, Crown Princess Sophia. His education spans multiple continents, preparing him for the global challenges of modern leadership.",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };

            context.Stories.AddRange(stories);
            await context.SaveChangesAsync();

            // Add sample photos
            var photos = new List<Photo>
            {
                new Photo
                {
                    FamilyId = doeFamily.Id,
                    UploadedByUserId = demoUsers[1].Id,
                    Title = "Family Christmas 2023",
                    Description = "Our annual family gathering around the Christmas tree",
                    ImageUrl = "/uploads/christmas2023.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-40),
                    DateTaken = new DateTime(2023, 12, 25)
                },
                new Photo
                {
                    FamilyId = doeFamily.Id,
                    UploadedByUserId = demoUsers[3].Id,
                    Title = "Sarah's Graduation",
                    Description = "So proud of our daughter!",
                    ImageUrl = "/uploads/graduation2024.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-60),
                    DateTaken = new DateTime(2024, 5, 15)
                },
                new Photo
                {
                    FamilyId = smithFamily.Id,
                    UploadedByUserId = demoUsers[5].Id,
                    Title = "Golden Anniversary",
                    Description = "50 wonderful years together",
                    ImageUrl = "/uploads/anniversary50.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-10),
                    DateTaken = DateTime.UtcNow.AddDays(-10)
                },
                
                // Royal Family Photos
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
                },
                new Photo
                {
                    FamilyId = royalFamily.Id,
                    UploadedByUserId = adminId,
                    Title = "The Royal Wedding 1955",
                    Description = "Prince Charles and Princess Diana's wedding ceremony - the great alliance",
                    ImageUrl = "/uploads/royal/wedding_charles_diana.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-90),
                    DateTaken = new DateTime(1955, 4, 21),
                    Location = "St. Royal's Cathedral"
                },
                new Photo
                {
                    FamilyId = royalFamily.Id,
                    UploadedByUserId = adminId,
                    Title = "King William II Coronation",
                    Description = "The modernizing monarch takes the throne - a new era begins",
                    ImageUrl = "/uploads/royal/coronation_william_ii.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-60),
                    DateTaken = new DateTime(1975, 8, 15),
                    Location = "Royal Cathedral, Capital City"
                },
                new Photo
                {
                    FamilyId = royalFamily.Id,
                    UploadedByUserId = adminId,
                    Title = "Crown Prince Alexander's Wedding",
                    Description = "The wedding of the century - uniting Royal and Habsburg bloodlines",
                    ImageUrl = "/uploads/royal/wedding_alexander_sophia.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-30),
                    DateTaken = new DateTime(2000, 5, 12),
                    Location = "Royal Palace Gardens"
                },
                new Photo
                {
                    FamilyId = royalFamily.Id,
                    UploadedByUserId = adminId,
                    Title = "Royal Family Portrait 2020",
                    Description = "Five generations together - a rare and precious moment",
                    ImageUrl = "/uploads/royal/family_portrait_2020.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-20),
                    DateTaken = new DateTime(2020, 12, 25),
                    Location = "Royal Palace Throne Room"
                },
                new Photo
                {
                    FamilyId = royalFamily.Id,
                    UploadedByUserId = adminId,
                    Title = "The Controversial Wedding",
                    Description = "Lady Charlotte and Earl Frederick - love transcends bloodlines",
                    ImageUrl = "/uploads/royal/wedding_charlotte_frederick.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-18),
                    DateTaken = new DateTime(2018, 9, 8),
                    Location = "Royal Private Chapel"
                },
                new Photo
                {
                    FamilyId = royalFamily.Id,
                    UploadedByUserId = adminId,
                    Title = "Prince Edward III First Official Portrait",
                    Description = "The future king at age 21 - dignity and wisdom beyond his years",
                    ImageUrl = "/uploads/royal/edward_iii_portrait.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-5),
                    DateTaken = new DateTime(2021, 3, 21),
                    Location = "Royal Portrait Studio"
                },
                new Photo
                {
                    FamilyId = royalFamily.Id,
                    UploadedByUserId = adminId,
                    Title = "Royal Children Summer 2023",
                    Description = "The next generation at play - Princess Elizabeth II, Lord Louis, Lady Marie, and Prince Philip",
                    ImageUrl = "/uploads/royal/children_summer_2023.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-3),
                    DateTaken = new DateTime(2023, 7, 15),
                    Location = "Royal Estate Gardens"
                }
            };

            context.Photos.AddRange(photos);
            await context.SaveChangesAsync();
        }
    }
}