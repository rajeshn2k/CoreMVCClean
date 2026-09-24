# AGENTS.md - Mavin AI Coding Agent Configuration

## Project Overview
This is a .NET 10.0 web application implementing a clean architecture pattern with:
- **CoreMVCClean**: ASP.NET Core MVC web application
- **CoreLibraryCleanAdditionalService**: Domain library with business logic and data models
- Implements CRUD operations for Book and Person entities
- Uses SQLite for data persistence
- Implements Director pattern for entity management
- Currently uses in-memory data initialization with mock data

## Architecture

### Project Structure
```
CoreMVCClean/
├── Controllers/          # MVC Controllers
├── Views/                # Razor Views
├── Code/                 # Application infrastructure (DependencyInjection, ErrorViewModel)
├── Program.cs            # Application entry point
└── appsettings.json      # Configuration

CoreLibraryCleanAdditionalService/
├── Models/               # Domain entities and DTOs
├── Director/             # Business logic layer (Director pattern)
├── Data/                 # Data initialization and repository patterns
└── Models/
    ├── Book.cs / Person.cs           # Domain entities
    ├── BookDTO.cs / PersonDTO.cs     # Data transfer objects
    └── BookCreateDTO.cs / PersonCreateDTO.cs  # Create DTOs
```

### Key Patterns
- **Director Pattern**: `IEntityDirector<TEntity, TCreate>` interface defines entity operations
- **DTO Pattern**: Separate DTOs for read and create operations
- **Dependency Injection**: Service registration in `DependencyInjection.cs`
- **Repository Pattern**: Static initializers for data management (currently in-memory)

## Coding Conventions

### C# Specific
- Target Framework: .NET 10.0
- Implicit Usings: Enabled
- Nullable Reference Types: Disabled (`<Nullable>disable</Nullable>`)
- Naming Conventions:
  - Public properties: PascalCase (`bookName`, `personId`)
  - Private fields: camelCase or _camelCase
  - Methods: PascalCase
  - Interfaces: PascalCase with 'I' prefix (`IEntityDirector`)
  - Namespaces: PascalCase (`Core.Library.Clean.AdditionalService`)

### File Organization
- One class per file
- File name matches class name
- Organize by domain/purpose in folders
- Controllers in `Controllers/` folder
- Models in `Models/` folder
- Business logic in `Director/` folder
- Data layer in `Data/` folder

### Dependencies
- **Newtonsoft.Json** (13.0.4) - JSON serialization
- **Serilog** (4.4.0) - Logging framework
- **Serilog.AspNetCore** (10.0.0) - ASP.NET Core integration
- **Serilog.Settings.Configuration** (10.0.1) - Configuration support

## Build and Development Commands

### Build
```bash
dotnet build
```

### Run Application
```bash
dotnet run --project CoreMVCClean/Core.MVC.Clean.csproj
```

### Restore Dependencies
```bash
dotnet restore
```

### Clean Build
```bash
dotnet clean
```

## Configuration

### Application Settings
- External API URLs configured in `appsettings.json`:
  - `bookAPIUrl`: URL for Book API service
  - `personAPIUrl`: URL for Person API service
- Currently using mock data via `DatabaseInitializerBook` and `DatabaseInitializerPerson`

### Environment
- Development: Uses `appsettings.Development.json`
- Production: Uses `appsettings.json` with HSTS enabled

## Entity Management

### Standard Entity Operations
All entities implement `IEntityDirector<TEntity, TCreate>` with:
- `GetEntitiesAsync()` - Get all entities
- `GetEntityByIdAsync(string id)` - Get single entity by ID
- `SearchEntitiesAsync(string searchValue)` - Search entities
- `SearchEntitiesByForeignIdAsync(string foreignId)` - Search by foreign key
- `UpdateEntityByIdAsync(string id, TEntity entity)` - Update single entity
- `UpdateEntitiesAsync(IEnumerable<string> ids, IEnumerable<TEntity> entities)` - Bulk update
- `CreateEntityAsync(TCreate entity)` - Create single entity
- `CreateEntitiesAsync(IEnumerable<TCreate> entities)` - Bulk create
- `DeleteEntityByIdAsync(string id)` - Delete single entity
- `DeleteEntitiesAsync()` - Delete all entities

### Current Implementation Notes
- Controllers currently use in-memory data from static initializers
- HTTP-based API integration is partially implemented but commented out
- `BookDirector` and `PersonDirector` are HTTP clients for external API calls
- Transition from in-memory to HTTP-based API is in progress

## Development Guidelines

### When Adding New Entities
1. Create domain entity class in `CoreLibraryCleanAdditionalService/Models/`
2. Create DTO classes (EntityDTO and EntityCreateDTO)
3. Create Director class implementing `IEntityDirector<TEntity, TCreate>`
4. Create Controller in `CoreMVCClean/Controllers/`
5. Register HTTP client in `DependencyInjection.cs`
6. Add configuration in `appsettings.json`
7. Create corresponding Views

### When Modifying Existing Entities
1. Update domain entity, DTOs, and maintain consistency
2. Update Director implementation if business logic changes
3. Update Controller if API contract changes
4. Update Views if UI changes required
5. Test all CRUD operations

### Data Layer Considerations
- Current implementation uses static in-memory lists
- Future enhancement: Replace with SQLite database
- Database initialization code already present in `Data/` folder
- Maintain separation between data access and business logic

## Error Handling
- Global exception handling via `UseExceptionHandler("/Home/Error")`
- HTTP exceptions thrown by Directors include status codes and error details
- ModelState validation in controllers for create/update operations

## Security Considerations
- HTTPS redirection enabled
- HSTS enabled in production
- Anti-forgery tokens on POST operations
- No sensitive data in code or configuration files

## Testing Strategy
- Manual testing through web interface
- API testing through HTTP endpoints
- Future: Add unit tests for Director layer
- Future: Add integration tests for Controllers

## Code Quality Standards
- Follow existing code patterns and conventions
- Maintain consistent formatting and indentation
- Add XML documentation comments for public APIs
- Use async/await for I/O operations
- Implement proper cancellation token support
- Handle exceptions appropriately

## Dependencies and Package Management
- Use stable package versions (avoid bleeding edge)
- Prefer packages published at least 7 days ago
- Update packages via `dotnet add package <PackageName>`
- Keep package versions consistent across projects

## Git Workflow
- Branch naming: `feature/`, `bugfix/`, `hotfix/`
- Commit messages: Conventional Commits format
- No secrets in code or configuration
- Review changes before committing

## Important Notes
- This is a learning project for C# programming
- Current data layer is in-memory (transition to SQLite in progress)
- External API integration is partially implemented
- Maintain backward compatibility when possible
- Follow .NET best practices and patterns