using Microsoft.EntityFrameworkCore;

namespace Vehicle.Infrastructure.Persistence;

public class VehicleDbContext : DbContext
{
    public VehicleDbContext(DbContextOptions<VehicleDbContext> options) : base(options)
    {
    }

    public DbSet<Core.Entities.Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(VehicleDbContext).Assembly);
            
            // Seed data for testing
            modelBuilder.Entity<Core.Entities.Vehicle>().HasData(
                new 
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    RegistrationNumber = "ABC123",
                    Make = "Volvo",
                    Model = "XC90",
                    Year = 2022,
                    Color = "Black",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null
                },
                new 
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    RegistrationNumber = "DEF456",
                    Make = "BMW",
                    Model = "X5",
                    Year = 2021,
                    Color = "White",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null
                },
                new 
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    RegistrationNumber = "GHI789",
                    Make = "Audi",
                    Model = "Q7",
                    Year = 2023,
                    Color = "Silver",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null
                }
            );
        }

}
