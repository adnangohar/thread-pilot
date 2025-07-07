using FluentAssertions;
using Vehicle.Application.Queries.GetVehicle;

namespace Vehicle.UnitTests.Application.Queries.GetVehicle;

public class GetVehicleByRegistrationNumberQueryValidatorTests
{
    private readonly GetVehicleByRegistrationNumberQueryValidator _validator;

    public GetVehicleByRegistrationNumberQueryValidatorTests()
    {
        _validator = new GetVehicleByRegistrationNumberQueryValidator();
    }

    [Fact]
    public void Validate_WithValidRegistrationNumber_ShouldPass()
    {
        // Arrange
        var query = new GetVehicleByRegistrationNumberQuery("ABC123");

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithEmptyOrWhitespaceRegistrationNumber_ShouldFail(string registrationNumber)
    {
        // Arrange
        var query = new GetVehicleByRegistrationNumberQuery(registrationNumber);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorMessage.Should().Be("Registration number is required.");
        result.Errors[0].PropertyName.Should().Be("RegistrationNumber");
    }

    [Fact]
    public void Validate_WithRegistrationNumberTooLong_ShouldFail()
    {
        // Arrange
        var longRegistrationNumber = "ABCDEFG"; // 7 characters, exceeds limit of 6
        var query = new GetVehicleByRegistrationNumberQuery(longRegistrationNumber);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorMessage.Should().Be("Registration number must not exceed 6 characters.");
        result.Errors[0].PropertyName.Should().Be("RegistrationNumber");
    }

    [Fact]
    public void Validate_WithNullRegistrationNumber_ShouldFail()
    {
        // Arrange
        var query = new GetVehicleByRegistrationNumberQuery(null!);

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorMessage.Should().Be("Registration number is required.");
        result.Errors[0].PropertyName.Should().Be("RegistrationNumber");
    }
}
