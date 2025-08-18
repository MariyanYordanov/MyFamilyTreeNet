# MyFamilyTreeNet - Family Social Network & Genealogy Platform

## Description

MyFamilyTreeNet is a comprehensive family social network and genealogy platform that allows users to create and manage family trees, share family stories, photos, and connect with other families. The project features both a traditional ASP.NET Core MVC interface and a modern Angular SPA frontend, backed by a robust Web API.

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
├── MyFamilyTreeNet.Web/          # Angular SPA frontend
│   ├── src/app/                  # Angular application
│   ├── features/                 # Feature modules
│   └── shared/                   # Shared components
└── MyFamilyTreeNet.Tests/        # Unit tests
```

## Features

### User Management
- User registration and login with JWT authentication
- User roles: User and Administrator
- User profiles with personal information (First Name, Middle Name, Last Name)
- Account lockout functionality after failed login attempts
- Password change functionality
- Session management with cookies and JWT tokens

### Family Management
- Create and manage multiple families per user
- Family descriptions and privacy settings (Public/Private)
- Add unlimited family members to each family
- Complete biographical information for each member:
  - Full name (First, Middle, Last)
  - Birth and death dates
  - Places of birth and death
  - Detailed biography
  - Gender
- Family photo/crest upload functionality
- Family statistics dashboard

### Relationship Management
- Define complex relationships between family members:
  - Parent/Child
  - Spouse
  - Sibling
  - Grandparent/Grandchild
  - Aunt/Uncle
  - Cousin
  - Other custom relationships
- Bidirectional relationship tracking
- Visual family tree generation with D3.js
- Interactive relationship exploration

### Content Management
- Upload and manage family photos with descriptions
- Create rich family stories and memories
- Attach stories to specific families
- Photo galleries for each family
- Document family history and traditions

### Search and Discovery
- Search families by name
- Browse public family profiles
- Filter families by privacy settings
- Latest families showcase on homepage
- Featured families display

### Interactive Visualizations
- Dynamic family tree visualization using D3.js
- Interactive nodes showing member details
- Zoom and pan functionality
- Relationship lines with labels
- Responsive design for all screen sizes

### Administration
- Comprehensive admin dashboard with:
  - Total families count
  - Total members count
  - Total stories count
  - User activity monitoring
- User management capabilities:
  - View all registered users
  - Ban/unban user accounts
  - View user details and activity
  - Role management
- System health monitoring

## Technologies

### Backend (ASP.NET Core 8.0)
- Entity Framework Core with SQLite/PostgreSQL support
- ASP.NET Core Identity for authentication
- JWT tokens for API security
- AutoMapper for DTO mapping
- Custom middleware for security headers and global error handling
- MVC pattern for web interface
- RESTful API design
- Swagger/OpenAPI documentation

### Frontend
#### MVC Views (Traditional Web Interface)
- Razor views with Bootstrap 5
- jQuery for interactivity
- D3.js for family tree visualization
- Font Awesome icons
- Responsive design

#### Angular SPA (Modern Frontend)
- Angular 20 with standalone components
- RxJS for reactive programming
- Angular Material UI components
- TypeScript for type safety
- Angular Router for navigation
- HTTP interceptors for authentication
- Lazy loading for performance

### Database
- SQLite for development
- PostgreSQL support for production
- Entity Framework Core migrations
- Seed data for initial setup
- Optimized queries with LINQ

## Prerequisites

Before running the project, ensure you have:

- .NET 8.0 SDK or newer
- Node.js 18+ and npm (for Angular frontend)
- Git for cloning the repository
- Visual Studio 2022 / VS Code (recommended)

## Installation and Setup

### Clone the Repository

```bash
git clone <repository-url>
cd MyFamilyTreeNet
```

### Backend Setup (ASP.NET Core)

```bash
# Navigate to the API directory
cd MyFamilyTreeNet.Api

# Restore NuGet packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the backend
dotnet run
```

The backend will be available at:
- API: `http://localhost:5000`
- MVC Interface: `http://localhost:5000`
- Swagger Documentation: `http://localhost:5000/swagger`

### Frontend Setup (Angular)

Open a new terminal:

```bash
# Navigate to the Angular directory
cd MyFamilyTreeNet.Web

# Install dependencies
npm install

# Run the Angular development server
npm start
```

The Angular application will be available at: `http://localhost:4200`

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

### Core Functionality
- User registration with email verification
- JWT and cookie-based authentication
- Role-based authorization (User/Admin)
- Account lockout after failed attempts

### Family Features
- Multi-family support per user
- Public/Private family visibility
- Family crest/photo upload
- Family member CRUD operations
- Complex relationship management
- Family statistics and analytics

### Visualization
- Interactive D3.js family tree
- Zoom/pan navigation
- Dynamic node positioning
- Relationship type labels
- Responsive design

### Content Features
- Rich text family stories
- Photo galleries
- Member biographies
- Historical documentation
- Date and location tracking

### Administrative
- User management dashboard
- Ban/unban functionality
- System statistics
- Activity monitoring
- Role management

### Technical Features
- RESTful API with Swagger
- Angular SPA frontend
- MVC web interface
- Real-time updates
- Responsive design
- Cross-platform compatibility

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