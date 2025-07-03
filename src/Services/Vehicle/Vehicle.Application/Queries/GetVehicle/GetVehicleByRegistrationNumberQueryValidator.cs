using FluentValidation;

namespace Vehicle.Application.Queries.GetVehicle;

public class GetVehicleByRegistrationNumberQueryValidator : AbstractValidator<GetVehicleByRegistrationNumberQuery>
{
    public GetVehicleByRegistrationNumberQueryValidator()
    {
        RuleFor(x => x.RegistrationNumber)
            .NotEmpty().WithMessage("Registration number is required.")
            .MaximumLength(20).WithMessage("Registration number must not exceed 20 characters.");
    }
}
