﻿using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly AppDbContext _context; // Injected AppDbContext

        public QuizController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("categories"), Authorize]
        public IActionResult GetCategories()
        {
            try
            {
                var categories = _context.Categories.ToList();
                Console.WriteLine("categories returned successfully");
                return Ok(new { message = "Categories are: ", data = categories });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("all-quizzes"), Authorize(Roles = "Teacher")]
        public IActionResult GetQuizzes()
        {
            try
            {
                // Fetch data by joining Submission, User, and Category tables
                var quizResults = _context.Submissions
                    .Select(s => new
                    {
                        UserName = s.User.FirstName + " " + s.User.LastName, // Combine FirstName and LastName
                        CategoryName = s.Category.Name, // Get Category name
                        QuizID = s.QuizID,
                        MarksObtained = s.MarksObtained,
                        TotalMarks = s.TotalMarks
                    })
                    .ToList();
                return Ok(new { message = "Quiz Records are", data = quizResults });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("user"), Authorize(Roles = "Student")]
        public IActionResult GetQuizzesById()
        {
            try
            {
                // Get the current user's ID from the claims
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;

                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId))
                {
                    return Unauthorized("Invalid or missing user ID.");
                }

                // Fetch quizzes for the logged-in teacher
                var quizResults = _context.Quizzes
                    .Where(q => q.UserId == parsedUserId) // Match the quizzes to the logged-in teacher
                    .Select(q => new
                    {
                        QuizID = q.QuizID,
                        CategoryName = q.CategoryName,
                        MarksObtained = q.MarksObtained,
                        TotalMarks = q.TotalMarks
                    })
                    .ToList();

                return Ok(new
                {
                    message = "Quiz records fetched successfully.",
                    data = quizResults
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new
                {
                    message = "An error occurred while fetching quiz records.",
                    error = ex.Message
                });
            }
        }


        [HttpGet("category"), Authorize]
        public IActionResult GetQuizzesByCategory([FromQuery] string categoryName)
        {
            try
            {
                // Fetch questions by category name
                var questions = _context.Questions
                    .Where(q => q.Category.Name == categoryName) // Filter by category name
                    .Select(q => new
                    {
                        QuestionID = q.QuestionID,
                        CategoryName = q.Category.Name,
                        QuestionText = q.QuestionText,
                        Options = q.Options,
                        CorrectAnswer = q.CorrectAnswer
                    })
                    .ToList();

                return Ok(new
                {
                    message = "Questions fetched successfully.",
                    data = questions
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new
                {
                    message = "An error occurred while fetching questions.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("record/quizzies/submission")]
        public async Task<IActionResult> RecordQuizSubmission([FromBody] QuizDto quizDto)
        {
            Console.WriteLine("Record quizzes called");
            Console.WriteLine($"Quiz submission received: {System.Text.Json.JsonSerializer.Serialize(quizDto)}");

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed.", errors = ModelState });
            }

            if (quizDto == null || quizDto.UserId == 0 || string.IsNullOrEmpty(quizDto.CategoryName))
            {
                return BadRequest(new { message = "Invalid quiz submission data.", errors = "User or Category field is missing." });
            }

            try
            {
                // Map QuizDto to Quiz model
                var quiz = new Quiz
                {
                    CategoryName = quizDto.CategoryName,
                    MarksObtained = quizDto.MarksObtained,
                    TotalMarks = quizDto.TotalMarks,
                    StartTime = quizDto.StartTime,
                    EndTime = quizDto.EndTime,
                    UserId = quizDto.UserId
                };

                // Add quiz submission to the database
                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();

                // Return the quiz data along with the auto-generated QuizID
                return Ok(new
                {
                    message = "Quiz submission recorded successfully.",
                    data = new
                    {
                        quiz.QuizID,
                        quiz.CategoryName,
                        quiz.MarksObtained,
                        quiz.TotalMarks,
                        quiz.StartTime,
                        quiz.EndTime,
                        quiz.UserId
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return BadRequest(new { message = "An error occurred while recording the quiz submission.", error = ex.Message });
            }
        }


        [HttpPost("record/submission")]
        public async Task<IActionResult> RecordSubmission([FromBody] SubmissionDto submission)
        {
            Console.WriteLine("Record submission called");
            Console.WriteLine($"Submission received: {System.Text.Json.JsonSerializer.Serialize(submission)}");

            if (submission == null || submission.UserId == 0 || submission.CategoryID == 0 || submission.QuizID == 0)
            {
                return BadRequest(new { message = "Invalid submission data.", errors = "User, Category, or Quiz field is missing." });
            }

            try
            {
                var _submission = new Submission
                {
                    UserId = submission.UserId,
                    CategoryID = submission.CategoryID,
                    QuizID = submission.QuizID,
                    MarksObtained = submission.MarksObtained,
                    TotalMarks = submission.TotalMarks,
                    StartTime = submission.StartTime,
                    EndTime = submission.EndTime,
                    AnsweredQuestions = submission.AnsweredQuestions
                };

                // Add submission to the database
                _context.Submissions.Add(_submission);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = "Quiz submission recorded successfully.",
                    data = new
                    {
                        _submission.QuizID,
                        _submission.CategoryID,
                        _submission.MarksObtained,
                        _submission.TotalMarks,
                        _submission.StartTime,
                        _submission.EndTime,
                        _submission.UserId
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "An error occurred while recording the submission.", error = ex.Message });
            }
        }


        //[HttpGet("get-record/quizId")]
        //public async Task<IActionResult> GetQuizRecord([FromQuery] int quizId)
        //{
        //    Console.WriteLine("Get Quiz Record called with quiz id as " + quizId);
        //    if (quizId == null)
        //    {
        //        return BadRequest(new { message = "Quiz Id is null", errors = "Quiz Id should not be null" });
        //    }

        //    try
        //    {
        //        var _submission = _context.Submissions.FirstOrDefault(s => s.QuizID == quizId);
        //        if (_submission == null)
        //        {
        //            return BadRequest(new { message = "No Submissions found against mentioned quizId " + quizId, errors = "No records exists in submission table against that quizId" });
        //        }
        //        return Ok(new
        //        {
        //            message = "Submission returned Success against." + quizId,
        //            data = new
        //            {

        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return BadRequest(new { message = "An error occurred while recording the submission.", error = ex.Message });
        //    }
        //}


        [HttpGet("get-record/quizId")]
        public async Task<IActionResult> GetQuizRecord([FromQuery] int quizId)
        {
            Console.WriteLine("Get Quiz Record called with quiz id as " + quizId);

            if (quizId == 0)
            {
                return BadRequest(new
                {
                    message = "Quiz Id is null",
                    errors = "Quiz Id should not be null"
                });
            }

            try
            {
                // Fetch the submission for the given quiz ID
                var submission = _context.Submissions.FirstOrDefault(s => s.QuizID == quizId);
                if (submission == null)
                {
                    return BadRequest(new
                    {
                        message = "No submissions found for the provided quizId: " + quizId,
                        errors = "No records exist in the submissions table for that quizId"
                    });
                }

                // Deserialize AnsweredQuestions (client handles parsing, so raw JSON is returned)
                var answeredQuestions = submission.AnsweredQuestions;

                // Extract question IDs from AnsweredQuestions
                var questionIds = JsonConvert.DeserializeObject<List<dynamic>>(answeredQuestions)
                    .Select(q => (int)q.questionID).ToList();

                // Fetch questions from the Questions table based on extracted question IDs
                var questions = _context.Questions
                    .Where(q => questionIds.Contains(q.QuestionID))
                    .Select(q => new
                    {
                        q.QuestionID,
                        q.QuestionText,
                        q.Options,        // Return raw JSON for options
                        q.CorrectAnswer   // Correct answer for the question
                    })
                    .ToList();

                // Construct the response payload
                return Ok(new
                {
                    message = "Submission data retrieved successfully for quizId: " + quizId,
                    data = new
                    {
                        submissionId = submission.SubmissionID,
                        userId = submission.UserId,
                        categoryId = submission.CategoryID,
                        quizId = submission.QuizID,
                        marksObtained = submission.MarksObtained,
                        totalMarks = submission.TotalMarks,
                        startTime = submission.StartTime,
                        endTime = submission.EndTime,
                        answeredQuestions = answeredQuestions, // Raw JSON to be parsed on the client
                        questions = questions                  // Question details
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new
                {
                    message = "An error occurred while retrieving the submission data.",
                    error = ex.Message
                });
            }
        }


    }
}
