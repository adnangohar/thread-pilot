using MediatR;
using Vehicle.Application.Common;
using Vehicle.Application.Interfaces;

namespace Vehicle.Application.Queries.GetVehicle;

public class GetVehicleByRegistrationNumberQueryHandler : IRequestHandler<GetVehicleByRegistrationNumberQuery, VehicleResult?>
{
    private readonly IVehicleService _vehicleService;

    public GetVehicleByRegistrationNumberQueryHandler(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    public async Task<VehicleResult?> Handle(GetVehicleByRegistrationNumberQuery request, CancellationToken cancellationToken)
    {
        return await _vehicleService.GetVehicleByRegistrationNumberAsync(request.RegistrationNumber);
    }
}
