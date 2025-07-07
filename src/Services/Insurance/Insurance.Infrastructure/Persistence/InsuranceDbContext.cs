using Insurance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Persistence
{
    public class InsuranceDbContext : DbContext
    {
        public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : base(options)
        {
        }

        public DbSet<Domain.Entities.Insurance> Insurances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InsuranceDbContext).Assembly);
            
            // Seed data for testing
            modelBuilder.Entity<Domain.Entities.Insurance>().HasData(
                new 
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    PersonalId = "19620421-3323",
                    Type = InsuranceType.PersonalHealth,
                    MonthlyCost = 20m,
                    IsActive = true,
                    VehicleRegistrationNumber = (string?)null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null
                },
                new 
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    PersonalId = "19631002-4622",
                    Type = InsuranceType.Car,
                    MonthlyCost = 30m,
                    IsActive = true,
                    VehicleRegistrationNumber = "ABC123",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null
                },
                new
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    PersonalId = "19620421-3323",
                    Type = InsuranceType.Pet,
                    MonthlyCost = 10m,
                    IsActive = true,
                    VehicleRegistrationNumber = (string?)null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null
                },
                new
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    PersonalId = "19631002-4622",
                    Type = InsuranceType.Pet,
                    MonthlyCost = 10m,
                    IsActive = true,
                    VehicleRegistrationNumber = (string?)null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null
                }
            );
        }
    }
}
