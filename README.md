# MyFamilyTreeNet - Family Social Network

## Description

MyFamilyTreeNet is a comprehensive family social network that allows users to create and manage family trees, share family stories, photos, and connect with other families. The project consists of an ASP.NET Core Web API with integrated MVC interface.

## Project Architecture

```
MyFamilyTreeNet/
├── MyFamilyTreeNet.Api/          # ASP.NET Core Web API + MVC
│   ├── Controllers/              # API and MVC controllers
│   ├── Areas/Admin/              # Admin panel
│   ├── Views/                    # MVC views
│   ├── Services/                 # Business logic
│   ├── DTOs/                     # Data Transfer Objects
│   └── Middleware/               # Custom middleware
├── MyFamilyTreeNet.Data/         # Entity Framework models
│   ├── Models/                   # Entity models
│   ├── Migrations/               # Database migrations
│   └── SeedData.cs               # Initial data
└── MyFamilyTreeNet.Tests/        # Unit tests
```

## Features

### User Management
- User registration and login with password validation
- User roles: User and Administrator
- User profiles with personal information
- Account lockout functionality

### Family Management
- Create families with descriptions and privacy settings
- Add family members with biographical information
- Define relationships between family members
- Family tree visualization
- Upload family crests

### Content Management
- Upload and manage family photos
- Create family stories with rich content
- View family history and relationships

### Search and Navigation
- Search families by name
- Browse public family profiles
- Interactive family tree display

### Administration
- Admin dashboard with statistics
- User management with ban/unban functionality
- System monitoring and user oversight

## Technologies

### Backend (ASP.NET Core 8.0)
- Entity Framework Core with SQLite
- ASP.NET Core Identity for authentication
- JWT tokens for API security
- Custom middleware for security and error handling
- D3.js for family tree visualization

### Database
- SQLite for development
- Entity Framework Core migrations
- Seed data for initial setup

## Prerequisites

Before running the project, ensure you have:

- .NET 8.0 SDK or newer
- Git for cloning the repository

## Installation and Setup

### Clone the Repository

```bash
git clone <repository-url>
cd MyFamilyTreeNet
```

### Run the Application

```bash
# Navigate to the API directory
cd MyFamilyTreeNet.Api

# Restore NuGet packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run
```

The application will be available at: `https://localhost:5001` and `http://localhost:5000`

## Test Accounts

After starting the application, you can use these test accounts:

### Administrator
- Email: `admin@myfamilytreenet.com`
- Password: `Admin123!`
- Role: Administrator

### Demo Users
- Email: `john@demo.com` | Password: `Demo123!`
- Email: `jane@demo.com` | Password: `Demo123!`
- Role: User

## API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout

### Families
- `GET /api/family` - Get all families (public)
- `GET /api/family/{id}` - Get family details
- `POST /api/family` - Create family (authenticated)
- `PUT /api/family/{id}` - Update family (authenticated)
- `DELETE /api/family/{id}` - Delete family (authenticated)

### Family Members
- `GET /api/member/family/{familyId}` - Get members by family
- `GET /api/member/{id}` - Get member details
- `POST /api/member` - Add member (authenticated)
- `PUT /api/member/{id}` - Update member (authenticated)
- `DELETE /api/member/{id}` - Delete member (authenticated)

### Stories
- `GET /api/story` - Get all stories
- `POST /api/story` - Create story (authenticated)

### Administration
- `GET /Admin/Users` - User management (Admin only)
- `POST /Admin/Users/ToggleLock` - Ban/unban user (Admin only)

Complete API documentation is available at: `https://localhost:5001/swagger`

## Web Interface

The application provides both API endpoints and a traditional MVC web interface:

- `/` - Home page
- `/AccountMvc/Login` - Login page
- `/AccountMvc/Register` - Registration page
- `/FamilyMvc` - Family management
- `/MemberMvc` - Member management
- `/StoryMvc/Create` - Create family stories
- `/Admin` - Admin panel (admin only)

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=my_family_tree_net.db"
  },
  "JwtSettings": {
    "SecretKey": "YourSecretKeyHere",
    "Issuer": "MyFamilyTreeNetApi",
    "Audience": "MyFamilyTreeNetClients",
    "ExpirationMinutes": 1440
  }
}
```

## Testing

### Run Backend Tests
```bash
cd MyFamilyTreeNet.Tests
dotnet test
```

## Key Features Implemented

- User registration and authentication
- Family creation and management
- Family member management with relationships
- Interactive family tree visualization
- Family photo/crest upload
- Family story creation
- Admin panel with user management
- User ban/unban functionality
- Responsive web interface
- API with Swagger documentation
- Unit tests coverage

## Security Features

- JWT token authentication
- Role-based authorization
- CSRF protection
- Input validation
- Content Security Policy headers
- Account lockout protection

## Database Models

- User (AspNetCore Identity)
- Family
- FamilyMember
- Relationship
- Story
- Photo (for family crests)

The application uses Entity Framework Core with code-first migrations to manage the database schema.