using System;
using Insurance.Contracts;

namespace Insurance.Application.Interfaces;

public interface IVehicleService
{
     Task<VehicleInfoDto?> GetVehicleInfoAsync(string registrationNumber);
}
