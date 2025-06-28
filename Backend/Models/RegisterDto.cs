// Backend/Models/RegisterDto.cs
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class RegisterDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; }  = null!;// Will be hashed in controller

        [Required]
        [StringLength(20)]
        public string Mobile { get; set; } = null!;
    }
}