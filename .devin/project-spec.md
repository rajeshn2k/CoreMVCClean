# Project Specification - CoreMVCClean

## Executive Summary
This is a .NET 10.0 ASP.NET Core MVC web application implementing a clean architecture pattern with a separate domain library. The application provides CRUD operations for Book and Person entities, currently using in-memory data storage with plans to migrate to SQLite. The project follows the Director pattern for business logic and implements DTO pattern for data transfer.

## Technical Stack

### Core Technologies
- **.NET 10.0**: Target framework
- **ASP.NET Core MVC**: Web framework
- **C#**: Primary language
- **SQLite**: Planned database (currently in-memory)
- **Razor Views**: Server-side rendering

### Key Libraries
- **Newtonsoft.Json** (13.0.4): JSON serialization/deserialization
- **Serilog** (4.4.0): Structured logging
- **Serilog.AspNetCore** (10.0.0): ASP.NET Core integration
- **Serilog.Settings.Configuration** (10.0.1): Configuration-based logging

## Architecture Overview

### Solution Structure
```
CoreMVCClean.slnx
├── CoreMVCClean/                          # Main web application
│   ├── Controllers/                       # MVC Controllers
│   ├── Views/                             # Razor Views
│   ├── Code/                              # Infrastructure code
│   ├── wwwroot/                           # Static assets
│   ├── Program.cs                         # Application entry point
│   └── appsettings.json                   # Configuration
│
└── CoreLibraryCleanAdditionalService/     # Domain library
    ├── Models/                            # Domain entities and DTOs
    ├── Director/                          # Business logic layer
    └── Data/                              # Data access layer
```

### Layer Responsibilities

#### Presentation Layer (CoreMVCClean)
- **Controllers**: Handle HTTP requests and responses
- **Views**: Render HTML using Razor syntax
- **Routing**: Map URLs to controller actions
- **Model Binding**: Convert HTTP data to C# objects
- **Validation**: Server-side input validation

#### Business Logic Layer (CoreLibraryCleanAdditionalService/Director)
- **Director Pattern**: Implement entity-specific business logic
- **HTTP Client Integration**: Communicate with external APIs
- **Data Transformation**: Convert between entities and DTOs
- **Error Handling**: Business-specific exception handling

#### Data Access Layer (CoreLibraryCleanAdditionalService/Data)
- **Repository Pattern**: Data access abstraction
- **Entity Management**: CRUD operations
- **Data Initialization**: Seed data for development
- **Database Operations**: SQL operations (future SQLite integration)

#### Domain Layer (CoreLibraryCleanAdditionalService/Models)
- **Entities**: Core business objects (Book, Person)
- **DTOs**: Data transfer objects for API communication
- **Domain Logic**: Business rules and validation

## Data Models

### Book Entity
```csharp
public class Book
{
    public string Id { get; set; }                    // Unique identifier (GUID)
    public string? personId { get; set; }             // Foreign key to Person
    public string bookCategory { get; set; }          // Category (Adventure, History, etc.)
    public string bookName { get; set; }              // Book title
    public string edition { get; set; }                // Edition type (Kindle, Paperback, etc.)
    public string image { get; set; }                 // Image path/URL
    public double price { get; set; }                 // Price in currency
    public DateTime dateCreated { get; set; }         // Creation timestamp
}
```

### Person Entity
```csharp
public class Person
{
    public string Id { get; set; }                    // Unique identifier (GUID)
    public string firstName { get; set; }             // First name
    public string lastName { get; set; }              // Last name
    public int rank { get; set; }                     // Rank/score
    public string category { get; set; }               // Category classification
    public DateTime dateOfBirth { get; set; }         // Birth date
    public bool isPlaySports { get; set; }            // Sports participation flag
    public DateTime dateCreated { get; set; }         // Creation timestamp
}
```

### DTO Pattern
The application uses separate DTOs for different operations:

#### BookDTO (Read Operations)
```csharp
public class BookDTO
{
    public string Id { get; set; }
    public string? personId { get; set; }
    public string bookCategory { get; set; }
    public string bookName { get; set; }
    public string edition { get; set; }
    public string image { get; set; }
    public double price { get; set; }
    public DateTime dateCreated { get; set; }
}
```

#### BookCreateDTO (Create Operations)
```csharp
public class BookCreateDTO
{
    public string? personId { get; set; }
    public string bookCategory { get; set; }
    public string bookName { get; set; }
    public string edition { get; set; }
    public string image { get; set; }
    public double price { get; set; }
}
```

*Similar pattern applies to PersonDTO and PersonCreateDTO*

## Business Logic

### Director Pattern Implementation
The `IEntityDirector<TEntity, TCreate>` interface defines the contract for entity operations:

