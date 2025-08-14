using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyFamilyTreeNet.Api.DTOs;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace MyFamilyTreeNet.Tests.IntegrationTests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly AppDbContext _context;
    private string? _authToken;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });

        _client = _factory.CreateClient();
        
        // Get the service scope to access the database context
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Ensure the database is created
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task AuthController_Test_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Test endpoint works!");
    }

    [Fact]
    public async Task AuthController_Register_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "integration-test1@example.com",
            Password = "Password123!",
            FirstName = "Test",
            MiddleName = "Middle",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Registration failed with status {response.StatusCode}: {errorContent}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("token");
        content.Should().Contain("user");
    }

    [Fact]
    public async Task AuthController_Register_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "invalid-email",
            Password = "123", // Too short
            FirstName = "",
            MiddleName = "",
            LastName = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AuthController_Login_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange - First register a user
        await RegisterTestUser();

        var loginDto = new LoginDto
        {
            Email = "integration-test2@example.com",
            Password = "Password123!",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("token");
        content.Should().Contain("user");
    }

    [Fact]
    public async Task AuthController_Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword",
            RememberMe = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FamilyController_GetAllFamilies_ReturnsSuccess()
    {
        // Arrange
        await SeedTestFamilies();

        // Act
        var response = await _client.GetAsync("/api/family");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var families = JsonSerializer.Deserialize<List<FamilyDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        families.Should().NotBeEmpty();
    }

    [Fact]
    public async Task FamilyController_GetFamilyById_WithValidId_ReturnsSuccess()
    {
        // Arrange
        await SeedTestFamilies();
        var family = await _context.Families.FirstAsync();

        // Act
        var response = await _client.GetAsync($"/api/family/{family.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(family.Name);
    }

    [Fact]
    public async Task FamilyController_GetFamilyById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/family/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FamilyController_CreateFamily_WithAuthentication_ReturnsSuccess()
    {
        // Arrange
        await AuthenticateAsync();
        var createFamilyDto = new CreateFamilyDto
        {
            Name = "Integration Test Family",
            Description = "Created during integration testing"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/family", createFamilyDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Integration Test Family");
    }

    [Fact]
    public async Task FamilyController_CreateFamily_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var createFamilyDto = new CreateFamilyDto
        {
            Name = "Test Family",
            Description = "Test Description"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/family", createFamilyDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FamilyController_GetFamilyTreeData_WithValidId_ReturnsSuccess()
    {
        // Arrange
        await SeedTestFamilyWithMembers();
        var family = await _context.Families.Include(f => f.FamilyMembers).FirstAsync();

        // Act
        var response = await _client.GetAsync($"/api/family/{family.Id}/tree-data");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("firstName");
    }

    private async Task RegisterTestUser()
    {
        var registerDto = new RegisterDto
        {
            Email = "integration-test2@example.com",
            Password = "Password123!",
            FirstName = "Login",
            MiddleName = "Middle",
            LastName = "Test"
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerDto);
    }

    private async Task AuthenticateAsync()
    {
        if (_authToken != null)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            return;
        }

        var registerDto = new RegisterDto
        {
            Email = "integration-test3@example.com",
            Password = "Password123!",
            FirstName = "Auth",
            MiddleName = "Middle",
            LastName = "Test"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        var content = await response.Content.ReadAsStringAsync();
        
        using var document = JsonDocument.Parse(content);
        _authToken = document.RootElement.GetProperty("token").GetString();
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
    }

    private async Task SeedTestFamilies()
    {
        if (!await _context.Families.AnyAsync())
        {
            var families = new List<Family>
            {
                new Family
                {
                    Name = "Test Family 1",
                    Description = "First test family",
                    CreatedByUserId = "test-user",
                    CreatedAt = DateTime.UtcNow
                },
                new Family
                {
                    Name = "Test Family 2",
                    Description = "Second test family",
                    CreatedByUserId = "test-user",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.Families.AddRange(families);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedTestFamilyWithMembers()
    {
        if (!await _context.Families.AnyAsync())
        {
            var family = new Family
            {
                Name = "Family with Members",
                Description = "Test family with members",
                CreatedByUserId = "test-user",
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
                }
            };

            _context.FamilyMembers.AddRange(members);
            await _context.SaveChangesAsync();
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        _client.Dispose();
    }
}