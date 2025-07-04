namespace Vehicle.Domain.Repositories;

public interface IVehicleRepository
{
    Task<Entities.Vehicle?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Entities.Vehicle vehicle, CancellationToken cancellationToken = default);
}
