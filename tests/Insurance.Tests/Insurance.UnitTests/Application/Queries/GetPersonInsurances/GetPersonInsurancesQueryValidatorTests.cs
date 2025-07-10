using FluentValidation.TestHelper;
using Insurance.Core.Queries.GetPersonInsurances;

namespace Insurance.UnitTests.Application.Queries.GetPersonInsurances;

public class GetPersonInsurancesQueryValidatorTests
{
    private readonly GetPersonInsurancesQueryValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_PersonalIdentificationNumber_Is_Empty()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery(string.Empty);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PersonalIdentificationNumber)
            .WithErrorMessage("Personal identification number is required.");
    }

    [Fact]
    public void Should_Have_Error_When_PersonalIdentificationNumber_Is_Invalid()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("123456789"); // Invalid Swedish personal number

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PersonalIdentificationNumber)
            .WithErrorMessage("Personal identification number is invalid.");
    }

    [Fact]
    public void Should_Have_Error_When_PersonalIdentificationNumber_Formatted_Is_Invalid()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("840833-4238"); // Invalid Swedish personal number

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PersonalIdentificationNumber)
            .WithErrorMessage("Personal identification number is invalid.");
    }


    [Fact]
    public void Should_Not_Have_Error_When_PersonalIdentificationNumber_Is_Valid()
    {
        // Arrange
        var query = new GetPersonInsurancesQuery("840831-4238"); // Valid Swedish personal number format

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PersonalIdentificationNumber);
    }

    [Fact]
    public void Should_Have_Error_When_PersonalIdentificationNumber_Exceeds_Maximum_Length()
    {
        // Arrange
        var longPersonalNumber = new string('1', 14); // 14 characters, exceeds max of 13
        var query = new GetPersonInsurancesQuery(longPersonalNumber);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PersonalIdentificationNumber)
            .WithErrorMessage("Personal identification number must not exceed 13 characters.");
    }

    [Theory]
    [InlineData("19840831-4238")]
    [InlineData("198408314238")]
    [InlineData("840831-4238")]
    public void Should_Not_Have_Error_When_PersonalIdentificationNumber_Is_Valid_Format(string personalNumber)
    {
        // Arrange
        var query = new GetPersonInsurancesQuery(personalNumber);

        // Act
        var result = _validator.TestValidate(query);

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
        var query = new GetPersonInsurancesQuery(personalNumber!);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PersonalIdentificationNumber)
            .WithErrorMessage("Personal identification number is required.");
    }
}
