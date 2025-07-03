using AutoMapper;
using Bogus;
using FluentAssertions;
using Insurance.Application.Interfaces;
using Insurance.Application.Queries.GetPersonInsurances;
using Insurance.Contracts;
using Insurance.Domain.Entities;
using Insurance.Domain.Repositories;
using Insurance.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Vehicle.Contracts;

namespace Insurance.UnitTests.Application.Queries.GetPersonInsurances;

public class GetPersonInsurancesQueryHandlerTests
{
    private readonly Mock<IInsuranceRepository> _mockInsuranceRepository;
    private readonly Mock<IVehicleService> _mockVehicleService;
    private readonly Mock<IMapper> _mockMapper;

    private readonly Mock<ILogger<GetPersonInsurancesQueryHandler>> _mockLogger;
    private readonly GetPersonInsurancesQueryHandler _handler;

    public GetPersonInsurancesQueryHandlerTests()
    {
        _mockInsuranceRepository = new Mock<IInsuranceRepository>();
        _mockVehicleService = new Mock<IVehicleService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<GetPersonInsurancesQueryHandler>>();
        _handler = new GetPersonInsurancesQueryHandler(
            _mockInsuranceRepository.Object,
            _mockVehicleService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenPersonHasNoInsurances_ShouldReturnNull()
    {
        // Arrange
        var pin = TestDataBuilder.GenerateSwedishPin();
        var query = new GetPersonInsurancesQuery(pin);
        var personalId = new PersonalIdentificationNumber(pin);
        var emptyInsurances = new List<Domain.Entities.Insurance>();

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
        var pin = TestDataBuilder.GenerateSwedishPin();
        var regNumber = TestDataBuilder.GenerateCarRegNumber();
        
        var query = new GetPersonInsurancesQuery(pin);
        var personalId = new PersonalIdentificationNumber(pin);
        var carInsurance = new CarInsurance(personalId, regNumber);
        var insurances = new List<Domain.Entities.Insurance> { carInsurance };

        var carInsuranceResponse = new CarInsuranceResponse
        {
            Type = "Car",
            MonthlyCost = 30m,
            Vehicle = new VehicleResponse
            {
                RegistrationNumber = regNumber,
                Make = "Toyota",
                Model = "Camry"
            }
        };

        var vehicleResponse = new VehicleResponse
        {
            RegistrationNumber = regNumber,
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
        result!.PersonalIdentificationNumber.Should().Be(pin);
        result.Insurances.Should().HaveCount(1);
        result.TotalMonthlyCost.Should().Be(30m);
        result.Insurances.First().Should().BeOfType<CarInsuranceResponse>();

        var carResult = (CarInsuranceResponse)result.Insurances.First();
        carResult.Vehicle.Should().NotBeNull();
        carResult.Vehicle!.RegistrationNumber.Should().Be(regNumber);
    }

    [Fact]
    public async Task Handle_WithMultipleInsurances_ShouldReturnAllInsurancesAndCalculateTotalCost()
    {
        // Arrange
        var pinValue = TestDataBuilder.GenerateSwedishPin();
        var regNumber = TestDataBuilder.GenerateCarRegNumber();
        var query = new GetPersonInsurancesQuery(pinValue);
        var pin = new PersonalIdentificationNumber(pinValue);
        var carInsurance = new CarInsurance(pin, regNumber);
        var petInsurance = new PetInsurance(pin, "Fluffy", "Dog");
        var healthInsurance = new PersonalHealthInsurance(pin, "Premium");
        var insurances = new List<Domain.Entities.Insurance> { carInsurance, petInsurance, healthInsurance };

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
            .ReturnsAsync(new VehicleResponse { RegistrationNumber = regNumber });

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
        var pinValue = TestDataBuilder.GenerateSwedishPin();
        var regNumber = TestDataBuilder.GenerateCarRegNumber();
        var query = new GetPersonInsurancesQuery(pinValue);
        var pin = new PersonalIdentificationNumber(pinValue);
        var carInsurance = new CarInsurance(pin, regNumber);
        var insurances = new List<Domain.Entities.Insurance> { carInsurance };

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
            .ReturnsAsync(null as VehicleResponse); // Simulate failure to get vehicle info

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Insurances.Should().HaveCount(1);
        var carResult = (CarInsuranceResponse)result.Insurances.First();
        carResult.Vehicle.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUnknownInsuranceType_ShouldSkipUnknownInsurance()
    {
        // Arrange
        var pinValue = TestDataBuilder.GenerateSwedishPin();
        var query = new GetPersonInsurancesQuery(pinValue);
        var pin = new PersonalIdentificationNumber(pinValue);
        var mockInsurance = new Mock<Domain.Entities.Insurance>(pin, 25m, Insurance.Domain.Enums.InsuranceType.Car);
        var petInsurance = new PetInsurance(pin, "Fluffy", "Dog");
        var insurances = new List<Domain.Entities.Insurance> { mockInsurance.Object, petInsurance };

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
}
