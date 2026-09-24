# Code Conventions - CoreMVCClean Project

## General Principles

### Code Style Philosophy
- **Consistency over creativity**: Follow existing patterns rather than introducing new ones
- **Readability first**: Write code that is easy to understand and maintain
- **Simplicity**: Choose the simplest solution that meets requirements
- **Maintainability**: Write code that can be easily modified and extended

### Core Values
- Code should be self-documenting where possible
- When clarification is needed, add comments explaining "why" not "what"
- Keep methods focused and single-purpose
- Avoid code duplication (DRY principle)
- Follow established patterns in the codebase

## C# Coding Standards

### Naming Conventions

#### Classes and Interfaces
- **Classes**: PascalCase (e.g., `BookDirector`, `DatabaseInitializerBook`)
- **Interfaces**: PascalCase with 'I' prefix (e.g., `IEntityDirector`)
- **Abstract classes**: PascalCase (e.g., `BaseController`)

#### Methods and Properties
- **Methods**: PascalCase (e.g., `GetEntitiesAsync`, `CreateEntityAsync`)
- **Properties**: PascalCase (e.g., `bookName`, `personId`, `dateCreated`)
- **Public fields**: PascalCase (avoid; use properties instead)
- **Private fields**: camelCase or _camelCase (e.g., `httpClient`, `_logger`)

#### Parameters and Variables
- **Method parameters**: camelCase (e.g., `entityId`, `cancellationToken`)
- **Local variables**: camelCase (e.g., `requestUrl`, `response`)
- **Constants**: PascalCase (e.g., `MaxRetryCount`, `DefaultTimeout`)

#### Namespaces
- **Namespaces**: PascalCase (e.g., `Core.Library.Clean.AdditionalService`)
- Match folder structure: `CoreMVCClean/Controllers/` → `Core.MVC.Clean.Controllers`

#### Enums
- **Enum type**: PascalCase (e.g., `BookCategory`, `PersonRank`)
- **Enum values**: PascalCase (e.g., `Adventure`, `History`, `Sports`)

### File Organization

#### File Naming
- One class per file
- File name matches class name exactly
- Use `.cs` extension for all C# files
- Organize files by domain/purpose in appropriate folders

#### Folder Structure
```
CoreMVCClean/
├── Controllers/          # MVC Controllers
├── Views/               # Razor Views
│   ├── Book/           # Book-specific views
│   ├── Person/         # Person-specific views
│   └── Shared/         # Shared views (layouts, partials)
├── Code/               # Infrastructure code
├── Models/             # View models (if needed)
└── wwwroot/           # Static assets

CoreLibraryCleanAdditionalService/
├── Models/            # Domain entities and DTOs
├── Director/          # Business logic layer
└── Data/              # Data access layer
```

#### File Content Order
1. Using statements (sorted alphabetically, System.* first)
2. Namespace declaration
3. Class/interface declaration
4. Fields (private fields first)
5. Constructors
6. Properties
7. Methods (public first, then private)
8. Nested types (if any)

### Code Formatting

#### Indentation and Spacing
- Use 4 spaces for indentation (no tabs)
- Use consistent indentation throughout
- Add space after commas in parameter lists
- Add space around operators (except for unary operators)
- No trailing whitespace

#### Braces and Line Breaks
- Opening brace on new line for classes, methods, properties
- Closing brace on new line
- No empty lines between opening brace and first statement
- One blank line between methods
- Two blank lines between types (classes, interfaces)

#### Line Length
- Maximum line length: 120 characters
- Break long lines at logical points
- Prefer readability over strict line length limits

### Code Structure

