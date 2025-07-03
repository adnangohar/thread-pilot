using System;
using Vehicle.Application.Common;

namespace Vehicle.Application.Interfaces;

public interface IVehicleService
{
    Task<VehicleResult?> GetVehicleByRegistrationNumberAsync(string registrationNumber);
}
