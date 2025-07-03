using Vehicle.Domain.Repositories;
using Vehicle.Domain.ValueObjects;

namespace Vehicle.Infrastructure.Repositories;

public class InMemoryVehicleRepository : IVehicleRepository
{
    private readonly Dictionary<string, Domain.Entities.Vehicle> _vehicles;

    public InMemoryVehicleRepository()
    {
        _vehicles = new Dictionary<string, Domain.Entities.Vehicle>
        {
            ["ABC123"] = new Domain.Entities.Vehicle("ABC123", "Toyota", "Camry", 2020, "Blue"),
            ["XYZ789"] = new Domain.Entities.Vehicle("XYZ789", "Honda", "Civic", 2021, "Red"),
            ["DEF456"] = new Domain.Entities.Vehicle("DEF456", "Ford", "Focus", 2019, "Silver"),
            ["GHI789"] = new Domain.Entities.Vehicle("GHI789", "BMW", "X5", 2022, "Black")
        };
    }

    public async Task AddAsync(Domain.Entities.Vehicle vehicle)
    {
        _vehicles[vehicle.RegistrationNumber.Value] = vehicle; 
        await Task.CompletedTask;
    }

    public async Task<Domain.Entities.Vehicle?> GetByRegistrationNumberAsync(RegistrationNumber registrationNumber)
    {
        _vehicles.TryGetValue(registrationNumber.Value, out var vehicle);
        return await Task.FromResult(vehicle);
    }
}