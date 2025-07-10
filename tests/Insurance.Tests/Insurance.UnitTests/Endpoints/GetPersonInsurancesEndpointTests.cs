using FluentAssertions;
using Insurance.Api.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System.Reflection;
using ProblemDetails = FastEndpoints.ProblemDetails;
using FluentValidation;
using FluentValidation.Results;
using DomainInsuranceType = Insurance.Core.Enums.InsuranceType;
using Insurance.Api.Contracts;
using Insurance.Core.Queries.GetPersonInsurances;
using Insurance.Core.Common;

namespace Insurance.UnitTests.Endpoints;

public class GetPersonInsurancesEndpointTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IValidator<GetPersonInsurancesRequest>> _validatorMock;
    private readonly GetPersonInsurancesEndpoint _endpoint;

    public GetPersonInsurancesEndpointTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _validatorMock = new Mock<IValidator<GetPersonInsurancesRequest>>();
        _endpoint = new GetPersonInsurancesEndpoint(_mediatorMock.Object, _validatorMock.Object);
        
        // Setup HttpContext for the endpoint using reflection
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/insurances";
        
        // Set HttpContext via reflection since it's read-only
        var httpContextProperty = typeof(GetPersonInsurancesEndpoint).BaseType!
            .GetProperty("HttpContext", BindingFlags.Public | BindingFlags.Instance);
        httpContextProperty?.SetValue(_endpoint, httpContext);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPersonHasInsurances_ShouldReturnOkWithResult()
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
                new InsuranceResponse { Type = DomainInsuranceType.PersonalHealth, MonthlyCost = 100.00m },
                new InsuranceResponse { Type = DomainInsuranceType.Pet, MonthlyCost = 50.00m }
            },
            TotalMonthlyCost = 150.00m
        };

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        _mediatorMock.Setup(m => m.Send(It.Is<GetPersonInsurancesQuery>(q => q.PersonalIdentificationNumber == expectedQuery.PersonalIdentificationNumber), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(personInsurancesResult);

        // Act
        var result = await _endpoint.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Results<Ok<PersonInsurancesResult>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>>();
        
        var okResult = result.Result as Ok<PersonInsurancesResult>;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(personInsurancesResult);
        
        _validatorMock.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.Is<GetPersonInsurancesQuery>(q => q.PersonalIdentificationNumber == pin), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPersonHasNoInsurances_ShouldReturnOkWithEmptyResult()
    {
        // Arrange
        var pin = TestDataBuilder.GenerateSwedishPin();
        var request = new GetPersonInsurancesRequest { PersonalIdentificationNumber = pin };
        var expectedQuery = new GetPersonInsurancesQuery(pin);

        var emptyPersonInsurancesResult = new PersonInsurancesResult
        {
            PersonalIdentificationNumber = pin,
            Insurances = new List<InsuranceResponse>(),
            TotalMonthlyCost = 0
        };

        _validatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        _mediatorMock.Setup(m => m.Send(It.Is<GetPersonInsurancesQuery>(q => q.PersonalIdentificationNumber == expectedQuery.PersonalIdentificationNumber), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(emptyPersonInsurancesResult);

        // Act
        var result = await _endpoint.ExecuteAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Results<Ok<PersonInsurancesResult>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>>();
        
        var okResult = result.Result as Ok<PersonInsurancesResult>;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(emptyPersonInsurancesResult);
        okResult!.Value.Should().NotBeNull();
        okResult!.Value!.Insurances.Should().BeEmpty();
        okResult!.Value!.TotalMonthlyCost.Should().Be(0);
        
        _validatorMock.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.Is<GetPersonInsurancesQuery>(q => q.PersonalIdentificationNumber == pin), It.IsAny<CancellationToken>()), Times.Once);
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
        result.Should().BeOfType<Results<Ok<PersonInsurancesResult>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>>();
        
        var badRequestResult = result.Result as BadRequest<ProblemDetails>;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().NotBeNull();
        badRequestResult!.Value!.Status.Should().Be(400);
        badRequestResult.Value.Detail.Should().Be("Invalid Swedish personal identification number format. Expected format: YYYYMMDD-NNNN or YYMMDD-NNNN");
        badRequestResult.Value.Instance.Should().Be("/insurances");
        
        _validatorMock.Verify(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetPersonInsurancesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
