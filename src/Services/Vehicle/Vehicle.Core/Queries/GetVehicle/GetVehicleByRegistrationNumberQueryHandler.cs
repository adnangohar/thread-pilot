using FluentValidation;
using Microsoft.Extensions.Logging;
using Vehicle.Core.Common;
using Vehicle.Core.Extensions;
using Vehicle.Core.Repositories;
using Vehicle.Core.ValueObjects;

namespace Vehicle.Core.Queries.GetVehicle;

public class GetVehicleByRegistrationNumberQueryHandler : IGetVehicleByRegistrationNumberQueryHandler
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IValidator<GetVehicleByRegistrationNumberQuery> _validator;
    private readonly ILogger<GetVehicleByRegistrationNumberQueryHandler> _logger;

    public GetVehicleByRegistrationNumberQueryHandler(IVehicleRepository vehicleRepository, IValidator<GetVehicleByRegistrationNumberQuery> validator, ILogger<GetVehicleByRegistrationNumberQueryHandler> logger)
    {
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<VehicleResult?> Handle(GetVehicleByRegistrationNumberQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate the request
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for GetVehicleByRegistrationNumberQuery: {Errors}", validationResult.Errors);
                return null;
            }

            var regNumber = new RegistrationNumber(request.RegistrationNumber);
            var vehicle = await _vehicleRepository.GetByRegistrationNumberAsync(regNumber.Value, cancellationToken);

            return vehicle?.ToResult();
        }
        catch (Exception)
        {
            _logger.LogError("An error occurred while handling GetVehicleByRegistrationNumberQuery for registration number {RegistrationNumber}", request.RegistrationNumber);
            return null;
        }
    }
}
