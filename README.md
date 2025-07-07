# ThreadPilot

A modern microservices-based .NET 9.0 application demonstrating clean architecture, CQRS pattern, and test-driven development for vehicle and insurance management services.

## 🏗️ Architecture & Design Decisions

### Clean Architecture
The solution follows **Clean Architecture** principles with clear separation of concerns:

- **Domain Layer**: Contains entities, value objects, enums, and repository interfaces
- **Application Layer**: Implements business logic, queries, commands, and application services using MediatR
- **Infrastructure Layer**: Handles data persistence, external service integrations, and technical concerns
- **API Layer**: Exposes REST endpoints using FastEndpoints with built-in validation and documentation

### CQRS Pattern with MediatR
- **Command Query Responsibility Segregation (CQRS)** implemented using **MediatR**
- Separate query and command handlers for optimal performance and maintainability
- Request/Response pattern with proper validation and error handling

### Test-Driven Development (TDD)
- Comprehensive unit tests for all business logic
- Integration tests for API endpoints
- Test coverage includes validators, handlers, and endpoint behaviors
- Uses xUnit, FluentAssertions, Moq, and Bogus for robust testing

### Structured Logging
- **Serilog** integration with structured logging
- Service-specific log context enrichment
- Console output with proper formatting for development and production

## � CI/CD Pipeline

### GitHub Actions Workflow
The project includes a comprehensive CI/CD pipeline implemented with GitHub Actions that ensures code quality and deployment readiness:

#### Build and Test Job
- **Multi-stage Build Process**: Automated restore, build, and test execution
- **Test Coverage**: Runs both unit and integration tests with code coverage collection
- **Quality Gates**: Build must pass all tests before proceeding to Docker image creation

#### Docker Image Build Job
- **Multi-service Images**: Builds Docker images for both Vehicle and Insurance APIs
- **Dependency Management**: Only runs after successful test completion
- **Optimized Builds**: Uses Docker BuildKit with GitHub Actions cache for faster builds

#### Pipeline Features
- **Triggers**: Automatically runs on pushes and pull requests to `main` branch
- **Parallel Execution**: Unit and integration tests run in parallel for faster feedback
- **Status Reporting**: Comprehensive build status reporting with clear success/failure indicators
- **Coverage Reports**: Integrates with Codecov for test coverage tracking

#### Workflow Structure
```yaml
# Triggered on push and PRs to main
on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

# Jobs:
# 1. build-and-test: Restore, build, and test all projects
# 2. build-docker-images: Build Docker images for deployment
# 3. build-status: Final status check and reporting
```

The pipeline ensures that:
- All code is properly compiled and tested before deployment
- Docker images are built and ready for deployment
- Test coverage is maintained and reported
- Build failures are caught early in the development cycle

## �🛠️ Technology Stack

### Core Technologies
- **.NET 9.0** - Latest .NET framework
- **C#** - Primary programming language
- **SQLite** - Lightweight database for development and testing
- **Entity Framework Core 9.0** - ORM with code-first approach

### API & Documentation
- **FastEndpoints** - High-performance, minimal API framework with built-in validation
- **Scalar/OpenAPI** - Modern API documentation and testing interface
- **API Versioning** - FastEndpoints supports URL segment versioning (e.g., `/v1/vehicles`, `/v2/vehicles`) and header-based versioning

### API Versioning Implementation
FastEndpoints supports multiple versioning strategies:
```csharp
// URL Segment Versioning
[Version(1)]
public class GetVehicleV1Endpoint : Endpoint<GetVehicleRequest, VehicleResponse>
{
    public override void Configure()
    {
        Get("/v1/vehicles/{id}");
    }
}

// Header-based Versioning
[Version(2, HeaderName = "X-API-Version")]
public class GetVehicleV2Endpoint : Endpoint<GetVehicleRequest, VehicleResponseV2>
{
    public override void Configure()
    {
        Get("/vehicles/{id}");
    }
}
```

### Patterns & Libraries
- **MediatR** - Mediator pattern implementation for CQRS
- **FluentValidation** - Type-safe validation rules
- **AutoMapper** - Object-to-object mapping
- **Serilog** - Structured logging framework

