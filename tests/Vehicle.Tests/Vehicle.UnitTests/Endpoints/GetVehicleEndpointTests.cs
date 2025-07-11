using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Vehicle.Api.Endpoints;
using Vehicle.Core.Common;
using Vehicle.Core.Queries.GetVehicle;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Reflection;
using ProblemDetails = FastEndpoints.ProblemDetails;
using FluentAssertions;

namespace Vehicle.UnitTests.Endpoints;

public class GetVehicleEndpointTests
{
    private readonly Mock<IGetVehicleByRegistrationNumberQueryHandler> _handlerMock;
    private readonly Mock<ILogger<GetVehicleEndpoint>> _loggerMock;
    private readonly GetVehicleEndpoint _endpoint;

    public GetVehicleEndpointTests()
    {
        _handlerMock = new Mock<IGetVehicleByRegistrationNumberQueryHandler>();
        _loggerMock = new Mock<ILogger<GetVehicleEndpoint>>();
        _endpoint = new GetVehicleEndpoint(_handlerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRegistrationNumber_ReturnsVehicle()
    {
        // Arrange
        var registrationNumber = "ABC123";
        var expectedVehicle = new VehicleResult(
            RegistrationNumber: registrationNumber,
            Make: "Toyota",
            Model: "Camry",
            Year: 2020,
            Color: "Blue"
        );

        _handlerMock
            .Setup(h => h.Handle(It.IsAny<GetVehicleByRegistrationNumberQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVehicle);

        // Setup HttpContext for the endpoint using reflection
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = $"/vehicles/{registrationNumber}";
        
        // Set HttpContext via reflection since it's read-only
        var httpContextProperty = typeof(GetVehicleEndpoint).BaseType!
            .GetProperty("HttpContext", BindingFlags.Public | BindingFlags.Instance);
        httpContextProperty?.SetValue(_endpoint, httpContext);

        // Setup route values
        SetupRouteValue(registrationNumber);

        // Act
        var result = await _endpoint.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().BeOfType<Results<Ok<VehicleResult>, NotFound<ProblemDetails>>>();
        
        var okResult = result.Result as Ok<VehicleResult>;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(expectedVehicle);
        
        _handlerMock.Verify(h => h.Handle(
            It.Is<GetVehicleByRegistrationNumberQuery>(q => q.RegistrationNumber == registrationNumber),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidRegistrationNumber_ReturnsNotFound()
    {
        // Arrange
        var invalidRegistrationNumber = "A"; // Too short, should fail validation

        // Setup HttpContext for the endpoint using reflection
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = $"/vehicles/{invalidRegistrationNumber}";
        
        // Set HttpContext via reflection since it's read-only
        var httpContextProperty = typeof(GetVehicleEndpoint).BaseType!
            .GetProperty("HttpContext", BindingFlags.Public | BindingFlags.Instance);
        httpContextProperty?.SetValue(_endpoint, httpContext);

        SetupRouteValue(invalidRegistrationNumber);

        // Act
        var result = await _endpoint.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().BeOfType<Results<Ok<VehicleResult>, NotFound<ProblemDetails>>>();
        
        var notFoundResult = result.Result as NotFound<ProblemDetails>;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.Value.Should().NotBeNull();
        notFoundResult!.Value!.Status.Should().Be(404);
        notFoundResult.Value.Detail.Should().Be("registrationNumber must be between 2 and 20 characters.");
        notFoundResult.Value.Instance.Should().Be($"/vehicles/{invalidRegistrationNumber}");
        
        _handlerMock.Verify(h => h.Handle(It.IsAny<GetVehicleByRegistrationNumberQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentVehicle_ReturnsNotFound()
    {
        // Arrange
        var registrationNumber = "XYZ999";

        _handlerMock
            .Setup(h => h.Handle(It.IsAny<GetVehicleByRegistrationNumberQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleResult?)null);

        // Setup HttpContext for the endpoint using reflection
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = $"/vehicles/{registrationNumber}";
        
        // Set HttpContext via reflection since it's read-only
        var httpContextProperty = typeof(GetVehicleEndpoint).BaseType!
            .GetProperty("HttpContext", BindingFlags.Public | BindingFlags.Instance);
        httpContextProperty?.SetValue(_endpoint, httpContext);

        SetupRouteValue(registrationNumber);

        // Act
        var result = await _endpoint.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().BeOfType<Results<Ok<VehicleResult>, NotFound<ProblemDetails>>>();
        
        var notFoundResult = result.Result as NotFound<ProblemDetails>;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.Value.Should().NotBeNull();
        notFoundResult!.Value!.Status.Should().Be(404);
        notFoundResult.Value.Detail.Should().Be($"No vehicle found with registration number {registrationNumber}");
        notFoundResult.Value.Instance.Should().Be($"/vehicles/{registrationNumber}");
        
        _handlerMock.Verify(h => h.Handle(
            It.Is<GetVehicleByRegistrationNumberQuery>(q => q.RegistrationNumber == registrationNumber),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHandlerThrowsException_ShouldPropagateException()
    {
        // Arrange
        var registrationNumber = "ABC123";
        var expectedException = new InvalidOperationException("Database connection failed");

        _handlerMock
            .Setup(h => h.Handle(It.IsAny<GetVehicleByRegistrationNumberQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Setup HttpContext for the endpoint using reflection
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = $"/vehicles/{registrationNumber}";
        
        // Set HttpContext via reflection since it's read-only
        var httpContextProperty = typeof(GetVehicleEndpoint).BaseType!
            .GetProperty("HttpContext", BindingFlags.Public | BindingFlags.Instance);
        httpContextProperty?.SetValue(_endpoint, httpContext);

        SetupRouteValue(registrationNumber);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _endpoint.ExecuteAsync(CancellationToken.None));
        
        exception.Should().Be(expectedException);
        exception.Message.Should().Be("Database connection failed");
        
        _handlerMock.Verify(h => h.Handle(It.IsAny<GetVehicleByRegistrationNumberQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationTokenIsCancelled_ShouldThrowOperationCancelledException()
    {
        // Arrange
        var registrationNumber = "ABC123";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _handlerMock
            .Setup(h => h.Handle(It.IsAny<GetVehicleByRegistrationNumberQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Setup HttpContext for the endpoint using reflection
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = $"/vehicles/{registrationNumber}";
        
        // Set HttpContext via reflection since it's read-only
        var httpContextProperty = typeof(GetVehicleEndpoint).BaseType!
            .GetProperty("HttpContext", BindingFlags.Public | BindingFlags.Instance);
        httpContextProperty?.SetValue(_endpoint, httpContext);

        SetupRouteValue(registrationNumber);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _endpoint.ExecuteAsync(cancellationTokenSource.Token));
        
        _handlerMock.Verify(h => h.Handle(It.IsAny<GetVehicleByRegistrationNumberQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyRegistrationNumber_ReturnsNotFound()
    {
        // Arrange
        var emptyRegistrationNumber = "";

        // Setup HttpContext for the endpoint using reflection
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = $"/vehicles/{emptyRegistrationNumber}";
        
        // Set HttpContext via reflection since it's read-only
        var httpContextProperty = typeof(GetVehicleEndpoint).BaseType!
            .GetProperty("HttpContext", BindingFlags.Public | BindingFlags.Instance);
        httpContextProperty?.SetValue(_endpoint, httpContext);

        SetupRouteValue(emptyRegistrationNumber);

        // Act
        var result = await _endpoint.ExecuteAsync(CancellationToken.None);

        // Assert
        result.Should().BeOfType<Results<Ok<VehicleResult>, NotFound<ProblemDetails>>>();
        
        var notFoundResult = result.Result as NotFound<ProblemDetails>;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.Value.Should().NotBeNull();
        notFoundResult!.Value!.Status.Should().Be(404);
        notFoundResult.Value.Detail.Should().Be("registrationNumber is required.");
        notFoundResult.Value.Instance.Should().Be($"/vehicles/{emptyRegistrationNumber}");
        
        _handlerMock.Verify(h => h.Handle(It.IsAny<GetVehicleByRegistrationNumberQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupRouteValue(string registrationNumber)
    {
        var routeValues = new Dictionary<string, object?> { { "registrationNumber", registrationNumber } };
        _endpoint.HttpContext.Request.RouteValues = new Microsoft.AspNetCore.Routing.RouteValueDictionary(routeValues);
    }
}
