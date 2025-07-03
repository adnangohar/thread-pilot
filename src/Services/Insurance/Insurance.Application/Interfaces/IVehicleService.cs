using Vehicle.Contracts;

namespace Insurance.Application.Interfaces;

public interface IVehicleService
{
     Task<VehicleResponse?> GetVehicleInfoAsync(string registrationNumber);
}
