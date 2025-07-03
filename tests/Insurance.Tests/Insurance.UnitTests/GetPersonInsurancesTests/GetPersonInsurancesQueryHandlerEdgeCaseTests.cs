using AutoMapper;
using FluentAssertions;
using Insurance.Application.Interfaces;
using Insurance.Application.Queries.GetPersonInsurances;
using Insurance.Contracts;
using Insurance.Domain.Repositories;
using Insurance.Domain.ValueObjects;
using Moq;
using Vehicle.Contracts;

namespace Insurance.UnitTests.GetPersonInsurancesTests;

public class GetPersonInsurancesQueryHandlerEdgeCaseTests
{
    private readonly Mock<IInsuranceRepository> _mockInsuranceRepository;
    private readonly Mock<IVehicleService> _mockVehicleService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetPersonInsurancesQueryHandler _handler;

    public GetPersonInsurancesQueryHandlerEdgeCaseTests()
    {
        _mockInsuranceRepository = new Mock<IInsuranceRepository>();
        _mockVehicleService = new Mock<IVehicleService>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetPersonInsurancesQueryHandler(
            _mockInsuranceRepository.Object,
            _mockVehicleService.Object,
            _mockMapper.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("12345678901")]
    public async Task Handle_WithInvalidPersonalIdentificationNumberFormats_ShouldReturnNull(string invalidPin)
    {
        // Arrange
        var query = new GetPersonInsurancesQuery(invalidPin);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _mockInsuranceRepository.Verify(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMapperReturnsNull_ShouldSkipInsurance()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789");
        var carInsurance = TestDataBuilder.CreateCarInsurance();
        var petInsurance = TestDataBuilder.CreatePetInsurance();
        var insurances = new List<Insurance.Domain.Entities.Insurance> { carInsurance, petInsurance };

        var petInsuranceResponse = TestDataBuilder.CreatePetInsuranceResponse();

        _mockInsuranceRepository
            .Setup(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<CarInsuranceResponse>(It.IsAny<Insurance.Domain.Entities.CarInsurance>()))
            .Returns((CarInsuranceResponse)null!);

        _mockMapper
            .Setup(x => x.Map<PetInsuranceResponse>(It.IsAny<Insurance.Domain.Entities.PetInsurance>()))
            .Returns(petInsuranceResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Insurances.Should().HaveCount(1);
        result.Insurances.Should().AllBeOfType<PetInsuranceResponse>();
        result.TotalMonthlyCost.Should().Be(10m);
    }

    [Fact]
    public async Task Handle_WhenVehicleServiceReturnsNull_ShouldStillIncludeCarInsurance()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789");
        var carInsurance = TestDataBuilder.CreateCarInsurance();
        var insurances = new List<Insurance.Domain.Entities.Insurance> { carInsurance };

        var carInsuranceResponse = TestDataBuilder.CreateCarInsuranceResponse();

        _mockInsuranceRepository
            .Setup(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<CarInsuranceResponse>(It.IsAny<Insurance.Domain.Entities.CarInsurance>()))
            .Returns(carInsuranceResponse);

        _mockVehicleService
            .Setup(x => x.GetVehicleInfoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleResponse?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Insurances.Should().HaveCount(1);
        var carResult = (CarInsuranceResponse)result.Insurances.First();
        carResult.Vehicle.Should().BeNull();
        result.TotalMonthlyCost.Should().Be(30m);
    }

    [Fact]
    public async Task Handle_WithLargeNumberOfInsurances_ShouldHandleAllInsurances()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789");
        var pin = TestDataBuilder.CreateValidPin();
        var insurances = new List<Insurance.Domain.Entities.Insurance>();
        
        // Create 100 pet insurances
        for (int i = 0; i < 100; i++)
        {
            insurances.Add(new Insurance.Domain.Entities.PetInsurance(pin, $"Pet{i}", "Cat"));
        }

        _mockInsuranceRepository
            .Setup(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<PetInsuranceResponse>(It.IsAny<Insurance.Domain.Entities.PetInsurance>()))
            .Returns((Insurance.Domain.Entities.PetInsurance pet) => new PetInsuranceResponse
            {
                Type = "Pet",
                MonthlyCost = 10m,
                PetName = pet.PetName,
                PetType = pet.PetType
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Insurances.Should().HaveCount(100);
        result.TotalMonthlyCost.Should().Be(1000m);
    }

    [Fact]
    public async Task Handle_WithMixedSuccessAndFailureMapping_ShouldIncludeOnlySuccessfulMappings()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789");
        var carInsurance = TestDataBuilder.CreateCarInsurance();
        var petInsurance = TestDataBuilder.CreatePetInsurance();
        var healthInsurance = TestDataBuilder.CreateHealthInsurance();
        var insurances = new List<Insurance.Domain.Entities.Insurance> { carInsurance, petInsurance, healthInsurance };

        var petInsuranceResponse = TestDataBuilder.CreatePetInsuranceResponse();
        var healthInsuranceResponse = TestDataBuilder.CreateHealthInsuranceResponse();

        _mockInsuranceRepository
            .Setup(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()))
            .ReturnsAsync(insurances);

        // Car insurance mapping fails
        _mockMapper
            .Setup(x => x.Map<CarInsuranceResponse>(It.IsAny<Insurance.Domain.Entities.CarInsurance>()))
            .Returns((CarInsuranceResponse)null!);

        // Pet insurance mapping succeeds
        _mockMapper
            .Setup(x => x.Map<PetInsuranceResponse>(It.IsAny<Insurance.Domain.Entities.PetInsurance>()))
            .Returns(petInsuranceResponse);

        // Health insurance mapping succeeds
        _mockMapper
            .Setup(x => x.Map<PersonalHealthInsuranceResponse>(It.IsAny<Insurance.Domain.Entities.PersonalHealthInsurance>()))
            .Returns(healthInsuranceResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Insurances.Should().HaveCount(2);
        result.Insurances.Should().Contain(x => x is PetInsuranceResponse);
        result.Insurances.Should().Contain(x => x is PersonalHealthInsuranceResponse);
        result.TotalMonthlyCost.Should().Be(30m);
    }

    [Fact]
    public async Task Handle_WhenCancellationTokenIsCancelled_ShouldRespectCancellation()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789");
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _mockInsuranceRepository
            .Setup(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(query, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task MapInsuranceToContract_WithCarInsurance_ShouldCallVehicleServiceWithCorrectParameters()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789");
        var carInsurance = TestDataBuilder.CreateCarInsurance("123456789", "XYZ789");
        var insurances = new List<Insurance.Domain.Entities.Insurance> { carInsurance };

        var carInsuranceResponse = TestDataBuilder.CreateCarInsuranceResponse();
        var vehicleResponse = TestDataBuilder.CreateVehicleResponse("XYZ789", "Honda", "Civic");

        _mockInsuranceRepository
            .Setup(x => x.GetByOwnerAsync(It.IsAny<PersonalIdentificationNumber>()))
            .ReturnsAsync(insurances);

        _mockMapper
            .Setup(x => x.Map<CarInsuranceResponse>(It.IsAny<Insurance.Domain.Entities.CarInsurance>()))
            .Returns(carInsuranceResponse);

        _mockVehicleService
            .Setup(x => x.GetVehicleInfoAsync("XYZ789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockVehicleService.Verify(x => x.GetVehicleInfoAsync("XYZ789", It.IsAny<CancellationToken>()), Times.Once);
        
        var carResult = (CarInsuranceResponse)result!.Insurances.First();
        carResult.Vehicle.Should().Be(vehicleResponse);
    }
}
