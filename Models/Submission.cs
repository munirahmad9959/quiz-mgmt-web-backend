namespace backend.Models
{
    public class Submission
    {
        public int SubmissionID { get; set; }  // Primary key

        // Foreign key references to User and Category
        public int UserId { get; set; }  // Foreign key to User
        public User User { get; set; }  // Navigation property to User

        public int CategoryID { get; set; }  // Foreign key to Category
        public Category Category { get; set; }  // Navigation property to Category

        // Foreign key reference to Quiz (the specific quiz taken)
        public int QuizID { get; set; }
        public Quiz Quiz { get; set; }

        public int MarksObtained { get; set; }  // Marks obtained in this submission
        public int TotalMarks { get; set; }  // Total marks possible in the quiz
        public DateTime? StartTime { get; set; }  // Start time of the quiz
        public DateTime? EndTime { get; set; }  // End time of the quiz

        // Optionally store the answered questions in a serialized format (JSON, for example)
        public string AnsweredQuestions { get; set; }  // Store answers as JSON or another format
    }
}
