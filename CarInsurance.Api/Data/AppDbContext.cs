using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<InsurancePolicy> Policies => Set<InsurancePolicy>();
    public DbSet<InsuranceClaim> Claims => Set<InsuranceClaim>();
    public DbSet<ProcessedPolicyExpiration> ProcessedPolicyExpirations => Set<ProcessedPolicyExpiration>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>()
            .HasIndex(c => c.Vin)
            .IsUnique();

        modelBuilder.Entity<InsurancePolicy>()
            .Property(p => p.StartDate)
            .IsRequired();

        modelBuilder.Entity<InsurancePolicy>()
            .Property(p => p.EndDate)
            .IsRequired();

        modelBuilder.Entity<InsuranceClaim>()
            .Property(c => c.ClaimDate)
            .IsRequired();
    }
}

public static class SeedData
{
    public static void EnsureSeeded(AppDbContext db)
    {
        if (db.Owners.Any()) return;

        var ana = new Owner { Name = "Ana Pop", Email = "ana.pop@example.com" };
        var bogdan = new Owner { Name = "Bogdan Ionescu", Email = "bogdan.ionescu@example.com" };
        db.Owners.AddRange(ana, bogdan);
        db.SaveChanges();

        var car1 = new Car { Vin = "VIN12345", Make = "Dacia", Model = "Logan", YearOfManufacture = 2018, OwnerId = ana.Id };
        var car2 = new Car { Vin = "VIN67890", Make = "VW", Model = "Golf", YearOfManufacture = 2021, OwnerId = bogdan.Id };
        db.Cars.AddRange(car1, car2);
        db.SaveChanges();

        db.Policies.AddRange(
            new InsurancePolicy { CarId = car1.Id, Provider = "Allianz", StartDate = new DateTime(2024,1,1), EndDate = new DateTime(2024,12,31) },
            new InsurancePolicy { CarId = car1.Id, Provider = "Groupama", StartDate = new DateTime(2025,1,1), EndDate = new DateTime(2025, 6, 25) },
            new InsurancePolicy { CarId = car2.Id, Provider = "Allianz", StartDate = new DateTime(2025,3,1), EndDate = new DateTime(2025,9,30) }
        );
        db.SaveChanges();

        db.Claims.AddRange(
            new InsuranceClaim { CarId = car1.Id, ClaimDate = new DateTime(2024,5,20), Description = "Minor scratch on the rear bumper", Amount = 300 },
            new InsuranceClaim { CarId = car2.Id, ClaimDate = new DateTime(2025,4,15), Description = "Broken windshield", Amount = 800 }
        );
        db.SaveChanges();
    }
}
