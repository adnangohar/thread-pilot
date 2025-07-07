using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Vehicle.Application.Common;
using Vehicle.Domain.Repositories;
using Vehicle.Domain.ValueObjects;

namespace Vehicle.Application.Queries.GetVehicle;

public class GetVehicleByRegistrationNumberQueryHandler : IRequestHandler<GetVehicleByRegistrationNumberQuery, VehicleResult?>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetVehicleByRegistrationNumberQuery> _validator;
    private readonly ILogger<GetVehicleByRegistrationNumberQueryHandler> _logger;

    public GetVehicleByRegistrationNumberQueryHandler(IVehicleRepository vehicleRepository, IMapper mapper, IValidator<GetVehicleByRegistrationNumberQuery> validator, ILogger<GetVehicleByRegistrationNumberQueryHandler> logger)
    {
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator;
        _logger = logger;
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

            return vehicle == null ? null : _mapper.Map<VehicleResult>(vehicle);
        }
        catch (Exception)
        {
            _logger.LogError("An error occurred while handling GetVehicleByRegistrationNumberQuery for registration number {RegistrationNumber}", request.RegistrationNumber);
            return null;
        }
    }
}