```csharp
public interface IEntityDirector<TEntity, TCreate>
    where TEntity : class
    where TCreate : class
{
    // Read Operations
    Task<IEnumerable<TEntity>> GetEntitiesAsync(CancellationToken cancellationToken);
    Task<TEntity> GetEntityByIdAsync(string entityId, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> SearchEntitiesAsync(string searchValue, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> SearchEntitiesByForeignIdAsync(string foreignId, CancellationToken cancellationToken);
    
    // Update Operations
    Task<long> UpdateEntityByIdAsync(string entityId, TEntity entity, CancellationToken cancellationToken);
    Task<long> UpdateEntitiesAsync(IEnumerable<string> entityIds, IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    
    // Create Operations
    Task<TEntity> CreateEntityAsync(TCreate entity, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> CreateEntitiesAsync(IEnumerable<TCreate> entities, CancellationToken cancellationToken);
    
    // Delete Operations
    Task<long> DeleteEntityByIdAsync(string entityId, CancellationToken cancellationToken);
    Task<long> DeleteEntitiesAsync(CancellationToken cancellationToken);
}
```

### BookDirector Implementation
- Extends `IEntityDirector<BookDTO, BookCreateDTO>`
- Uses HttpClient for external API communication
- Implements JSON serialization/deserialization
- Handles HTTP status codes and error responses
- Supports bulk operations (create/update/delete multiple entities)

### PersonDirector Implementation
- Similar pattern to BookDirector
- Manages Person entity operations
- Supports search by various Person attributes

## API Endpoints

### Book Controller Routes
- `GET /Book` - List all books (with optional search)
- `GET /Book/Create` - Display create form
- `POST /Book/Create` - Create new book
- `GET /Book/Edit/{bookId}` - Display edit form
- `POST /Book/Edit/{bookId}` - Update book
- `POST /Book/Delete/{bookId}` - Delete book

### Person Controller Routes
- `GET /Person` - List all persons (with optional search)
- `GET /Person/Create` - Display create form
- `POST /Person/Create` - Create new person
- `GET /Person/Edit/{personId}` - Display edit form
- `POST /Person/Edit/{personId}` - Update person
- `POST /Person/Delete/{personId}` - Delete person

### Other Routes
- `GET /` - Home page
- `GET /Home/Error` - Error page
- `GET /Ping` - Health check endpoint

## Configuration

