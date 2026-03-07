using System;
using AppointmentScheduler.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentScheduler.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(x => x.Email);
        modelBuilder.Entity<Appointment>().HasIndex(x => x.Title);

        modelBuilder.Entity<Appointment>()
            .HasOne(x => x.User)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Value conversion for NodaTime Instant to datetime2
        var instantConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<NodaTime.Instant, DateTime>(
            v => v.ToDateTimeUtc(),
            v => NodaTime.Instant.FromDateTimeUtc(DateTime.SpecifyKind(v, DateTimeKind.Utc))
        );

        modelBuilder.Entity<Appointment>()
            .Property(x => x.Date)
            .HasConversion(instantConverter)
            .HasColumnType("datetime2");

        modelBuilder.Entity<Appointment>()
            .Property(x => x.ReminderDate)
            .HasConversion(instantConverter)
            .HasColumnType("datetime2");
        
    }
}
