using System;
using Vehicle.Domain.ValueObjects;

namespace Vehicle.Domain.Repositories;

public interface IVehicleRepository
{
    Task<Entities.Vehicle?> GetByRegistrationNumberAsync(RegistrationNumber registrationNumber);
    Task AddAsync(Entities.Vehicle vehicle);
}
