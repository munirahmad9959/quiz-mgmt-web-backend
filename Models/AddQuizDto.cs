namespace backend.Models
{
    public class AddQuizDto
    {
        public string CorrectAnswer { get; set; }
        public string Options { get; set; }
        public string QuestionText { get; set; }

    }
}
