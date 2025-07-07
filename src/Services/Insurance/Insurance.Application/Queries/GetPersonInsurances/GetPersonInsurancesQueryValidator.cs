using FluentValidation;
using ActiveLogin.Identity.Swedish;

namespace Insurance.Application.Queries.GetPersonInsurances;

public class GetPersonInsurancesQueryValidator : AbstractValidator<GetPersonInsurancesQuery>
{
    public GetPersonInsurancesQueryValidator()
    {
        RuleFor(x => x.PersonalIdentificationNumber)
            .NotEmpty().WithMessage("Personal identification number is required.")
            .MaximumLength(13).WithMessage("Personal identification number must not exceed 13 characters.")
            .Must(IsValidSwedishPersonalNumber).WithMessage("Personal identification number is invalid.");

    }

    private static bool IsValidSwedishPersonalNumber(string personalNumber)
    {
        return PersonalIdentityNumber.TryParse(personalNumber, StrictMode.Off, out var personnummer);
    }
}
