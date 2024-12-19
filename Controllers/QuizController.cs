using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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


        [HttpGet("get-quizzes/catName")]
        public async Task<IActionResult> GetQuizzesfromDb([FromQuery] string catName)
        {
            Console.WriteLine("Get Quizzes from Db called with category name: " + catName);
            if (string.IsNullOrWhiteSpace(catName))
            {
                return BadRequest(new
                {
                    message = "Category name is null or empty",
                    errors = "Category name should not be null or empty"
                });
            }

            try
            {
                // Fetch the questions for the given category name
                var quizResults = _context.Questions
                    .Where(q => q.Category.Name == catName) // Filter by Category Name
                    .Select(q => new
                    {
                        QuestionID = q.QuestionID,
                        CatName = q.Category.Name,
                        Quest = q.QuestionText,
                        Options = q.Options,
                        CorrectOptions = q.CorrectAnswer
                    })
                    .ToList();

                if (!quizResults.Any())
                {
                    return NotFound(new
                    {
                        message = "No quizzes found for the given category name.",
                        data = quizResults
                    });
                }

                // Construct the response payload
                return Ok(new
                {
                    message = "Submission data retrieved successfully for category name: " + catName,
                    data = quizResults
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


        [HttpPost("add-quizzes/catName")]
        public async Task<IActionResult> AddQuizzestoDb([FromQuery] string catName, [FromBody] AddQuizDto quizData)
        {
            Console.WriteLine("Received quiz data for category: " + catName);

            // Validate category name
            if (string.IsNullOrWhiteSpace(catName))
            {
                return BadRequest(new
                {
                    message = "Category name is null or empty",
                    errors = "Category name should not be null or empty"
                });
            }

            // Validate quiz data
            if (string.IsNullOrWhiteSpace(quizData.QuestionText))
            {
                return BadRequest(new
                {
                    message = "Question text is null or empty",
                    errors = "Question text should not be null or empty"
                });
            }

            if (string.IsNullOrWhiteSpace(quizData.CorrectAnswer))
            {
                return BadRequest(new
                {
                    message = "Correct answer is null or empty",
                    errors = "Correct answer should not be null or empty"
                });
            }

            if (string.IsNullOrWhiteSpace(quizData.Options) || !IsValidJson(quizData.Options))
            {
                return BadRequest(new
                {
                    message = "Options are not valid JSON.",
                    errors = "Ensure the options are a valid JSON string."
                });
            }

            try
            {
                var category = _context.Categories.FirstOrDefault(c => c.Name == catName);

                if (category == null)
                {
                    return NotFound(new
                    {
                        message = "Category not found",
                        errors = $"No category found with the name '{catName}'"
                    });
                }

                // Create new Question entity
                var newQuestion = new Question
                {
                    CategoryID = category.CategoryID,
                    QuestionText = quizData.QuestionText,
                    Options = quizData.Options,
                    CorrectAnswer = quizData.CorrectAnswer
                };

                // Add new Question to the database
                _context.Questions.Add(newQuestion);
                await _context.SaveChangesAsync();

                // Return success response
                return Ok(new
                {
                    message = "Quiz added successfully",
                    questionID = newQuestion.QuestionID,
                    categoryName = category.Name,
                    questionText = newQuestion.QuestionText,
                    correctAnswer = newQuestion.CorrectAnswer
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new
                {
                    message = "Internal server error occurred.",
                    error = ex.Message
                });
            }
        }

        private bool IsValidJson(string json)
        {
            try
            {
                JToken.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }


        [HttpPost("edit-quizzes/catName")]
        public async Task<IActionResult> EditQuizzesinDb([FromQuery] string catName, [FromBody] EditQuizDto quizData)
        {
            Console.WriteLine($"Received quiz data for editing with category name: {catName}");

            // Step 1: Validate category name
            if (string.IsNullOrWhiteSpace(catName))
            {
                return BadRequest(new
                {
                    message = "Category name is null or empty",
                    errors = "Category name should not be null or empty"
                });
            }

            // Step 2: Validate quiz data
            if (quizData.QuestionID <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid QuestionID",
                    errors = "QuestionID must be a valid positive integer"
                });
            }

            if (string.IsNullOrWhiteSpace(quizData.QuestionText))
            {
                return BadRequest(new
                {
                    message = "Question text is null or empty",
                    errors = "Question text should not be null or empty"
                });
            }

            if (string.IsNullOrWhiteSpace(quizData.CorrectAnswer))
            {
                return BadRequest(new
                {
                    message = "Correct answer is null or empty",
                    errors = "Correct answer should not be null or empty"
                });
            }

            if (string.IsNullOrWhiteSpace(quizData.Options) || !IsValidJson(quizData.Options))
            {
                return BadRequest(new
                {
                    message = "Options are not valid JSON.",
                    errors = "Ensure the options are a valid JSON string."
                });
            }

            try
            {
                // Step 3: Validate category
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == catName);
                if (category == null)
                {
                    return NotFound(new
                    {
                        message = "Category not found",
                        errors = $"No category found with the name '{catName}'"
                    });
                }

                // Step 4: Validate question existence
                var existingQuestion = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionID == quizData.QuestionID);
                if (existingQuestion == null)
                {
                    return NotFound(new
                    {
                        message = "Question not found",
                        errors = $"No question found with the ID '{quizData.QuestionID}'"
                    });
                }

                // Step 5: Update question
                existingQuestion.CategoryID = category.CategoryID; // Update the category
                existingQuestion.QuestionText = quizData.QuestionText;
                existingQuestion.Options = quizData.Options;
                existingQuestion.CorrectAnswer = quizData.CorrectAnswer;

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Step 6: Return success response
                return Ok(new
                {
                    message = "Quiz updated successfully",
                    questionID = existingQuestion.QuestionID,
                    categoryName = category.Name,
                    questionText = existingQuestion.QuestionText,
                    correctAnswer = existingQuestion.CorrectAnswer
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while editing quiz: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Internal server error occurred.",
                    error = ex.Message
                });
            }
        }

        [HttpDelete("delete/quiz/{quizId}")] // Corrected route
        public async Task<IActionResult> DeleteQuizfromDb(int quizId) // Removed FromQuery attribute
        {
            Console.WriteLine($"Received request to delete quiz with ID: {quizId}");

            // Step 1: Validate quizId
            if (quizId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid Quiz ID",
                    errors = "Quiz ID must be a valid positive integer"
                });
            }

            try
            {
                // Step 2: Find the quiz by ID
                var existingQuiz = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionID == quizId);
                if (existingQuiz == null)
                {
                    return NotFound(new
                    {
                        message = "Quiz not found",
                        errors = $"No quiz found with the ID '{quizId}'"
                    });
                }

                // Step 3: Remove the quiz
                _context.Questions.Remove(existingQuiz);
                await _context.SaveChangesAsync();

                // Step 4: Return success response
                return Ok(new
                {
                    message = "Quiz deleted successfully",
                    quizId = quizId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while deleting quiz: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Internal server error occurred.",
                    error = ex.Message
                });
            }
        }


    }
}
