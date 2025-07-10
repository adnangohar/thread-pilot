using FluentValidation.TestHelper;
using Insurance.Api.Contracts;
using Insurance.Api.Validation;

namespace Insurance.UnitTests.Endpoints;

public class GetPersonInsurancesRequestValidatorTests
{
    private readonly GetPersonInsurancesRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_PersonalIdentificationNumber_Is_Empty()
    {
        // Arrange
        var request = new GetPersonInsurancesRequest
        {
            PersonalIdentificationNumber = string.Empty
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PersonalIdentificationNumber)
            .WithErrorMessage("Personal identification number is required.");
    }

    [Fact]
    public void Should_Have_Error_When_PersonalIdentificationNumber_Is_Invalid()
    {
        // Arrange
        var request = new GetPersonInsurancesRequest
        {
            PersonalIdentificationNumber = "123456789" // Invalid Swedish personal number
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PersonalIdentificationNumber)
            .WithErrorMessage("Invalid Swedish personal identification number format. Expected format: YYYYMMDD-NNNN or YYMMDD-NNNN");
    }

    [Fact]
    public void Should_Not_Have_Error_When_PersonalIdentificationNumber_Is_Valid()
    {
        // Arrange
        var request = new GetPersonInsurancesRequest
        {
            PersonalIdentificationNumber = "19810624-8696" // Valid Swedish personal number format
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PersonalIdentificationNumber);
    }

    [Theory]
    [InlineData("19870927-4222")]
    [InlineData("198709274222")]
    [InlineData("870927-4222")]
    public void Should_Not_Have_Error_When_PersonalIdentificationNumber_Is_Valid_Format(string personalNumber)
    {
        // Arrange
        var request = new GetPersonInsurancesRequest
        {
            PersonalIdentificationNumber = personalNumber
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PersonalIdentificationNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_PersonalIdentificationNumber_Is_Null_Or_Empty(string? personalNumber)
    {
        // Arrange
        var request = new GetPersonInsurancesRequest
        {
            PersonalIdentificationNumber = personalNumber!
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PersonalIdentificationNumber)
            .WithErrorMessage("Personal identification number is required.");
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("123")]
    [InlineData("19801230")]
    [InlineData("1234567890123456")]
    [InlineData("invalid-format")]
    public void Should_Have_Error_When_PersonalIdentificationNumber_Has_Invalid_Format(string personalNumber)
    {
        // Arrange
        var request = new GetPersonInsurancesRequest
        {
            PersonalIdentificationNumber = personalNumber
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PersonalIdentificationNumber)
            .WithErrorMessage("Invalid Swedish personal identification number format. Expected format: YYYYMMDD-NNNN or YYMMDD-NNNN");
    }
}
