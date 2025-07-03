using FluentValidation;
using Vehicle.Contracts;

namespace Vehicle.Api.Validators;

public class GetVehicleValidator : Validator<GetVehicleRequest>
{
    public GetVehicleValidator()
    {
        RuleFor(x => x.RegistrationNumber)
            .NotEmpty().WithMessage("Registration number is required")
            .Matches(@"^[A-Z]{3}\d{3}$").WithMessage("Registration number must be in format ABC123");
    }
}