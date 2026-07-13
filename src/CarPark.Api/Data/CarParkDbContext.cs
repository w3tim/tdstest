using CarPark.Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CarPark.Api.Data;

public class CarParkDbContext(DbContextOptions<CarParkDbContext> options) : DbContext(options)
{
    public DbSet<ParkingSpace> ParkingSpaces => Set<ParkingSpace>();

    public DbSet<ParkingSession> ParkingSessions => Set<ParkingSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParkingSpace>()
            .HasKey(space => space.SpaceNumber);

        modelBuilder.Entity<ParkingSession>()
            .HasKey(session => session.Id);

        modelBuilder.Entity<ParkingSession>()
            .Property(session => session.VehicleReg)
            .HasMaxLength(20)
            .IsRequired();

        modelBuilder.Entity<ParkingSession>()
            .Property(session => session.VehicleCharge)
            .HasPrecision(10, 2);

        modelBuilder.Entity<ParkingSpace>()
            .HasMany(e => e.ParkingSessions)
            .WithOne()
            .HasForeignKey(e => e.SpaceNumber)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<ParkingSpace>().HasData(
            Enumerable.Range(1, 10)
                .Select(spaceNumber => new ParkingSpace
                {
                    SpaceNumber = spaceNumber
                }));

        base.OnModelCreating(modelBuilder);
    }
}