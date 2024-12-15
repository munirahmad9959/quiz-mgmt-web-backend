namespace backend.Models
{
    public class SubmissionDto
    {
        public int UserId{ get; set; }
        public int CategoryID{ get; set; }

        public int QuizID { get; set; }

        public int MarksObtained { get; set; }

        public int TotalMarks { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string AnsweredQuestions { get; set; }   // Store answers as JSON or another format

    }
}
