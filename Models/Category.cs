namespace backend.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }

        // Navigation property for Quizzes
        public ICollection<Quiz> Quizzes { get; set; }

        // Navigation property for Questions
        public ICollection<Question> Questions { get; set; }
    }
}
