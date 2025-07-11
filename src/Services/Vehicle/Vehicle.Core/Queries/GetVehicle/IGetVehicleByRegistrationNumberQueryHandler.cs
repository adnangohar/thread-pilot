using Vehicle.Core.Common;

namespace Vehicle.Core.Queries.GetVehicle;

public interface IGetVehicleByRegistrationNumberQueryHandler
{
    Task<VehicleResult?> Handle(GetVehicleByRegistrationNumberQuery request, CancellationToken cancellationToken);
}
