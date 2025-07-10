using Microsoft.EntityFrameworkCore;
using Vehicle.Core.Repositories;
using Vehicle.Infrastructure.Persistence;

namespace Vehicle.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly VehicleDbContext _context;

    public VehicleRepository(VehicleDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Core.Entities.Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Core.Entities.Vehicle?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles.FirstOrDefaultAsync(v => v.RegistrationNumber == registrationNumber, cancellationToken);
    }
}
