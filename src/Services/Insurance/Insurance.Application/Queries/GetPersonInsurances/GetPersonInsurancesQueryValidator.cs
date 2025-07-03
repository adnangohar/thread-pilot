using FluentValidation;

namespace Insurance.Application.Queries.GetPersonInsurances;

public class GetPersonInsurancesQueryValidator : AbstractValidator<GetPersonInsurancesQuery>
{
    public GetPersonInsurancesQueryValidator()
    {
        RuleFor(x => x.PersonalIdentificationNumber)
            .NotEmpty().WithMessage("Personal identification number is required.")
            .MaximumLength(20).WithMessage("Personal identification number must not exceed 20 characters.");
    }
}
