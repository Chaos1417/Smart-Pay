// Backend/Data/AppDbContext.cs (CORRECTED)
using Microsoft.EntityFrameworkCore;
using Backend.Models; // Ensure this is present

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Beneficiary> Beneficiaries { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Call the base method first

            // Configure decimal precision for currency fields
            modelBuilder.Entity<User>()
                .Property(u => u.Balance)
                .HasColumnType("decimal(18, 2)"); // Use 18, 2 for precision

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18, 2)"); // Use 18, 2 for precision

            // Configure Beneficiary relationships
            // A user can own many beneficiaries (the ones they added)
            modelBuilder.Entity<Beneficiary>()
                .HasOne(b => b.OwnerUser)
                .WithMany(u => u.OwnedBeneficiaries) // Matches ICollection<Beneficiary> OwnedBeneficiaries in User.cs
                .HasForeignKey(b => b.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // A user can be a beneficiary for many others
            modelBuilder.Entity<Beneficiary>()
                .HasOne(b => b.BeneficiaryUser)
                .WithMany(u => u.BeneficiaryForUsers) // Matches ICollection<Beneficiary> BeneficiaryForUsers in User.cs
                .HasForeignKey(b => b.BeneficiaryUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Add unique constraint for Beneficiary relationship (OwnerId + BeneficiaryId should be unique)
            modelBuilder.Entity<Beneficiary>()
                .HasIndex(b => new { b.OwnerUserId, b.BeneficiaryUserId })
                .IsUnique();


            // Configure Transaction relationships
            // A transaction has one Sender, a User can have many SentTransactions
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Sender)
                .WithMany(u => u.SentTransactions) // Matches ICollection<Transaction> SentTransactions in User.cs
                .HasForeignKey(t => t.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict to prevent accidental deletion

            // A transaction has one Receiver, a User can have many ReceivedTransactions
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Receiver)
                .WithMany(u => u.ReceivedTransactions) // Matches ICollection<Transaction> ReceivedTransactions in User.cs
                .HasForeignKey(t => t.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict to prevent accidental deletion
        }
    }
}