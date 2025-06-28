// Backend/Models/Beneficiary.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Beneficiary
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BeneficiaryId { get; set; } // Primary key for the beneficiary relationship

        // The user who added this beneficiary
        public int OwnerUserId { get; set; }
        [ForeignKey("OwnerUserId")]
        public User OwnerUser { get; set; } = null!;// Navigation property to the user who created this beneficiary record

        // The user who IS the beneficiary (the recipient of funds)
        public int BeneficiaryUserId { get; set; }
        [ForeignKey("BeneficiaryUserId")]
        public User BeneficiaryUser { get; set; } = null!;// Navigation property to the actual user who is the beneficiary

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}