using System;
using Vehicle.Application.DTOs;

namespace Vehicle.Application.Interfaces;

public interface IVehicleService
{
    Task<VehicleDto?> GetVehicleByRegistrationNumberAsync(string registrationNumber);
}