#### Class Organization
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Core.MVC.Clean.Controllers
{
    public class ExampleController : Controller
    {
        // Fields
        private readonly IExampleService _exampleService;

        // Constructor
        public ExampleController(IExampleService exampleService)
        {
            _exampleService = exampleService;
        }

        // Properties (if any)
        public string PropertyName { get; set; }

        // Public Methods
        public async Task<IActionResult> Index()
        {
            // Implementation
        }

        // Private Methods
        private async Task<bool> ValidateInput(string input)
        {
            // Implementation
        }
    }
}
```

#### Method Structure
- Keep methods focused on single responsibility
- Prefer methods under 50 lines
- Use descriptive method names
- Avoid excessive parameters (consider parameter objects)

### Async/Await Patterns

#### Best Practices
- Use async/await for I/O operations
- Avoid async void (use async Task instead)
- Use ConfigureAwait(false) in library code
- Pass CancellationToken when available
- Don't block on async code (no .Result or .Wait())

#### Example
```csharp
// Good
public async Task<BookDTO> GetBookAsync(string bookId, CancellationToken cancellationToken)
{
    var response = await httpClient.GetAsync(bookId, cancellationToken)
        .ConfigureAwait(false);
    return await response.Content.ReadAsAsync<BookDTO>(cancellationToken)
        .ConfigureAwait(false);
}

// Bad
public BookDTO GetBook(string bookId)
{
    var response = httpClient.GetAsync(bookId).Result;
    return response.Content.ReadAsAsync<BookDTO>().Result;
}
```

### Exception Handling

#### Guidelines
- Catch specific exceptions before general ones
- Don't catch Exception unless absolutely necessary
- Log exceptions with sufficient context
- Provide meaningful error messages to users
- Use throw; to re-throw (not throw ex)

#### Example
```csharp
try
{
    var result = await someOperationAsync(cancellationToken);
    return result;
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    _logger.LogWarning("Resource not found: {ResourceId}", resourceId);
    return null;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error processing request for {ResourceId}", resourceId);
    throw;
}
```

### Dependency Injection

#### Constructor Injection
- Use constructor injection for dependencies
- Mark dependencies as readonly
- Use interfaces for dependencies when possible
- Don't use service locator pattern

#### Example
```csharp
public class BookController : Controller
{
    private readonly BookDirector _bookDirector;
    private readonly ILogger<BookController> _logger;

    public BookController(BookDirector bookDirector, ILogger<BookController> logger)
    {
        _bookDirector = bookDirector;
        _logger = logger;
    }
}
```

### LINQ Guidelines

#### Performance Considerations
- Prefer IQueryable for database queries
- Use IEnumerable for in-memory collections
- Be careful with deferred execution
- Avoid multiple enumerations (use .ToList() or .ToArray())

#### Readability
- Use method syntax for simple queries
- Use query syntax for complex queries
- Break complex LINQ into multiple steps
- Add comments for complex operations

#### Example
```csharp
// Good - simple query
var activeBooks = books.Where(b => b.IsActive).ToList();

// Good - complex query with query syntax
var expensiveBooks = from book in books
                     where book.Price > 100
                     orderby book.Price descending
                     select book;

// Bad - complex single-line LINQ
var result = books.Where(b => b.Category == "Fiction" && b.Price > 50 && b.IsActive).OrderByDescending(b => b.Price).ThenBy(b => b.Name).Select(b => new { b.Name, b.Price }).ToList();
```

### String Handling

#### Guidelines
- Use string interpolation for simple formatting
- Use StringBuilder for complex string building
- Use string.IsNullOrWhiteSpace() for validation
- Avoid string concatenation in loops

#### Example
```csharp
// Good
var message = $"Book {bookName} created successfully";

// Good - complex building
var sb = new StringBuilder();
sb.AppendLine("Book Summary:");
sb.AppendLine($"Name: {bookName}");
sb.AppendLine($"Price: {price}");
var summary = sb.ToString();

// Bad
var message = "Book " + bookName + " created successfully";
```

### Collections

#### Guidelines
- Use specific collection types (List<T>, Dictionary<K,V>)
- Use IReadOnlyCollection/IReadOnlyList for read-only returns
- Initialize collections with capacity when size is known
- Use collection initializers for clarity

#### Example
```csharp
// Good
public class BookService
{
    private readonly List<Book> _books = new List<Book>(100);
    
    public IReadOnlyCollection<Book> GetAllBooks()
    {
        return _books.AsReadOnly();
    }
}

// Bad
public class BookService
{
    private ArrayList _books; // Use List<Book> instead
}
```

### Null Handling

#### Guidelines
- Enable nullable reference types (currently disabled in project)
- Use null-conditional operators (?.) and null-coalescing (??)
- Guard against null arguments
- Return empty collections instead of null

#### Example
```csharp
// Good
public BookDTO? GetBook(string bookId)
{
    return _books.FirstOrDefault(b => b.Id == bookId);
}

