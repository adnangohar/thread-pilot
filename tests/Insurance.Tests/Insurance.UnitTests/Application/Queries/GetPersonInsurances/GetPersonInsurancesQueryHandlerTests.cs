using AutoMapper;
using FluentAssertions;
using Insurance.Application.Interfaces;
using Insurance.Application.Queries.GetPersonInsurances;
using Insurance.Contracts;
using Insurance.Domain.Repositories;
using Insurance.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using DomainInsuranceType = Insurance.Domain.Enums.InsuranceType;
using ContractsInsuranceType = Insurance.Contracts.InsuranceType;

namespace Insurance.UnitTests.Application.Queries.GetPersonInsurances;

public class GetPersonInsurancesQueryHandlerTests
{
    private readonly Mock<IInsuranceRepository> _mockInsuranceRepository;
    private readonly Mock<IVehicleService> _mockVehicleService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<ILogger<GetPersonInsurancesQueryHandler>> _mockLogger;
    private readonly GetPersonInsurancesQueryHandler _handler;
    public const string EnableDetailedVehicleInfo = "EnableDetailedVehicleInfo";

    public GetPersonInsurancesQueryHandlerTests()
    {
        _mockInsuranceRepository = new Mock<IInsuranceRepository>();
        _mockVehicleService = new Mock<IVehicleService>();
        _mockMapper = new Mock<IMapper>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockLogger = new Mock<ILogger<GetPersonInsurancesQueryHandler>>();
        _handler = new GetPersonInsurancesQueryHandler(
            _mockInsuranceRepository.Object,
            _mockVehicleService.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockFeatureManager.Object);
    }

