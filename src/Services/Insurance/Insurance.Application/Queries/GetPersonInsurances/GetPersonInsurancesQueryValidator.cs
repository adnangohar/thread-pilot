using FluentValidation;
using ActiveLogin.Identity.Swedish;

namespace Insurance.Application.Queries.GetPersonInsurances;

public class GetPersonInsurancesQueryValidator : AbstractValidator<GetPersonInsurancesQuery>
{
    public GetPersonInsurancesQueryValidator()
    {
        RuleFor(x => x.PersonalIdentificationNumber)
            .NotEmpty().WithMessage("Personal identification number is required.")
            .Must(IsValidSwedishPersonalNumber).WithMessage("Personal identification number is invalid.")
            .MaximumLength(20).WithMessage("Personal identification number must not exceed 13 characters.");
    }

    private static bool IsValidSwedishPersonalNumber(string personalNumber)
    {
        return PersonalIdentityNumber.TryParse(personalNumber, StrictMode.Off, out var personnummer);
    }
}
