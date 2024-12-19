namespace backend.Models
{
    public class EditQuizDto
    {
        public int QuestionID { get; set; }
        public string CorrectAnswer { get; set; }
        public string Options { get; set; }
        public string QuestionText { get; set; }
    }
}
