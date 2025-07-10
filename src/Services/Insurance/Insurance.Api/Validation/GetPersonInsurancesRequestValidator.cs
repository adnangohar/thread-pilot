using ActiveLogin.Identity.Swedish;
using FluentValidation;
using Insurance.Api.Contracts;

namespace Insurance.Api.Validation;

public class GetPersonInsurancesRequestValidator : AbstractValidator<GetPersonInsurancesRequest>
{
    public GetPersonInsurancesRequestValidator()
    {
        RuleFor(x => x.PersonalIdentificationNumber)
            .NotEmpty()
            .WithMessage("Personal identification number is required.")
            .Must(IsValidSwedishPersonalNumber)
            .WithMessage("Invalid Swedish personal identification number format. Expected format: YYYYMMDD-NNNN or YYMMDD-NNNN");
    }

   private static bool IsValidSwedishPersonalNumber(string personalNumber)
    {
        return PersonalIdentityNumber.TryParse(personalNumber, StrictMode.Off, out var personnummer);
    }
}
