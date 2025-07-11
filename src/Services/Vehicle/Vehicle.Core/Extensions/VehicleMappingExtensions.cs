using Vehicle.Core.Common;

namespace Vehicle.Core.Extensions;

public static class VehicleMappingExtensions
{
    public static VehicleResult ToResult(this Entities.Vehicle vehicle)
    {
        return new VehicleResult(
            RegistrationNumber: vehicle.RegistrationNumber,
            Make: vehicle.Make,
            Model: vehicle.Model,
            Year: vehicle.Year,
            Color: vehicle.Color
        );
    }

    public static IEnumerable<VehicleResult> ToResult(this IEnumerable<Entities.Vehicle> vehicles)
    {
        return vehicles.Select(vehicle => vehicle.ToResult());
    }
}
