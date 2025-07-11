namespace Insurance.Core.Interfaces;

using Insurance.Core.Common;

public interface IVehicleService
{
     Task<VehicleResponse?> GetVehicleInfoAsync(string registrationNumber, CancellationToken cancellationToken);
}
