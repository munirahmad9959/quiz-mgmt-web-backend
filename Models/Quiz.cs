namespace backend.Models
{
    public class Quiz
    {
        public int QuizID { get; set; } // Primary Key
        public string CategoryName { get; set; } // Name of the Category
        public int MarksObtained { get; set; }
        public int TotalMarks { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // Foreign Key to User
        public int UserId { get; set; }  // Reference to User
        public User User { get; set; }    // Navigation property to User
    }
}
