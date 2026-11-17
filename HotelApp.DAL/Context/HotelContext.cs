using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelApp.DAL.Base;
using HotelApp.DAL.Exceptions;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Context
{
    public class HotelContext : DbContext
    {
        public HotelContext(DbContextOptions<HotelContext> options) : base(options) { }

        // ---- Last line of defense: nothing invalid reaches SQL Server and no raw SQL error leaves the DAL ----

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ValidateTrackedEntities();
            try
            {
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }
            catch (Exception ex) when (DataErrorTranslator.IsDataException(ex))
            {
                throw DataErrorTranslator.Translate(ex, "HotelContext.SaveChanges");
            }
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ValidateTrackedEntities();
            try
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            catch (Exception ex) when (DataErrorTranslator.IsDataException(ex))
            {
                throw DataErrorTranslator.Translate(ex, "HotelContext.SaveChangesAsync");
            }
        }

        /// <summary>Validates the fields of every entity that is about to be inserted or updated.</summary>
        private void ValidateTrackedEntities()
        {
            var errors = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .SelectMany(e => EntityValidator.GetErrors(e.Entity))
                .Distinct()
                .ToList();

            if (errors.Count > 0)
                throw new EntityValidationException(errors);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationDetail> ReservationDetails { get; set; }
        public DbSet<ReservationAudit> ReservationAudits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Columns, lengths and unique indexes ----
            // (unique indexes need a max length; SQL Server does not allow them on nvarchar(max))
            modelBuilder.Entity<User>(e =>
            {
                e.Property(u => u.Name).IsRequired().HasMaxLength(100);
                e.Property(u => u.Email).IsRequired().HasMaxLength(150);
                e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
                e.Property(u => u.Role).IsRequired().HasMaxLength(30);
                e.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<Guest>(e =>
            {
                e.Property(g => g.IdentityDocument).IsRequired().HasMaxLength(30);
                e.Property(g => g.FirstName).IsRequired().HasMaxLength(100);
                e.Property(g => g.LastName).IsRequired().HasMaxLength(100);
                e.Property(g => g.Phone).IsRequired().HasMaxLength(30);
                e.Property(g => g.Email).IsRequired().HasMaxLength(150);
                e.HasIndex(g => g.IdentityDocument).IsUnique();
                e.Property(g => g.BirthDate).HasColumnType("date");
            });

            modelBuilder.Entity<RoomType>(e =>
            {
                e.Property(t => t.Name).IsRequired().HasMaxLength(60);
            });

            modelBuilder.Entity<Hotel>(e =>
            {
                e.Property(h => h.Name).IsRequired().HasMaxLength(100);
                e.Property(h => h.Address).IsRequired().HasMaxLength(200);
                e.Property(h => h.City).IsRequired().HasMaxLength(80);
                e.Property(h => h.Phone).IsRequired().HasMaxLength(30);
                e.Property(h => h.Email).HasMaxLength(150);
                e.HasIndex(h => h.Name).IsUnique();
                e.Property(h => h.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<Block>(e =>
            {
                e.Property(b => b.Name).IsRequired().HasMaxLength(60);
                e.Property(b => b.Description).HasMaxLength(250);
                // A block name cannot repeat inside the same hotel
                e.HasIndex(b => new { b.HotelId, b.Name }).IsUnique();
                e.Property(b => b.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<Floor>(e =>
            {
                e.Property(f => f.Description).HasMaxLength(250);
                // A floor number cannot repeat inside the same block
                e.HasIndex(f => new { f.BlockId, f.Number }).IsUnique();
                e.Property(f => f.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<Room>(e =>
            {
                e.Property(r => r.Number).IsRequired().HasMaxLength(10);
                e.Property(r => r.CurrentStatus).IsRequired().HasMaxLength(30);
                // A room number cannot repeat on the same floor (the BL also checks it across the whole hotel)
                e.HasIndex(r => new { r.FloorId, r.Number }).IsUnique();
                e.Property(r => r.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<Reservation>(e =>
            {
                e.Property(r => r.ReservationCode).IsRequired().HasMaxLength(30);
                e.Property(r => r.Status).IsRequired().HasMaxLength(30);
                e.HasIndex(r => r.ReservationCode).IsUnique();
                // RNF-001: the availability search filters by dates
                e.HasIndex(r => new { r.CheckInDate, r.CheckOutDate });
            });

            modelBuilder.Entity<ReservationAudit>(e =>
            {
                e.Property(a => a.Action).IsRequired().HasMaxLength(30);
                e.Property(a => a.Details).HasMaxLength(500);
            });

            // ---- Relationships (section 12 of the analysis and design document) ----
            // Hotel 1:N Block
            modelBuilder.Entity<Block>()
                .HasOne<Hotel>().WithMany()
                .HasForeignKey(b => b.HotelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Block 1:N Floor
            modelBuilder.Entity<Floor>()
                .HasOne<Block>().WithMany()
                .HasForeignKey(f => f.BlockId)
                .OnDelete(DeleteBehavior.Restrict);

            // Floor 1:N Room
            modelBuilder.Entity<Room>()
                .HasOne<Floor>().WithMany()
                .HasForeignKey(r => r.FloorId)
                .OnDelete(DeleteBehavior.Restrict);

            // RoomType 1:N Room
            modelBuilder.Entity<Room>()
                .HasOne<RoomType>().WithMany()
                .HasForeignKey(r => r.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Guest 1:N Reservation
            modelBuilder.Entity<Reservation>()
                .HasOne<Guest>().WithMany()
                .HasForeignKey(r => r.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            // User 1:N Reservation (who handled the reservation)
            modelBuilder.Entity<Reservation>()
                .HasOne<User>().WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation 1:N ReservationDetail (composition: deleting the reservation deletes its details)
            modelBuilder.Entity<ReservationDetail>()
                .HasOne<Reservation>().WithMany()
                .HasForeignKey(d => d.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Room 1:N ReservationDetail
            modelBuilder.Entity<ReservationDetail>()
                .HasOne<Room>().WithMany()
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Audit (the log is never cascade-deleted)
            modelBuilder.Entity<ReservationAudit>()
                .HasOne<Reservation>().WithMany()
                .HasForeignKey(a => a.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReservationAudit>()
                .HasOne<User>().WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
