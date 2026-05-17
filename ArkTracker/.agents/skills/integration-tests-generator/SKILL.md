# Integration Tests Skill Definition

## Skill Name
`integration-tests`

## Description
Expert-level capability to create, configure, and maintain comprehensive integration test suites that validate application functionality across all layers (REST API, Application, Infrastructure, and Database). This skill enables creation of production-ready integration tests using modern frameworks and best practices.

## Domain
Quality Assurance / Software Testing / C# / .NET

## Category
Testing & Quality Assurance

---

## Overview

This skill provides comprehensive knowledge and implementation capability for:
- Creating integration test projects and infrastructure
- Testing REST API endpoints with real HTTP clients
- Testing application layer handlers (MediatR/CQRS)
- Testing repository and data access layers
- Testing database operations with in-memory providers
- Setting up test fixtures and factories
- JWT authentication testing
- Authorization and security testing
- End-to-end workflow testing

---

## Key Capabilities

### 1. Test Project Setup
- Create .NET test projects with proper SDK configuration
- Configure project dependencies (xUnit, FluentAssertions, testing libraries)
- Set up project references to production code
- Configure test runners and discovery
- Implement proper project structure and organization

### 2. Test Infrastructure
- Create WebApplicationFactory for ASP.NET Core test hosting
- Implement database fixtures with in-memory providers
- Set up test data seeding
- Configure dependency injection for tests
- Implement proper async lifecycle management (IAsyncLifetime)
- Create JWT token generation helpers
- Configure test isolation and cleanup

### 3. REST API Testing
- Create tests for HTTP endpoints
- Verify HTTP status codes and responses
- Test request/response serialization
- Validate headers and content types
- Test URL routing and query parameters
- Verify error responses

### 4. Authentication & Security Testing
- Test login endpoints with valid/invalid credentials
- Verify JWT token generation
- Test authorization attributes
- Validate authorization checks
- Test token expiration handling
- Verify unauthorized access rejection

### 5. Application Layer Testing
- Test MediatR request handlers
- Validate query handler execution
- Test business logic layer
- Verify data transformation
- Test validation behaviors
- Test error handling

### 6. Repository & Data Access Testing
- Test repository interface implementation
- Verify database query execution
- Test CRUD operations
- Validate data filtering and sorting
- Test relationship handling
- Verify transaction management

### 7. Database Layer Testing
- Test entity persistence
- Verify database constraints
- Test data integrity
- Validate migrations
- Test entity relationships
- Verify index usage

### 8. End-to-End Testing
- Create complete workflow tests
- Test multi-layer integration
- Verify data flow across layers
- Test security across workflows
- Test edge cases and error scenarios
- Validate user scenarios

### 9. Test Patterns & Best Practices
- Implement AAA pattern (Arrange, Act, Assert)
- Use FluentAssertions for readable assertions
- Follow test naming conventions
- Implement proper test isolation
- Use parameterized tests when appropriate
- Document test intent clearly
- Avoid test interdependencies

### 10. Documentation
- Create comprehensive test documentation
- Document test coverage and scenarios
- Provide usage examples and commands
- Create quick reference guides
- Document architecture and design
- Provide troubleshooting guidance

---

## Technical Skills Required

### Frameworks & Libraries
- **xUnit** - Test framework with async support
- **FluentAssertions** - Assertion library for readable assertions
- **Microsoft.AspNetCore.Mvc.Testing** - ASP.NET Core test utilities
- **Microsoft.EntityFrameworkCore** - Data access testing
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database provider
- **Moq** - Mocking library for dependencies
- **JWT tokens** - Authentication testing

### C# Features
- async/await patterns
- LINQ queries
- Extension methods
- Generics
- Dependency injection
- Interfaces and abstractions
- Entity Framework Core
- Collections (List, Dictionary, etc.)

### .NET Architecture
- REST API design
- MediatR CQRS pattern
- Repository pattern
- Dependency injection containers
- Entity Framework Core DbContext
- ASP.NET Core middleware
- Authentication/Authorization

