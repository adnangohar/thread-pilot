using AutoMapper;
using FluentAssertions;
using Insurance.Api.Endpoints;
using Insurance.Application.Common;
using Insurance.Application.Queries.GetPersonInsurances;
using Insurance.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System.Reflection;
using ProblemDetails = FastEndpoints.ProblemDetails;
using FluentValidation;
using FluentValidation.Results;

namespace Insurance.UnitTests.Endpoint;

public class GetPersonInsurancesEndpointTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<GetPersonInsurancesRequest>> _validatorMock;
    private readonly GetPersonInsurancesEndpoint _endpoint;

    public GetPersonInsurancesEndpointTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<GetPersonInsurancesRequest>>();
        _endpoint = new GetPersonInsurancesEndpoint(_mediatorMock.Object, _mapperMock.Object, _validatorMock.Object);
        
        // Setup HttpContext for the endpoint using reflection
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/insurances/get-by-pin";
        
        // Set HttpContext via reflection since it's read-only
        var httpContextProperty = typeof(GetPersonInsurancesEndpoint).BaseType!
            .GetProperty("HttpContext", BindingFlags.Public | BindingFlags.Instance);
        httpContextProperty?.SetValue(_endpoint, httpContext);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPersonHasInsurances_ShouldReturnOkWithMappedResponse()
    {
        // Arrange
        var pin = TestDataBuilder.GenerateSwedishPin();
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = pin };
        var expectedQuery = new GetPersonInsurancesQuery(pin);
        
        var personInsurancesResult = new PersonInsurancesResult
        {
            PersonalIdentificationNumber = pin,
            Insurances = new List<InsuranceResponse>
            {
                new PersonalHealthInsuranceResponse { Type = "Health", MonthlyCost = 100.00m, CoverageLevel = "Premium" },
                new PetInsuranceResponse { Type = "Pet", MonthlyCost = 50.00m, PetName = "Buddy", PetType = "Dog" }
            },
            TotalMonthlyCost = 150.00m
        };

        var expectedResponse = new PersonInsurancesResponse
        {
            PersonalIdentificationNumber = pin,
            Insurances = new List<InsuranceResponse>
            {
                new PersonalHealthInsuranceResponse { Type = "Health", MonthlyCost = 100.00m, CoverageLevel = "Premium" },
                new PetInsuranceResponse { Type = "Pet", MonthlyCost = 50.00m, PetName = "Buddy", PetType = "Dog" }
            },
            TotalMonthlyCost = 150.00m
        };

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        _mediatorMock.Setup(m => m.Send(It.Is<GetPersonInsurancesQuery>(q => q.PersonalIdentificationNumber == expectedQuery.PersonalIdentificationNumber), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(personInsurancesResult);
        
        _mapperMock.Setup(m => m.Map<PersonInsurancesResponse>(personInsurancesResult))
                   .Returns(expectedResponse);

        // Act
        var result = await _endpoint.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Results<Ok<PersonInsurancesResponse>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>>();
        
        var okResult = result.Result as Ok<PersonInsurancesResponse>;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        
        _validatorMock.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.Is<GetPersonInsurancesQuery>(q => q.PersonalIdentificationNumber == pin), It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<PersonInsurancesResponse>(personInsurancesResult), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPersonHasNoInsurances_ShouldReturnNotFoundWithProblemDetails()
    {
        // Arrange
        var pin = TestDataBuilder.GenerateSwedishPin();
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = pin };
        var expectedQuery = new GetPersonInsurancesQuery(pin);

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        _mediatorMock.Setup(m => m.Send(It.Is<GetPersonInsurancesQuery>(q => q.PersonalIdentificationNumber == expectedQuery.PersonalIdentificationNumber), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PersonInsurancesResult?)null);

        // Act
        var result = await _endpoint.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Results<Ok<PersonInsurancesResponse>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>>();
        
        var notFoundResult = result.Result as NotFound<ProblemDetails>;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.Value.Should().NotBeNull();
        notFoundResult!.Value!.Status.Should().Be(404);
        notFoundResult.Value.Detail.Should().Be($"No insurances found for person with identification number {pin}");
        notFoundResult.Value.Instance.Should().Be("/insurances/get-by-pin");
        
        _validatorMock.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.Is<GetPersonInsurancesQuery>(q => q.PersonalIdentificationNumber == pin), It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<PersonInsurancesResponse>(It.IsAny<PersonInsurancesResult>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMediatorThrowsException_ShouldPropagateException()
    {
        // Arrange
        var pin = TestDataBuilder.GenerateSwedishPin();
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = pin };
        var expectedException = new InvalidOperationException("Database connection failed");

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPersonInsurancesQuery>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _endpoint.ExecuteAsync(request, CancellationToken.None));
        
        exception.Should().Be(expectedException);
        exception.Message.Should().Be("Database connection failed");
        
        _validatorMock.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetPersonInsurancesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<PersonInsurancesResponse>(It.IsAny<PersonInsurancesResult>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMapperThrowsException_ShouldPropagateException()
    {
        // Arrange
        var pin = TestDataBuilder.GenerateSwedishPin();
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = pin };
        var personInsurancesResult = new PersonInsurancesResult
        {
            PersonalIdentificationNumber = pin,
            Insurances = new List<InsuranceResponse>(),
            TotalMonthlyCost = 0
        };
        var expectedException = new AutoMapperMappingException("Mapping failed");

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPersonInsurancesQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(personInsurancesResult);
        
        _mapperMock.Setup(m => m.Map<PersonInsurancesResponse>(It.IsAny<PersonInsurancesResult>()))
                   .Throws(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(() => 
            _endpoint.ExecuteAsync(request, CancellationToken.None));
        
        exception.Should().Be(expectedException);
        exception.Message.Should().Be("Mapping failed");
        
        _validatorMock.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetPersonInsurancesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<PersonInsurancesResponse>(personInsurancesResult), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationTokenIsCancelled_ShouldThrowOperationCancelledException()
    {
        // Arrange
        var pin = TestDataBuilder.GenerateSwedishPin();
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = pin };
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPersonInsurancesQuery>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => 
            _endpoint.ExecuteAsync(request, cancellationTokenSource.Token));
        
        _validatorMock.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetPersonInsurancesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<PersonInsurancesResponse>(It.IsAny<PersonInsurancesResult>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidationFails_ShouldReturnBadRequestWithValidationErrors()
    {
        // Arrange
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = "invalid-pin" };
        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("PersonalIdentificationNumber", "Invalid Swedish personal identification number format. Expected format: YYYYMMDD-NNNN or YYMMDD-NNNN")
        };
        var validationResult = new ValidationResult(validationErrors);

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(validationResult);

        // Act
        var result = await _endpoint.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Results<Ok<PersonInsurancesResponse>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>>();
        
        var badRequestResult = result.Result as BadRequest<ProblemDetails>;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().NotBeNull();
        badRequestResult!.Value!.Status.Should().Be(400);
        badRequestResult.Value.Detail.Should().Be("Invalid Swedish personal identification number format. Expected format: YYYYMMDD-NNNN or YYMMDD-NNNN");
        badRequestResult.Value.Instance.Should().Be("/insurances/get-by-pin");
        
        _validatorMock.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetPersonInsurancesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        _mapperMock.Verify(m => m.Map<PersonInsurancesResponse>(It.IsAny<PersonInsurancesResult>()), Times.Never);
    }
}