// Good - defensive programming
public void ProcessBook(BookDTO? book)
{
    if (book == null)
    {
        throw new ArgumentNullException(nameof(book));
    }
    // Process book
}

// Bad
public BookDTO GetBook(string bookId)
{
    return _books.FirstOrDefault(b => b.Id == bookId) ?? null; // Redundant
}
```

### Comments and Documentation

#### XML Documentation
- Add XML comments for public APIs
- Include <summary>, <param>, <returns> tags
- Use <exception> for documented exceptions
- Keep comments concise and accurate

#### Example
```csharp
/// <summary>
/// Gets a book by its unique identifier.
/// </summary>
/// <param name="bookId">The unique identifier of the book.</param>
/// <param name="cancellationToken">Cancellation token for the operation.</param>
/// <returns>The book if found; otherwise, null.</returns>
/// <exception cref="ArgumentNullException">Thrown when bookId is null or empty.</exception>
public async Task<BookDTO?> GetBookAsync(string bookId, CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(bookId))
    {
        throw new ArgumentNullException(nameof(bookId));
    }
    
    // Implementation
}
```

#### Inline Comments
- Use inline comments to explain "why" not "what"
- Comment complex algorithms or business rules
- Don't comment obvious code
- Keep comments up to date with code changes

### ASP.NET MVC Specific Conventions

#### Controller Guidelines
- Inherit from Controller base class
- Use async actions for I/O operations
- Return IActionResult for flexibility
- Use [HttpGet], [HttpPost] attributes explicitly
- Implement proper HTTP status codes

#### Example
```csharp
[HttpGet]
public async Task<IActionResult> Index(string? search)
{
    var books = await _bookDirector.GetEntitiesAsync(cancellationToken);
    
    if (!string.IsNullOrWhiteSpace(search))
    {
        books = books.Where(b => b.bookName.Contains(search, StringComparison.OrdinalIgnoreCase));
    }
    
    return View(books);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([FromForm] BookCreateDTO book)
{
    if (!ModelState.IsValid)
    {
        return View(book);
    }
    
    await _bookDirector.CreateEntityAsync(book, cancellationToken);
    return RedirectToAction(nameof(Index));
}
```

#### View Guidelines
- Use strongly-typed views
- Implement proper model binding
- Use tag helpers over HTML helpers
- Include anti-forgery tokens in forms
- Use view components for reusable UI

### HTTP Client Usage

#### Best Practices
- Use HttpClient via dependency injection
- Don't create new HttpClient instances per request
- Set appropriate timeouts
- Implement proper error handling
- Use base addresses for common endpoints

#### Example
```csharp
// Registration in DI
services.AddHttpClient<BookDirector>(client =>
{
    client.BaseAddress = new Uri(configuration["bookAPIUrl"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Usage in Director
public async Task<BookDTO> GetBookAsync(string bookId, CancellationToken cancellationToken)
{
    var response = await _httpClient.GetAsync($"api/books/{bookId}", cancellationToken);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsAsync<BookDTO>(cancellationToken);
}
```

### JSON Serialization

#### Guidelines
- Use Newtonsoft.Json (as per project dependencies)
- Configure serialization settings consistently
- Handle null values appropriately
- Use proper date formatting
- Consider circular reference handling

#### Example
```csharp
private static StringContent CreateJsonContent(object content)
{
    var settings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"
    };
    
    var json = JsonConvert.SerializeObject(content, settings);
    return new StringContent(json, Encoding.UTF8, "application/json");
}
```

### Logging

#### Guidelines
- Use Serilog for structured logging
- Log at appropriate levels (Debug, Information, Warning, Error)
- Include context in log messages
- Don't log sensitive information
- Use structured logging with properties

#### Example
```csharp
public class BookController : Controller
{
    private readonly ILogger<BookController> _logger;
    
    public async Task<IActionResult> Create(BookCreateDTO book)
    {
        _logger.LogInformation("Creating new book: {BookName}", book.bookName);
        
        try
        {
            var result = await _bookDirector.CreateEntityAsync(book, cancellationToken);
            _logger.LogInformation("Book created successfully: {BookId}", result.Id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create book: {BookName}", book.bookName);
            ModelState.AddModelError(string.Empty, "Failed to create book");
            return View(book);
        }
    }
}
```

### Testing Conventions (Future)

#### Unit Test Guidelines
- Arrange-Act-Assert pattern
- Descriptive test names
- One assertion per test
- Use test data builders for complex objects
- Mock external dependencies

#### Example
```csharp
[Fact]
public async Task GetBookAsync_WithValidId_ReturnsBook()
{
    // Arrange
    var bookId = "test-id";
    var expectedBook = new BookDTO { Id = bookId, bookName = "Test Book" };
    _mockBookDirector.Setup(x => x.GetEntityByIdAsync(bookId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedBook);
    
    // Act
    var result = await _controller.GetBook(bookId);
    
    // Assert
    var viewResult = Assert.IsType<ViewResult>(result);
    var model = Assert.IsType<BookDTO>(viewResult.Model);
    Assert.Equal(expectedBook.bookName, model.bookName);
}
```

## Code Review Checklist

### Before Submitting Code
- [ ] Code compiles without warnings
- [ ] Follows project naming conventions
- [ ] Methods are focused and single-purpose
- [ ] Proper error handling implemented
- [ ] Async/await used correctly
- [ ] No hardcoded values (use configuration)
- [ ] XML documentation added for public APIs
- [ ] No sensitive data in code
- [ ] Dependencies are appropriate and stable
- [ ] Code is tested and verified

### Performance Considerations
- [ ] No unnecessary database queries
- [ ] Proper use of async/await
- [ ] Efficient LINQ queries
- [ ] Proper resource disposal
- [ ] No memory leaks
- [ ] Appropriate caching where needed

### Security Considerations
- [ ] Input validation implemented
- [ ] SQL injection prevention (parameterized queries)
- [ ] XSS prevention (proper encoding)
- [ ] CSRF protection (anti-forgery tokens)
- [ ] No sensitive data in logs
- [ ] Proper authentication/authorization

## Common Anti-Patterns to Avoid

### Code Smells
- **God classes**: Classes that do too much
- **Long methods**: Methods that are too long/complex
- **Magic numbers**: Unexplained numeric literals
- **Dead code**: Commented out code that should be removed
- **Copy-paste programming**: Duplicated code

### Architectural Anti-Patterns
- **Tight coupling**: Classes that depend too heavily on each other
- **Circular dependencies**: A depends on B, B depends on A
- **God objects**: Objects that know too much or do too much
- **Spaghetti code**: Unstructured, hard-to-follow code
- **Golden hammer**: Using the same solution for every problem

### Performance Anti-Patterns
- **N+1 query problem**: Executing multiple queries instead of one
- **Premature optimization**: Optimizing before measuring
- **Excessive memory allocation**: Creating unnecessary objects
- **Blocking on async**: Using .Result or .Wait() on async methods
- **Inefficient algorithms**: Using O(n²) when O(n) would work

## Tool Configuration

### Recommended IDE Settings
- Use 4 spaces for indentation
- Show whitespace characters
- Enable XML documentation generation
- Configure code formatting rules
- Set up code analysis rules

### .editorconfig Example
```ini
root = true

[*]
charset = utf-8
indent_style = space
indent_size = 4
end_of_line = crlf
insert_final_newline = true
trim_trailing_whitespace = true

[*.cs]
dotnet_style_qualification_for_field = false:suggestion
dotnet_style_qualification_for_property = false:suggestion
dotnet_style_qualification_for_method = false:suggestion
dotnet_style_qualification_for_event = false:suggestion
```

## Continuous Improvement

### Code Quality Metrics
- Maintain high code coverage (when tests are added)
- Keep cyclomatic complexity low
- Monitor code duplication
- Track technical debt
- Regular code reviews

### Refactoring Guidelines
- Refactor in small steps
- Keep tests passing during refactoring
- Improve without changing behavior
- Document refactoring reasons
- Update related documentation

This code conventions document serves as the authoritative guide for maintaining consistency and quality in the CoreMVCClean project. All code generation should strictly adhere to these standards.