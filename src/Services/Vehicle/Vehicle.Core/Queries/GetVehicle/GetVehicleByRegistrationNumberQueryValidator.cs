using FluentValidation;

namespace Vehicle.Core.Queries.GetVehicle;

public class GetVehicleByRegistrationNumberQueryValidator : AbstractValidator<GetVehicleByRegistrationNumberQuery>
{
    public GetVehicleByRegistrationNumberQueryValidator()
    {
        RuleFor(x => x.RegistrationNumber)
            .NotEmpty().WithMessage("Registration number is required.")
            .MaximumLength(6).WithMessage("Registration number must not exceed 6 characters.");
    }
}
