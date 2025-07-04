using System;
using Microsoft.EntityFrameworkCore;

namespace Vehicle.Infrastructure.Persistence;

public class VehicleDbContext : DbContext
{
    public VehicleDbContext(DbContextOptions<VehicleDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Vehicle> Vehicles { get; set; }

}
