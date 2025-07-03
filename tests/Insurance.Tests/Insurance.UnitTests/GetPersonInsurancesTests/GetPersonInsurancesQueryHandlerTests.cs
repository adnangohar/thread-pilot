using AutoMapper;
using FluentAssertions;
using Insurance.Application.Interfaces;
using Insurance.Application.Queries.GetPersonInsurances;
using Insurance.Contracts;
using Insurance.Domain.Entities;
using Insurance.Domain.Repositories;
using Insurance.Domain.ValueObjects;
using Moq;
using Vehicle.Contracts;

namespace Insurance.UnitTests.GetPersonInsurancesTests;

public class GetPersonInsurancesQueryHandlerTests
{
    private readonly Mock<IInsuranceRepository> _mockInsuranceRepository;
    private readonly Mock<IVehicleService> _mockVehicleService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetPersonInsurancesQueryHandler _handler;

    public GetPersonInsurancesQueryHandlerTests()
    {
        _mockInsuranceRepository = new Mock<IInsuranceRepository>();
        _mockVehicleService = new Mock<IVehicleService>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetPersonInsurancesQueryHandler(
            _mockInsuranceRepository.Object,
            _mockVehicleService.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WhenPersonHasNoInsurances_ShouldReturnNull()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789");
        var pin = new PersonalIdentificationNumber("123456789");
        var emptyInsurances = new List<Insurance.Domain.Entities.Insurance>();

        _mockInsuranceRepository
            .Setup(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()))
            .ReturnsAsync(emptyInsurances);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _mockInsuranceRepository.Verify(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidPersonalIdentificationNumber_ShouldReturnPersonInsurancesResult()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789");
        var pin = new PersonalIdentificationNumber("123456789");
        var carInsurance = new CarInsurance(pin, "ABC123");
        var insurances = new List<Insurance.Domain.Entities.Insurance> { carInsurance };

        var carInsuranceResponse = new CarInsuranceResponse
        {
            Type = "Car",
            MonthlyCost = 30m,
            Vehicle = new VehicleResponse
            {
                RegistrationNumber = "ABC123",
                Make = "Toyota",
                Model = "Camry"
            }
        };

        var vehicleResponse = new VehicleResponse
        {
            RegistrationNumber = "ABC123",
            Make = "Toyota",
            Model = "Camry"
        };

        _mockInsuranceRepository
            .Setup(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<CarInsuranceResponse>(It.IsAny<CarInsurance>()))
            .Returns(carInsuranceResponse);

        _mockVehicleService
            .Setup(x => x.GetVehicleInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PersonalIdentificationNumber.Should().Be("123456789");
        result.Insurances.Should().HaveCount(1);
        result.TotalMonthlyCost.Should().Be(30m);
        result.Insurances.First().Should().BeOfType<CarInsuranceResponse>();

        var carResult = (CarInsuranceResponse)result.Insurances.First();
        carResult.Vehicle.Should().NotBeNull();
        carResult.Vehicle!.RegistrationNumber.Should().Be("ABC123");
    }

    [Fact]
    public async Task Handle_WithMultipleInsurances_ShouldReturnAllInsurancesAndCalculateTotalCost()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789");
        var pin = new PersonalIdentificationNumber("123456789");
        var carInsurance = new CarInsurance(pin, "ABC123");
        var petInsurance = new PetInsurance(pin, "Fluffy", "Dog");
        var healthInsurance = new PersonalHealthInsurance(pin, "Premium");
        var insurances = new List<Insurance.Domain.Entities.Insurance> { carInsurance, petInsurance, healthInsurance };

        var carInsuranceResponse = new CarInsuranceResponse { Type = "Car", MonthlyCost = 30m };
        var petInsuranceResponse = new PetInsuranceResponse { Type = "Pet", MonthlyCost = 10m };
        var healthInsuranceResponse = new PersonalHealthInsuranceResponse { Type = "Health", MonthlyCost = 20m };

        _mockInsuranceRepository
            .Setup(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<CarInsuranceResponse>(It.IsAny<CarInsurance>()))
            .Returns(carInsuranceResponse);

        _mockMapper
            .Setup(x => x.Map<PetInsuranceResponse>(It.IsAny<PetInsurance>()))
            .Returns(petInsuranceResponse);

        _mockMapper
            .Setup(x => x.Map<PersonalHealthInsuranceResponse>(It.IsAny<PersonalHealthInsurance>()))
            .Returns(healthInsuranceResponse);

        _mockVehicleService
            .Setup(x => x.GetVehicleInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VehicleResponse { RegistrationNumber = "ABC123" });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Insurances.Should().HaveCount(3);
        result.TotalMonthlyCost.Should().Be(60m);
        result.Insurances.Should().Contain(x => x is CarInsuranceResponse);
        result.Insurances.Should().Contain(x => x is PetInsuranceResponse);
        result.Insurances.Should().Contain(x => x is PersonalHealthInsuranceResponse);
    }

    [Fact]
    public async Task Handle_WhenVehicleServiceThrowsException_ShouldStillReturnCarInsurance()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789");
        var pin = new PersonalIdentificationNumber("123456789");
        var carInsurance = new CarInsurance(pin, "ABC123");
        var insurances = new List<Insurance.Domain.Entities.Insurance> { carInsurance };

        var carInsuranceResponse = new CarInsuranceResponse
        {
            Type = "Car",
            MonthlyCost = 30m
        };

        _mockInsuranceRepository
            .Setup(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<CarInsuranceResponse>(It.IsAny<CarInsurance>()))
            .Returns(carInsuranceResponse);

        _mockVehicleService
            .Setup(x => x.GetVehicleInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Vehicle service unavailable"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Insurances.Should().HaveCount(1);
        var carResult = (CarInsuranceResponse)result.Insurances.First();
        carResult.Vehicle.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithInvalidPersonalIdentificationNumber_ShouldReturnNull()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("invalid");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _mockInsuranceRepository.Verify(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnNull()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789");

        _mockInsuranceRepository
            .Setup(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()))
            .ThrowsAsync(new Exception("Repository error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUnknownInsuranceType_ShouldSkipUnknownInsurance()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789");
        var pin = new PersonalIdentificationNumber("123456789");
        
        // Create a mock insurance that doesn't match any known types
        var mockInsurance = new Mock<Insurance.Domain.Entities.Insurance>(pin, 25m, Insurance.Domain.Enums.InsuranceType.Car);
        var petInsurance = new PetInsurance(pin, "Fluffy", "Dog");
        var insurances = new List<Insurance.Domain.Entities.Insurance> { mockInsurance.Object, petInsurance };

        var petInsuranceResponse = new PetInsuranceResponse { Type = "Pet", MonthlyCost = 10m };

        _mockInsuranceRepository
            .Setup(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<PetInsuranceResponse>(It.IsAny<PetInsurance>()))
            .Returns(petInsuranceResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Insurances.Should().HaveCount(1);
        result.Insurances.First().Should().BeOfType<PetInsuranceResponse>();
        result.TotalMonthlyCost.Should().Be(10m);
    }

    [Fact]
    public void Constructor_WithNullInsuranceRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new GetPersonInsurancesQueryHandler(null!, _mockVehicleService.Object, _mockMapper.Object));

        exception.ParamName.Should().Be("insuranceRepository");
    }

    [Fact]
    public void Constructor_WithNullVehicleService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new GetPersonInsurancesQueryHandler(_mockInsuranceRepository.Object, null!, _mockMapper.Object));

        exception.ParamName.Should().Be("vehicleService");
    }

    [Fact]
    public void Constructor_WithNullMapper_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new GetPersonInsurancesQueryHandler(_mockInsuranceRepository.Object, _mockVehicleService.Object, null!));

        exception.ParamName.Should().Be("mapper");
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToServices()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789");
        var pin = new PersonalIdentificationNumber("123456789");
        var carInsurance = new CarInsurance(pin, "ABC123");
        var insurances = new List<Insurance.Domain.Entities.Insurance> { carInsurance };
        var cancellationToken = new CancellationToken();

        var carInsuranceResponse = new CarInsuranceResponse
        {
            Type = "Car",
            MonthlyCost = 30m
        };

        _mockInsuranceRepository
            .Setup(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<CarInsuranceResponse>(It.IsAny<CarInsurance>()))
            .Returns(carInsuranceResponse);

        _mockVehicleService
            .Setup(x => x.GetVehicleInfoAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(new VehicleResponse { RegistrationNumber = "ABC123" });

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        _mockVehicleService.Verify(x => x.GetVehicleInfoAsync("ABC123", cancellationToken), Times.Once);
    }
}