### Application Settings (appsettings.json)
```json
{
  "bookAPIUrl": "http://localhost:5001/api/books",
  "personAPIUrl": "http://localhost:5002/api/persons",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Dependency Injection Configuration
Services are registered in `DependencyInjection.cs`:
- `IConfiguration` as singleton
- `BookDirector` as HttpClient with base address from configuration
- `PersonDirector` as HttpClient with base address from configuration

## Current Implementation Status

### Completed Features
- ✅ MVC application structure
- ✅ Director pattern implementation
- ✅ DTO pattern for data transfer
- ✅ In-memory data initialization with seed data
- ✅ Basic CRUD operations for Books and Persons
- ✅ Search functionality
- ✅ Foreign key relationships (Book → Person)
- ✅ HTTP client infrastructure for external API integration
- ✅ Dependency injection setup
- ✅ Error handling middleware
- ✅ Security features (HTTPS, HSTS, Anti-forgery)

### In Progress Features
- 🔄 Migration from in-memory to SQLite database
- 🔄 External API integration (partially implemented)
- 🔄 Complete HTTP-based API client usage

### Planned Features
- 📋 Unit tests for Director layer
- 📋 Integration tests for Controllers
- 📋 Database migrations
- 📋 Advanced search and filtering
- 📋 Authentication and authorization
- 📋 API documentation (Swagger/OpenAPI)
- 📋 Performance optimization
- 📋 Advanced logging and monitoring
- 📋 API response model standardization (see `api-standardization-requirements.md`)

## Data Flow

### Create Operation Flow
1. User submits form via HTTP POST
2. Controller receives and validates data
3. Controller calls Director's CreateEntityAsync
4. Director transforms CreateDTO to Entity
5. Director calls external API or data layer
6. Response transformed back to DTO
7. Controller redirects to Index action

### Read Operation Flow
1. User requests page via HTTP GET
2. Controller calls Director's GetEntitiesAsync
3. Director queries external API or data layer
4. Response transformed to DTOs
5. Controller passes DTOs to View
6. View renders HTML with data

### Update Operation Flow
1. User submits edit form via HTTP POST
2. Controller receives and validates data
3. Controller calls Director's UpdateEntityByIdAsync
4. Director transforms DTO to Entity
5. Director calls external API or data layer
6. Response indicates success/failure
7. Controller redirects to Index action

### Delete Operation Flow
1. User clicks delete button (POST with Anti-forgery token)
2. Controller calls Director's DeleteEntityByIdAsync
3. Director calls external API or data layer
4. Response indicates success/failure
5. Controller redirects to Index action

## Security Considerations

### Implemented Security Measures
- HTTPS redirection for all requests
- HSTS (HTTP Strict Transport Security) in production
- Anti-forgery tokens on POST operations
- Input validation through Model State
- No sensitive data in code
- Configuration-based external URLs

### Security Best Practices to Follow
- Never hardcode credentials or API keys
- Use parameterized queries for database operations
- Validate and sanitize all user input
- Implement proper error handling without exposing sensitive information
- Use secure dependency versions
- Regular security updates for dependencies

## Performance Considerations

### Current Performance Characteristics
- In-memory data access (fast but not persistent)
- Synchronous operations in some areas (async/await improvement needed)
- No caching implemented
- No connection pooling for HTTP clients

### Performance Optimization Opportunities
- Implement response caching
- Use connection pooling for HTTP clients
- Implement database indexing when SQLite is added
- Optimize queries and data loading
- Implement pagination for large datasets
- Use async operations consistently

## Error Handling Strategy

### Current Error Handling
- Global exception handler in middleware pipeline
- HTTP status code handling in Directors
- Model state validation in Controllers
- Custom error page for exceptions

### Error Handling Best Practices
- Log all errors with sufficient context
- Provide user-friendly error messages
- Implement retry logic for transient failures
- Use appropriate HTTP status codes
- Never expose stack traces to end users

## Testing Strategy

### Current Testing
- Manual testing through web interface
- Ad-hoc API testing

### Planned Testing
- Unit tests for Director layer business logic
- Integration tests for Controller actions
- End-to-end tests for complete workflows
- Performance tests for database operations
- Security tests for vulnerabilities

## Deployment Considerations

### Development Deployment
- Local development with dotnet run
- In-memory data for quick iterations
- Development configuration overrides

### Production Deployment
- Build in Release configuration
- Use SQLite database for persistence
- Configure production appsettings
- Enable HSTS and security headers
- Implement proper logging and monitoring
- Use environment-specific configurations

## Future Enhancement Roadmap

### API Standardization (Priority Enhancement)
**Reference**: See `api-standardization-requirements.md` for detailed requirements

The application needs to adapt to standardized API response models with:
- Wrapped success responses (`ApiResponse<T>`)
- Structured error responses (`ApiErrorResponse`)
- Correlation ID tracking for request tracing
- Standardized error codes
- Enhanced error handling and logging

**Impact**: This enhancement will require changes to Models, Directors, Controllers, and Infrastructure layers while maintaining backward compatibility.

### Phase 1: Database Migration
- Replace in-memory lists with SQLite
- Implement Entity Framework Core
- Create database migrations
- Update data access layer
- Test data persistence

### Phase 2: API Integration
- Complete external API client implementation
- Implement proper error handling for HTTP calls
- Add retry logic and circuit breakers
- Implement request/response logging

### Phase 3: Testing Infrastructure
- Add unit test project
- Create integration test project
- Implement test data setup
- Set up CI/CD pipeline

### Phase 4: Advanced Features
- Add authentication and authorization
- Implement advanced search and filtering
- Add pagination and sorting
- Create API documentation
- Implement caching strategies

### Phase 5: Performance and Monitoring
- Add application performance monitoring
- Implement structured logging
- Add health check endpoints
- Optimize database queries
- Implement CDN for static assets

## Development Guidelines

### Code Generation Rules
- Always follow existing patterns and conventions
- Maintain consistency with current implementation
- Implement complete functionality (no partial implementations)
- Update all related files (models, DTOs, controllers, views)
- Test changes before considering them complete

### Architectural Principles
- Maintain separation of concerns
- Follow SOLID principles
- Use dependency injection for service resolution
- Implement proper error handling
- Keep code DRY (Don't Repeat Yourself)

### Quality Standards
- Code must compile without warnings
- Follow C# coding conventions
- Add XML documentation for public APIs
- Implement proper async/await patterns
- Handle cancellation tokens appropriately

## Maintenance Guidelines

### Regular Maintenance Tasks
- Keep dependencies updated for security patches
- Review and optimize database queries
- Monitor application performance
- Update documentation as code changes
- Review and refactor code for maintainability

### Troubleshooting Common Issues
- **Build failures**: Check dependency versions, clean and rebuild
- **Runtime errors**: Check logs, verify configuration, test database connectivity
- **Performance issues**: Profile application, check database queries, review HTTP client usage
- **Security issues**: Update dependencies, review code for vulnerabilities, implement security best practices

This specification serves as the authoritative guide for understanding the CoreMVCClean project architecture, implementation details, and development guidelines for AI-assisted code generation.