### Testing Concepts
- Unit testing principles
- Integration testing strategies
- Test isolation and fixtures
- Test data management
- Async test patterns
- HTTP testing
- Database testing

---

## Common Patterns & Solutions

### Pattern 1: WebApplicationFactory Setup
```csharp
public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace DbContext with in-memory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });
        });
    }
}
```

### Pattern 2: Database Fixture
```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private readonly DbContextOptions<AppDbContext> _options;
    public AppDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    public async Task InitializeAsync()
    {
        Context = new AppDbContext(_options);
        await Context.Database.EnsureCreatedAsync();
        await SeedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
    }

    private async Task SeedTestDataAsync() { }
}
```

### Pattern 3: REST API Test
```csharp
public class ApiControllerTests : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private HttpClient _client;

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Endpoint_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var request = new { data = "value" };
        var content = new StringContent(
            JsonSerializer.Serialize(request), 
            Encoding.UTF8, 
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/endpoint", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Pattern 4: JWT Token Helper
```csharp
public static class JwtTokenHelper
{
    public static string GenerateTestToken(string userId = "1")
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("secret-key"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "test-issuer",
            audience: "test-audience",
            claims: new[] 
            { 
                new Claim(ClaimTypes.NameIdentifier, userId) 
            },
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### Pattern 5: Handler Test
```csharp
public class QueryHandlerTests : IAsyncLifetime
{
    private IMediator _mediator;
    private AppDbContext _dbContext;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddScoped<AppDbContext>(_ => _dbContext);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Handler).Assembly));
        
        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Handle_WithValidQuery_ReturnsExpectedResult()
    {
        // Arrange
        var query = new MyQuery(value: "test");

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
    }
}
```

### Pattern 6: Repository Test
```csharp
public class RepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private IRepository _repository;

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsEntity()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await _repository.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
    }
}
```

### Pattern 7: End-to-End Test
```csharp
public class EndToEndTests : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory;
    private HttpClient _client;

    [Fact]
    public async Task CompleteWorkflow_Success()
    {
        // Step 1: Login
        var loginResponse = await _client.PostAsync("/api/auth/login", ...);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await loginResponse.Content.ReadAsStringAsync();

        // Step 2: Make authenticated request
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        var dataResponse = await _client.GetAsync("/api/data");
        dataResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Verify data
        var data = await dataResponse.Content.ReadAsAsync<DataDto>();
        data.Should().NotBeNull();
    }
}
```

### Pattern 8: FluentAssertions Usage
```csharp
// Collections
result.Should().NotBeEmpty();
result.Should().HaveCount(5);
result.Should().ContainItem(expectedItem);
result.Should().AllSatisfy(item => item.IsValid.Should().BeTrue());

// Strings
response.Should().Contain("expected text");
response.Should().StartWith("prefix");
response.Should().Match("*pattern*");

// Objects
entity.Should().NotBeNull();
entity.Should().BeEquivalentTo(expected);
entity.Id.Should().Be(expectedId);

// Exceptions
await action.Should().ThrowAsync<ArgumentException>();
```

---

## Test Types Covered

### 1. Endpoint Tests
- Login/authentication endpoints
- GET endpoints with parameters
- POST/PUT endpoints with body
- DELETE endpoints
- Status code verification
- Response content validation

### 2. Authorization Tests
- Protected endpoints require token
- Invalid tokens rejected
- Expired tokens rejected
- Missing authorization header
- Wrong authorization scheme
- Insufficient permissions

### 3. Input Validation Tests
- Required fields validation
- Format validation
- Range validation
- Length validation
- Type validation
- Custom validation rules

### 4. Business Logic Tests
- Query handler execution
- Data transformation
- Calculation accuracy
- State changes
- Side effects
- Error conditions

### 5. Data Layer Tests
- Create operations
- Read operations
- Update operations
- Delete operations
- Query filtering
- Sorting and ordering
- Relationships

### 6. Error Handling Tests
- 404 Not Found
- 400 Bad Request
- 401 Unauthorized
- 403 Forbidden
- 500 Internal Server Error
- Exception handling

### 7. Security Tests
- Password hashing
- Token generation
- Token validation
- Authorization enforcement
- CORS headers
- SQL injection prevention

### 8. Performance Tests
- Response time validation
- Large dataset handling
- Query optimization
- Memory efficiency
- Concurrent request handling

---

## Best Practices

### 1. Test Organization
- One test class per controller/handler/repository
- Logical grouping of related tests
- Clear folder structure
- Consistent naming conventions
- Descriptive test method names

### 2. Test Isolation
- Independent test setup
- No test interdependencies
- Fresh database per test class
- Cleanup after tests
- Proper async lifecycle

### 3. Assertion Quality
- One logical assertion per test (or related)
- Use FluentAssertions for readability
- Meaningful assertion messages
- Test important behaviors
- Avoid over-testing

### 4. Test Data
- Minimal required data
- Meaningful test values
- Clear data relationships
- Proper cleanup
- Reusable factories

### 5. Test Performance
- Use in-memory database
- Minimize I/O operations
- Parallel test execution
- Avoid unnecessary sleeps
- Cache static data appropriately

### 6. Test Maintainability
- DRY principle
- Helper methods for common operations
- Clear test intent
- Easy to modify
- Easy to extend
- Good documentation

### 7. Error Messages
- Descriptive failure messages
- Clear assertion reasons
- Actionable error text
- Include relevant data
- Help debugging process

### 8. Mocking Strategy
- Mock external dependencies only
- Test real implementations
- Use fakes for complex setups
- Avoid over-mocking
- Test actual interactions

---

## Common Scenarios

### Scenario 1: Testing JWT Authentication
1. Create test token with JwtTokenHelper
2. Add token to request headers
3. Make API request
4. Verify successful response
5. Test with invalid/expired token
6. Verify authorization error

### Scenario 2: Testing Database Operations
1. Seed test data in fixture
2. Execute repository method
3. Verify results match expectations
4. Add new entity
5. Verify persistence
6. Query updated data

### Scenario 3: Testing Complete Workflow
1. Make login request
2. Extract and use token
3. Make authenticated requests
4. Verify multi-step results
5. Check side effects
6. Verify final state

### Scenario 4: Testing Error Conditions
1. Submit invalid input
2. Verify error response
3. Check error message
4. Verify HTTP status code
5. Test multiple error cases
6. Verify error handling

### Scenario 5: Testing Authorization
1. Create request without token
2. Verify 401 Unauthorized
3. Create request with invalid token
4. Verify rejection
5. Create request with valid token
6. Verify success

---

## Integration Test File Structure

```
ProjectTests/
├── Fixtures/
│   ├── ApiWebApplicationFactory.cs
│   ├── DatabaseFixture.cs
│   └── SeedDataFixture.cs
├── Helpers/
│   ├── JwtTokenHelper.cs
│   ├── TestDataBuilder.cs
│   └── HttpClientExtensions.cs
├── Tests/
│   ├── API/
│   │   ├── AuthControllerTests.cs
│   │   └── DataControllerTests.cs
│   ├── Application/
│   │   ├── QueryHandlerTests.cs
│   │   └── CommandHandlerTests.cs
│   ├── Data/
│   │   ├── RepositoryTests.cs
│   │   └── EntityTests.cs
│   └── E2E/
│       └── WorkflowTests.cs
└── appsettings.test.json
```

---

## Implementation Checklist

When creating integration tests, follow this checklist:

- [ ] Create test project with proper references
- [ ] Configure xUnit and FluentAssertions
- [ ] Create WebApplicationFactory
- [ ] Implement DatabaseFixture
- [ ] Create JwtTokenHelper
- [ ] Write API endpoint tests
- [ ] Write authorization tests
- [ ] Write handler tests
- [ ] Write repository tests
- [ ] Write end-to-end tests
- [ ] Verify test isolation
- [ ] Test async patterns
- [ ] Document test data
- [ ] Test error scenarios
- [ ] Optimize test performance
- [ ] Create documentation
- [ ] Verify all tests pass

---

## Tools & Commands

### Build Test Project
```bash
dotnet build ProjectTests/ProjectTests.csproj
```

### Run All Tests
```bash
dotnet test ProjectTests/ProjectTests.csproj
```

### Run Specific Test Class
```bash
dotnet test ProjectTests/ProjectTests.csproj --filter "ClassName=Project.Tests.AuthControllerTests"
```

### Run Tests with Filter
```bash
dotnet test ProjectTests/ProjectTests.csproj --filter "Name~Login"
```

### Run with Detailed Output
```bash
dotnet test ProjectTests/ProjectTests.csproj --logger "console;verbosity=detailed"
```

### Generate Coverage Report
```bash
dotnet test ProjectTests/ProjectTests.csproj /p:CollectCoverage=true
```

---

## Dependencies

### Required NuGet Packages
- `xunit` - Test framework
- `xunit.runner.visualstudio` - Test runner
- `Microsoft.NET.Test.Sdk` - Test SDK
- `FluentAssertions` - Assertion library
- `Microsoft.AspNetCore.Mvc.Testing` - Web host testing
- `Microsoft.EntityFrameworkCore.InMemory` - In-memory database
- `Moq` - Mocking library (optional)
- `coverlet.collector` - Code coverage

### Project References
- Reference to main project containing application code
- Reference to infrastructure project
- Reference to domain models
- Reference to application services

---

## Troubleshooting Common Issues

### Issue: Tests Not Discovered
- **Solution:** Ensure class/methods are public, marked with [Fact], inherit IAsyncLifetime

### Issue: Database Not Seeded
- **Solution:** Verify InitializeAsync is called, check async patterns

### Issue: JWT Token Invalid
- **Solution:** Verify secret key matches, check token format, validate claims

### Issue: Test Timeouts
- **Solution:** Check for deadlocks in async code, verify database isn't locked

### Issue: Test Interdependencies
- **Solution:** Ensure fresh database per test, avoid shared state

### Issue: HTTP 404 Errors
- **Solution:** Verify route configuration, check controller names, validate URLs

### Issue: Authorization Failures
- **Solution:** Verify token includes required claims, check policy configuration

### Issue: Data Not Persisting
- **Solution:** Verify SaveChangesAsync is called, check transaction handling

---

## Advanced Topics

### Custom Test Attributes
- Create reusable test fixtures
- Implement theory data generators
- Create custom assertions

### Parameterized Tests
- Use [Theory] with [InlineData]
- Use [MemberData] for complex data
- Use [ClassData] for custom providers

### Test Helpers
- Create builder patterns for test data
- Create factory methods
- Create assertion helpers

### Performance Testing
- Measure response times
- Test load scenarios
- Verify query optimization

### Security Testing
- Test authentication flows
- Test authorization boundaries
- Test input sanitization
- Test error information leakage

---

## Resources & References

### Documentation
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [ASP.NET Core Testing](https://docs.microsoft.com/en-us/aspnet/core/test/)
- [Entity Framework Testing](https://docs.microsoft.com/en-us/ef/core/testing/)

### Best Practices
- AAA Pattern (Arrange, Act, Assert)
- Test Isolation Principles
- Single Responsibility in Tests
- Clear Test Names
- Proper Async Patterns

### Example Projects
- Review open-source projects
- Study Microsoft samples
- Analyze clean code examples
- Follow team conventions

---

## Summary

This skill enables professionals to:
1. Create production-ready integration test suites
2. Test all application layers comprehensively
3. Ensure code quality and reliability
4. Implement security and authorization testing
5. Validate complete user workflows
6. Maintain clean, organized test code
7. Document testing approaches
8. Troubleshoot test issues effectively

The skill combines technical knowledge with practical experience to deliver professional-grade integration testing solutions for .NET applications.

---

**Skill Version:** 1.0
**Last Updated:** May 17, 2026
**Framework:** .NET 10.0, C# 13
**Test Framework:** xUnit, FluentAssertions
**Status:** Production Ready

