// Backend/Models/TransferRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class TransferRequestDto
    {
        [Required]
        public int SenderUserID { get; set; } // ID of the user initiating the transfer
        [Required]
        public int BeneficiaryID { get; set; } // ID of the Beneficiary *relationship*
        [Required]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
         public string? Description { get; set; }
        public decimal Amount { get; set; }
    }
}