using System;
using AutoMapper;
using FluentAssertions;
using Moq;
using Vehicle.Application.Common;
using Vehicle.Application.Queries.GetVehicle;
using Vehicle.Domain.Repositories;
using Vehicle.Domain.ValueObjects;

namespace Vehicle.UnitTests.Application.Queries.GetVehicle;

public class GetVehicleByRegistrationNumberQueryHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetVehicleByRegistrationNumberQueryHandler _handler;

    public GetVehicleByRegistrationNumberQueryHandlerTests()
    {
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetVehicleByRegistrationNumberQueryHandler(_vehicleRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRegistrationNumberAndVehicleExists_ReturnsVehicleResult()
    {
        // Arrange
        var registrationNumber = "ABC123";
        var query = new GetVehicleByRegistrationNumberQuery(registrationNumber);
        var cancellationToken = CancellationToken.None;

        var vehicle = new Domain.Entities.Vehicle(
            new RegistrationNumber(registrationNumber),
            make: "Toyota",
            model: "Camry",
            year: 2020,
            color: "Blue"
        );

        var expectedResult = new VehicleResult(
            RegistrationNumber: registrationNumber,
            Make: "Toyota",
            Model: "Camry",
            Year: 2020,
            Color: "Blue"
        );

        _vehicleRepositoryMock
            .Setup(r => r.GetByRegistrationNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _mapperMock
            .Setup(m => m.Map<VehicleResult>(vehicle))
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResult);

        _vehicleRepositoryMock.Verify(r => r.GetByRegistrationNumberAsync(It.Is<string>(rn => rn == registrationNumber), It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<VehicleResult>(vehicle), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidRegistrationNumberButVehicleNotFound_ReturnsNull()
    {
        // Arrange
        var registrationNumber = "XYZ789";
        var query = new GetVehicleByRegistrationNumberQuery(registrationNumber);
        var cancellationToken = CancellationToken.None;

        _vehicleRepositoryMock
            .Setup(r => r.GetByRegistrationNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Vehicle?)null);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().BeNull();

        _vehicleRepositoryMock.Verify(r => r.GetByRegistrationNumberAsync(
            It.Is<string>(rn => rn == registrationNumber), It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<VehicleResult>(It.IsAny<Domain.Entities.Vehicle>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidRegistrationNumberFormat_ReturnsNull()
    {
        // Arrange
        var invalidRegistrationNumber = "A"; // Too short, doesn't match ABC123 pattern
        var query = new GetVehicleByRegistrationNumberQuery(invalidRegistrationNumber);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().BeNull();

        // Verify that repository was never called due to invalid format
        _vehicleRepositoryMock.Verify(r => r.GetByRegistrationNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mapperMock.Verify(m => m.Map<VehicleResult>(It.IsAny<Domain.Entities.Vehicle>()), Times.Never);
    }
}