### Feature Flags
- **Microsoft.FeatureManagement** - Feature flag management for controlling application behavior
- **EnableVehicleDetailsIntegration** - Controls whether vehicle details are fetched and included in insurance responses

#### Feature Flag Configuration
The application uses Microsoft.FeatureManagement to enable/disable features at runtime:

```json
{
  "FeatureManagement": {
    "EnableVehicleDetailsIntegration": true
  }
}
```

When `EnableVehicleDetailsIntegration` is enabled, the Insurance API will:
- Fetch vehicle details from the Vehicle API for car insurance policies
- Include vehicle information (make, model, year, color) in the insurance response
- Handle graceful fallbacks when the Vehicle API is unavailable

When disabled, car insurance policies will not include vehicle details, improving response times and reducing external dependencies.

### Resilience & HTTP
- **Microsoft.Extensions.Http.Resilience** - Built-in resilience patterns with retry policies
- **HttpClient** - Configured with timeout and resilience handlers

### Testing
- **xUnit** - Primary testing framework
- **FluentAssertions** - Readable test assertions
- **Moq** - Advanced mocking framework for creating test doubles of dependencies
- **Bogus** - Test data generation with realistic fake data
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing with in-memory test server

### DevOps & Containerization
- **Docker** - Containerization with multi-stage builds
- **Docker Compose** - Multi-service orchestration
- **Health Checks** - Built-in health monitoring

## 🚀 Running the Solution/Developer Onboarding

### Prerequisites
- .NET 9.0 SDK
- Docker and Docker Compose

### Using Docker Compose (Recommended)
```bash
# Clone the repository
git clone <repository-url>
cd thread-pilot

# Build and run all services
docker-compose up --build

# Run in detached mode
docker-compose up -d --build
```

### Running Locally
```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run Vehicle API (Terminal 1)
dotnet run --project src/Services/Vehicle/Vehicle.Api

# Run Insurance API (Terminal 2)
dotnet run --project src/Services/Insurance/Insurance.Api
```

### Development Workflow
- **Code Style**: Follow .NET coding conventions and use EditorConfig
- **Testing**: Write tests before implementing features (TDD approach)
- **API Documentation**: Update OpenAPI specifications when adding new endpoints
- **Logging**: Use structured logging with appropriate log levels
- **CI/CD Integration**: All changes are validated through the automated pipeline
  - Pull requests trigger full build and test validation
  - Merges to `main` branch build deployment-ready Docker images
  - Test coverage is automatically collected and reported
- **Database Changes**: Create EF Core migrations for schema changes

### Using VS Code Tasks
The project includes pre-configured VS Code tasks:
- `build` - Build the entire solution
- `build-vehicle` - Build only Vehicle API
- `build-insurance` - Build only Insurance API
- `watch-vehicle` - Run Vehicle API in watch mode
- `watch-insurance` - Run Insurance API in watch mode

## 🌐 Service Endpoints

### Vehicle API (Port 5001)
- `GET /vehicles/{registrationNumber}` - Get vehicle by registration number
- `GET /health` - Health check endpoint
- API Documentation: `http://localhost:5001/scalar/v1`

### Insurance API (Port 5002)
- `POST /insurances` - Get person's insurance policies
- `GET /health` - Health check endpoint
- API Documentation: `http://localhost:5002/scalar/v1`

## 📊 Seed Data

The application includes pre-seeded test data:

### Vehicles
- **ABC123** - Volvo XC90 (2022, Black)
- **DEF456** - BMW X5 (2021, White)
- **GHI789** - Audi Q7 (2023, Silver)

### Insurance Policies
- **Personal ID: 19620421-3323**
  - Personal Health Insurance (€20/month)
  - Pet Insurance (€10/month)
- **Personal ID: 19631002-4622**
  - Car Insurance for ABC123 (€30/month)

## 🧪 Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Vehicle.Tests/Vehicle.UnitTests
dotnet test tests/Insurance.Tests/Insurance.IntegrationTests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure
- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test complete API workflows with in-memory database
- **Test Data Builders**: Provide consistent test data generation

