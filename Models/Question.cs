namespace backend.Models
{
    public class Question
    {
        public int QuestionID { get; set; }  // Primary key
        public int CategoryID { get; set; } // Foreign key for Category
        public Category Category { get; set; } // Navigation property for Category

        public string QuestionText { get; set; }
        public string Options { get; set; }  // JSON format
        public string CorrectAnswer { get; set; }
    }
}
