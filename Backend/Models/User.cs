// Backend/Models/User.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Mobile { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Role { get; set; }  = null!;// e.g., "User", "Admin"

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = null!; // e.g., "Active", "Pending", "Suspended"

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties for EF Core relationships
        public ICollection<Beneficiary> OwnedBeneficiaries { get; set; }  = null!;// Beneficiaries added by this user
        public ICollection<Beneficiary> BeneficiaryForUsers { get; set; }  = null!;// When this user is a beneficiary for someone else
        public ICollection<Transaction> SentTransactions { get; set; } = null!;
        public ICollection<Transaction> ReceivedTransactions { get; set; } = null!;
    }
}