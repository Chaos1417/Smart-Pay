// Backend/Models/AddBeneficiaryDto.cs
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class AddBeneficiaryDto
    {
        [Required]
        public string Name { get; set; } = null!;// Frontend sends this, but we'll use it to find the user
        [Required]
        public string AccountNumber { get; set; }= null!; // Frontend sends this (e.g., email), used to find BeneficiaryUserId
    }
}