### Mocking Strategy
The project uses **Moq** for creating test doubles and isolating units under test:

```csharp
// Repository mocking for isolated unit testing
var mockVehicleRepository = new Mock<IVehicleRepository>();
mockVehicleRepository.Setup(r => r.GetByRegistrationNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Vehicle { RegistrationNumber = "ABC123" });

// External service mocking for integration tests
var mockVehicleService = new Mock<IVehicleService>();
mockVehicleService.Setup(s => s.GetVehicleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new VehicleResponse { RegistrationNumber = "ABC123", Make = "Volvo" });

// Validator mocking for endpoint testing
var mockValidator = new Mock<IValidator<GetPersonInsurancesRequest>>();
mockValidator.Setup(v => v.ValidateAsync(It.IsAny<GetPersonInsurancesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
```

**Key Mocking Patterns:**
- **Repository Pattern**: Mock data access layers for fast, isolated unit tests
- **External Services**: Mock HTTP clients and external APIs to avoid network dependencies
- **Validators**: Mock validation logic to test business logic separately

## 🛡️ Error Handling & Resilience

### Error Handling Strategy
- **No Exception Throwing**: Uses FastEndpoints' typed results pattern
- **Graceful Degradation**: Services continue operating even when dependencies fail
- **Structured Error Responses**: Consistent error format across all endpoints
- **Validation Errors**: Clear, actionable validation messages

### Resilience Features
- **HTTP Resilience**: Built-in retry policies for external service calls
- **Circuit Breaker**: Prevents cascading failures
- **Timeout Configuration**: Configurable timeouts for all HTTP operations
- **Health Checks**: Proactive monitoring of service health

## 🔧 Extensibility

### Clean Architecture Benefits
- **Loose Coupling**: Easy to swap implementations
- **Dependency Injection**: All dependencies are injected and testable
- **Repository Pattern**: Database-agnostic data access
- **Interface Segregation**: Well-defined contracts between layers

### Adding New Features
1. **Domain**: Define entities, value objects, and repository interfaces
2. **Application**: Create queries/commands with MediatR handlers
3. **Infrastructure**: Implement repository and external service integrations
4. **API**: Add FastEndpoints with validation and documentation

### Configuration
- Centralized package management with `Directory.Packages.props`
- Environment-specific configuration with `appsettings.json`
- Docker environment variables for deployment flexibility

## 🔒 Security Considerations

### Current Implementation
- **CORS Configuration**: Configured for development (should be restricted in production)
- **Input Validation**: Comprehensive validation using FluentValidation
- **SQL Injection Prevention**: Entity Framework Core with parameterized queries
- **Health Check Security**: Basic health endpoints for monitoring

## 📁 Project Structure

```
ThreadPilot/
├── src/
│   ├── Services/
│   │   ├── Vehicle/           # Vehicle microservice
│   │   └── Insurance/         # Insurance microservice
│   └── Shared/
│       └── ThreadPilot.Common/ # Shared utilities
├── tests/                     # Test projects
├── docker/                    # Docker configuration
└── docker-compose.yml         # Service orchestration
```

## Personal Reflection

### Similar Experience

I've contributed to multiple microservices-based projects, primarily focused on designing and implementing integration layers that bridge modern services with legacy systems. At Rebtel, we faced the challenge of interfacing a monolithic legacy backend with a newly developed microservices architecture. This involved addressing issues such as eventual consistency, data synchronization all while maintaining high availability and fault tolerance.

### Interesting Challenges

The most engaging part of this assignment was designing an extensible solution and implementing resilient service-to-service communication. Ensuring the Insurance Service could gracefully handle failures from the Vehicle Service, while still providing a smooth user experience. This required timeout policies and fallback strategies.

### Future Enhancements
- **Authentication**: Implement JWT/OAuth2 integration for endpoints
- **Authorization**: Add Role-based access control for endpoints
- **Observability**: Distributed tracing with OpenTelemetry for better observability
- **API Versioning**: API versioning strategy for backward compatibility