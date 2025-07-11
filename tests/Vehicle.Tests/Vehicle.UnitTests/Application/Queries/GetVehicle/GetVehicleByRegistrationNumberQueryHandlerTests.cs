using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Vehicle.Core.Queries.GetVehicle;
using Vehicle.Core.Repositories;
using Vehicle.Core.ValueObjects;

namespace Vehicle.UnitTests.Application.Queries.GetVehicle;

public class GetVehicleByRegistrationNumberQueryHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly Mock<IValidator<GetVehicleByRegistrationNumberQuery>> _validatorMock;
    private readonly Mock<ILogger<GetVehicleByRegistrationNumberQueryHandler>> _loggerMock;
    private readonly GetVehicleByRegistrationNumberQueryHandler _handler;


    public GetVehicleByRegistrationNumberQueryHandlerTests()
    {
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _validatorMock = new Mock<IValidator<GetVehicleByRegistrationNumberQuery>>();
        _loggerMock = new Mock<ILogger<GetVehicleByRegistrationNumberQueryHandler>>();
        _handler = new GetVehicleByRegistrationNumberQueryHandler(_vehicleRepositoryMock.Object, _validatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRegistrationNumberAndVehicleExists_ReturnsVehicleResult()
    {
        // Arrange
        var registrationNumber = "ABC123";
        var query = new GetVehicleByRegistrationNumberQuery(registrationNumber);
        var cancellationToken = CancellationToken.None;

        var vehicle = new Core.Entities.Vehicle(
            new RegistrationNumber(registrationNumber),
            make: "Toyota",
            model: "Camry",
            year: 2020,
            color: "Blue"
        );

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetVehicleByRegistrationNumberQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _vehicleRepositoryMock
            .Setup(r => r.GetByRegistrationNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.RegistrationNumber.Should().Be(registrationNumber);
        result.Make.Should().Be("Toyota");
        result.Model.Should().Be("Camry");
        result.Year.Should().Be(2020);
        result.Color.Should().Be("Blue");

        _vehicleRepositoryMock.Verify(r => r.GetByRegistrationNumberAsync(It.Is<string>(rn => rn == registrationNumber), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidRegistrationNumberButVehicleNotFound_ReturnsNull()
    {
        // Arrange
        var registrationNumber = "XYZ789";
        var query = new GetVehicleByRegistrationNumberQuery(registrationNumber);
        var cancellationToken = CancellationToken.None;

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetVehicleByRegistrationNumberQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _vehicleRepositoryMock
            .Setup(r => r.GetByRegistrationNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Core.Entities.Vehicle?)null);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().BeNull();

        _vehicleRepositoryMock.Verify(r => r.GetByRegistrationNumberAsync(
            It.Is<string>(rn => rn == registrationNumber), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRegistrationNumberFormat_ReturnsNull()
    {
        // Arrange
        var invalidRegistrationNumber = "A"; // Too short, doesn't match ABC123 pattern
        var query = new GetVehicleByRegistrationNumberQuery(invalidRegistrationNumber);
        var cancellationToken = CancellationToken.None;

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("RegistrationNumber", "Registration number must not exceed 6 characters.")
        };
        var validationResult = new ValidationResult(validationFailures);

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetVehicleByRegistrationNumberQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().BeNull();

        // Verify that repository was never called due to invalid format
        _vehicleRepositoryMock.Verify(r => r.GetByRegistrationNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
