using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class DataContext : DbContext
{
    public DbSet<Barber> Barbers { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Salon> Salons { get; set; }
    public DbSet<Timeslot> Timeslots { get; set; }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) { }
}