    [Fact]
    public async Task Handle_WhenPersonHasNoInsurances_ShouldReturnEmptyResult()
    {
        // Arrange
        var pin = TestDataBuilder.GenerateSwedishPin();
        var query = new GetPersonInsurancesQuery(pin);
        var personalId = new PersonalIdentificationNumber(pin);
        var emptyInsurances = new List<Domain.Entities.Insurance>();

        _mockInsuranceRepository
            .Setup(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyInsurances);

        _mockFeatureManager
            .Setup(x => x.IsEnabledAsync(EnableDetailedVehicleInfo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PersonalIdentificationNumber.Should().Be(pin);
        result.Insurances.Should().BeEmpty();
        result.TotalMonthlyCost.Should().Be(0);
        _mockInsuranceRepository.Verify(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidPersonalIdentificationNumber_ShouldReturnPersonInsurancesResult()
    {
        // Arrange
        var pin = TestDataBuilder.GenerateSwedishPin();
        var regNumber = TestDataBuilder.GenerateCarRegNumber();
        
        var query = new GetPersonInsurancesQuery(pin);
        var personalId = new PersonalIdentificationNumber(pin);
        var carInsurance = new Domain.Entities.Insurance(personalId, 30m, DomainInsuranceType.Car, regNumber);
        
        var insurances = new List<Domain.Entities.Insurance> { carInsurance };

        var insuranceResponse = new InsuranceResponse
        {
            MonthlyCost = 30m,
            Type = ContractsInsuranceType.Car
        };

        var vehicleResponse = new VehicleResponse
        {
            RegistrationNumber = regNumber,
            Make = "Toyota",
            Model = "Camry"
        };

        _mockInsuranceRepository
            .Setup(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<InsuranceResponse>(It.IsAny<Domain.Entities.Insurance>()))
            .Returns(insuranceResponse);

        _mockVehicleService
            .Setup(x => x.GetVehicleInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleResponse);

        _mockFeatureManager
            .Setup(x => x.IsEnabledAsync(EnableDetailedVehicleInfo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PersonalIdentificationNumber.Should().Be(pin);
        result.Insurances.Should().HaveCount(1);
        result.TotalMonthlyCost.Should().Be(30m);
        result.Insurances.First().Should().BeOfType<InsuranceResponse>();

        var carResult = result.Insurances.First();
        carResult.VehicleInfo.Should().NotBeNull();
        carResult.VehicleInfo!.RegistrationNumber.Should().Be(regNumber);
    }

    [Fact]
    public async Task Handle_WithMultipleInsurances_ShouldReturnAllInsurancesAndCalculateTotalCost()
    {
        // Arrange
        var pinValue = TestDataBuilder.GenerateSwedishPin();
        var regNumber = TestDataBuilder.GenerateCarRegNumber();
        var query = new GetPersonInsurancesQuery(pinValue);
        var pin = new PersonalIdentificationNumber(pinValue);
        
        var carInsurance = new Domain.Entities.Insurance(pin, 30m, DomainInsuranceType.Car, regNumber);
        var petInsurance = new Domain.Entities.Insurance(pin, 10m, DomainInsuranceType.Pet);
        var healthInsurance = new Domain.Entities.Insurance(pin, 20m, DomainInsuranceType.PersonalHealth);

        var insurances = new List<Domain.Entities.Insurance> { carInsurance, petInsurance, healthInsurance };

        var carInsuranceResponse = new InsuranceResponse { Type = ContractsInsuranceType.Car, MonthlyCost = 30m };
        var petInsuranceResponse = new InsuranceResponse { Type = ContractsInsuranceType.Pet, MonthlyCost = 10m };
        var healthInsuranceResponse = new InsuranceResponse {  Type = ContractsInsuranceType.PersonalHealth, MonthlyCost = 20m };

        _mockInsuranceRepository
            .Setup(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<InsuranceResponse>(carInsurance))
            .Returns(carInsuranceResponse);

        _mockMapper
            .Setup(x => x.Map<InsuranceResponse>(petInsurance))
            .Returns(petInsuranceResponse);

        _mockMapper
            .Setup(x => x.Map<InsuranceResponse>(healthInsurance))
            .Returns(healthInsuranceResponse);

        _mockVehicleService
            .Setup(x => x.GetVehicleInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VehicleResponse { RegistrationNumber = regNumber });

        _mockFeatureManager
            .Setup(x => x.IsEnabledAsync(EnableDetailedVehicleInfo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Insurances.Should().HaveCount(3);
        result.TotalMonthlyCost.Should().Be(60m);
        result.Insurances.Should().Contain(x => x.Type == ContractsInsuranceType.Car);
        result.Insurances.Should().Contain(x => x.Type == ContractsInsuranceType.Pet);
        result.Insurances.Should().Contain(x => x.Type == ContractsInsuranceType.PersonalHealth);
    }

    [Fact]
    public async Task Handle_WhenVehicleServiceThrowsException_ShouldStillReturnCarInsurance()
    {
        // Arrange
        var pinValue = TestDataBuilder.GenerateSwedishPin();
        var regNumber = TestDataBuilder.GenerateCarRegNumber();
        var query = new GetPersonInsurancesQuery(pinValue);
        var pin = new PersonalIdentificationNumber(pinValue);
        
        var carInsurance = new Domain.Entities.Insurance(pin, 30m, DomainInsuranceType.Car, regNumber);
        
        var insurances = new List<Domain.Entities.Insurance> { carInsurance };

        var insuranceResponse = new InsuranceResponse
        {
            Type = ContractsInsuranceType.Car,
            MonthlyCost = 30m
        };

        _mockInsuranceRepository
            .Setup(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<InsuranceResponse>(It.IsAny<Domain.Entities.Insurance>()))
            .Returns(insuranceResponse);

        _mockVehicleService
            .Setup(x => x.GetVehicleInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as VehicleResponse); // Simulate failure to get vehicle info

        _mockFeatureManager
            .Setup(x => x.IsEnabledAsync(EnableDetailedVehicleInfo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Insurances.Should().HaveCount(1);
        var carResult = result.Insurances.First();
        carResult.VehicleInfo.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldReturnNull()
    {
        // Arrange
        var pinValue = TestDataBuilder.GenerateSwedishPin();
        var query = new GetPersonInsurancesQuery(pinValue);

        _mockInsuranceRepository
            .Setup(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        _mockFeatureManager
            .Setup(x => x.IsEnabledAsync(EnableDetailedVehicleInfo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _mockInsuranceRepository.Verify(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFeatureFlagDisabled_ShouldNotFetchVehicleInfo()
    {
        // Arrange
        var pin = TestDataBuilder.GenerateSwedishPin();
        var regNumber = TestDataBuilder.GenerateCarRegNumber();
        var query = new GetPersonInsurancesQuery(pin);
        var personalId = new PersonalIdentificationNumber(pin);
        var carInsurance = new Domain.Entities.Insurance(personalId, 30m, DomainInsuranceType.Car, regNumber);
        
        var insurances = new List<Domain.Entities.Insurance> { carInsurance };

        var insuranceResponse = new InsuranceResponse
        {
            MonthlyCost = 30m,
            Type = ContractsInsuranceType.Car
        };

        _mockInsuranceRepository
            .Setup(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<InsuranceResponse>(It.IsAny<Domain.Entities.Insurance>()))
            .Returns(insuranceResponse);

        _mockFeatureManager
            .Setup(x => x.IsEnabledAsync(EnableDetailedVehicleInfo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PersonalIdentificationNumber.Should().Be(pin);
        result.Insurances.Should().HaveCount(1);
        result.TotalMonthlyCost.Should().Be(30m);
        
        var carResult = result.Insurances.First();
        carResult.VehicleInfo.Should().BeNull();
        
        // Verify that vehicle service was NOT called
        _mockVehicleService.Verify(x => x.GetVehicleInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenFeatureFlagEnabled_ShouldFetchVehicleInfo()
    {
        // Arrange
        var pin = TestDataBuilder.GenerateSwedishPin();
        var regNumber = TestDataBuilder.GenerateCarRegNumber();
        var query = new GetPersonInsurancesQuery(pin);
        var personalId = new PersonalIdentificationNumber(pin);
        var carInsurance = new Domain.Entities.Insurance(personalId, 30m, DomainInsuranceType.Car, regNumber);
        
        var insurances = new List<Domain.Entities.Insurance> { carInsurance };

        var insuranceResponse = new InsuranceResponse
        {
            MonthlyCost = 30m,
            Type = ContractsInsuranceType.Car
        };

        var vehicleResponse = new VehicleResponse
        {
            RegistrationNumber = regNumber,
            Make = "Toyota",
            Model = "Camry",
            Year = 2023,
            Color = "Blue"
        };

        _mockInsuranceRepository
            .Setup(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<InsuranceResponse>(It.IsAny<Domain.Entities.Insurance>()))
            .Returns(insuranceResponse);

        _mockVehicleService
            .Setup(x => x.GetVehicleInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleResponse);

        _mockFeatureManager
            .Setup(x => x.IsEnabledAsync(EnableDetailedVehicleInfo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PersonalIdentificationNumber.Should().Be(pin);
        result.Insurances.Should().HaveCount(1);
        result.TotalMonthlyCost.Should().Be(30m);
        
        var carResult = result.Insurances.First();
        carResult.VehicleInfo.Should().NotBeNull();
        carResult.VehicleInfo!.RegistrationNumber.Should().Be(regNumber);
        carResult.VehicleInfo.Make.Should().Be("Toyota");
        
        // Verify that vehicle service WAS called
        _mockVehicleService.Verify(x => x.GetVehicleInfoAsync(regNumber, It.IsAny<CancellationToken>()), Times.Once);
    }
}
