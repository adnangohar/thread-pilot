using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Insurance.Core.Common;
using Insurance.Core.Interfaces;
using Insurance.Core.Queries.GetPersonInsurances;
using Insurance.Core.Repositories;
using Insurance.Core.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using DomainInsuranceType = Insurance.Core.Enums.InsuranceType;

namespace Insurance.UnitTests.Application.Queries.GetPersonInsurances;

public class GetPersonInsurancesQueryHandlerTests
{
    private readonly Mock<IInsuranceRepository> _mockInsuranceRepository;
    private readonly Mock<IVehicleService> _mockVehicleService;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<IValidator<GetPersonInsurancesQuery>> _mockValidator;
    private readonly Mock<ILogger<GetPersonInsurancesQueryHandler>> _mockLogger;
    private readonly GetPersonInsurancesQueryHandler _handler;
    public const string EnableDetailedVehicleInfo = "EnableDetailedVehicleInfo";

    public GetPersonInsurancesQueryHandlerTests()
    {
        _mockInsuranceRepository = new Mock<IInsuranceRepository>();
        _mockVehicleService = new Mock<IVehicleService>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockValidator = new Mock<IValidator<GetPersonInsurancesQuery>>();
        _mockLogger = new Mock<ILogger<GetPersonInsurancesQueryHandler>>();
        _handler = new GetPersonInsurancesQueryHandler(
            _mockInsuranceRepository.Object,
            _mockVehicleService.Object,
            _mockValidator.Object,
            _mockLogger.Object,
            _mockFeatureManager.Object);
    }

    private void SetupValidValidation()
    {
        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<GetPersonInsurancesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task Handle_WhenPersonHasNoInsurances_ShouldReturnEmptyResult()
    {
        // Arrange
        var pin = TestDataBuilder.GenerateSwedishPin();
        var query = new GetPersonInsurancesQuery(pin);
        var personalId = new PersonalIdentificationNumber(pin);
        var emptyInsurances = new List<Core.Entities.Insurance>();

        SetupValidValidation();

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
        var carInsurance = new Core.Entities.Insurance(personalId, 30m, DomainInsuranceType.Car, regNumber);
        
        var insurances = new List<Core.Entities.Insurance> { carInsurance };

        var vehicleResponse = new VehicleResponse
        {
            RegistrationNumber = regNumber,
            Make = "Toyota",
            Model = "Camry"
        };

        SetupValidValidation();

        _mockInsuranceRepository
            .Setup(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

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
        
        var carInsurance = new Core.Entities.Insurance(pin, 30m, DomainInsuranceType.Car, regNumber);
        var petInsurance = new Core.Entities.Insurance(pin, 10m, DomainInsuranceType.Pet);
        var healthInsurance = new Core.Entities.Insurance(pin, 20m, DomainInsuranceType.PersonalHealth);

        var insurances = new List<Core.Entities.Insurance> { carInsurance, petInsurance, healthInsurance };

        SetupValidValidation();

        _mockInsuranceRepository
            .Setup(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

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
        result.Insurances.Should().Contain(x => x.Type == DomainInsuranceType.Car);
        result.Insurances.Should().Contain(x => x.Type == DomainInsuranceType.Pet);
        result.Insurances.Should().Contain(x => x.Type == DomainInsuranceType.PersonalHealth);
    }

    [Fact]
    public async Task Handle_WhenVehicleServiceThrowsException_ShouldStillReturnCarInsurance()
    {
        // Arrange
        var pin = TestDataBuilder.GenerateSwedishPin();
        var regNumber = TestDataBuilder.GenerateCarRegNumber();
        
        var query = new GetPersonInsurancesQuery(pin);
        var personalId = new PersonalIdentificationNumber(pin);
        var carInsurance = new Core.Entities.Insurance(personalId, 30m, DomainInsuranceType.Car, regNumber);
        
        var insurances = new List<Core.Entities.Insurance> { carInsurance };

        SetupValidValidation();

        _mockInsuranceRepository
            .Setup(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

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

        SetupValidValidation();

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
        var carInsurance = new Core.Entities.Insurance(personalId, 30m, DomainInsuranceType.Car, regNumber);
        
        var insurances = new List<Core.Entities.Insurance> { carInsurance };

        SetupValidValidation();

        _mockInsuranceRepository
            .Setup(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

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
        var carInsurance = new Core.Entities.Insurance(personalId, 30m, DomainInsuranceType.Car, regNumber);
        
        var insurances = new List<Core.Entities.Insurance> { carInsurance };

        var vehicleResponse = new VehicleResponse
        {
            RegistrationNumber = regNumber,
            Make = "Toyota",
            Model = "Camry",
            Year = 2023,
            Color = "Blue"
        };

        SetupValidValidation();

        _mockInsuranceRepository
            .Setup(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(insurances);

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

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldReturnNull()
    {
        // Arrange
        var pin = "invalid-pin";
        var query = new GetPersonInsurancesQuery(pin);
        var validationFailures = new List<ValidationFailure> 
        { 
            new ValidationFailure("PersonalIdentificationNumber", "Personal identification number is invalid.") 
        };
        var validationResult = new ValidationResult(validationFailures);

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<GetPersonInsurancesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        
        // Verify that repository was NOT called
        _mockInsuranceRepository.Verify(x => x.GetByPersonalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
