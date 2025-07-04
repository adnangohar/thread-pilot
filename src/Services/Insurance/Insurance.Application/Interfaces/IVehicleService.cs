using Insurance.Contracts;

namespace Insurance.Application.Interfaces;

public interface IVehicleService
{
     Task<VehicleResponse?> GetVehicleInfoAsync(string registrationNumber, CancellationToken cancellationToken);
}
