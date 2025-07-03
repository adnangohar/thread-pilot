using AutoMapper;
using MediatR;
using Vehicle.Application.Common;
using Vehicle.Domain.Repositories;
using Vehicle.Domain.ValueObjects;

namespace Vehicle.Application.Queries.GetVehicle;

public class GetVehicleByRegistrationNumberQueryHandler : IRequestHandler<GetVehicleByRegistrationNumberQuery, VehicleResult?>
{
   private readonly IVehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;

    public GetVehicleByRegistrationNumberQueryHandler(IVehicleRepository vehicleRepository, IMapper mapper)
    {
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<VehicleResult?> Handle(GetVehicleByRegistrationNumberQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var regNumber = new RegistrationNumber(request.RegistrationNumber);
            var vehicle = await _vehicleRepository.GetByRegistrationNumberAsync(regNumber);

            return vehicle == null ? null : _mapper.Map<VehicleResult>(vehicle);
        }
        catch (ArgumentException)
        {
            // Invalid registration number format
            return null;
        }
    }
}
