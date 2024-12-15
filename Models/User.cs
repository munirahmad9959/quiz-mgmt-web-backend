using System.Collections.Generic;
namespace backend.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? ResetToken { get; set; } // Nullable string, no need for default value
        public DateTime? TokenExpiry { get; set; } // Nullable DateTime for token expiration

        public string Role { get; set; } = string.Empty;

        // Navigation property to quizzes and submissions
        public ICollection<Quiz> Quizzes { get; set; }  // User can have many quizzes
        public ICollection<Submission> Submissions { get; set; }  // User can have many submissions
    }
}
