using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class UserLoginDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(50)]
        public required string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [MaxLength(50)]
        public required string Password { get; set; }
    }
}
