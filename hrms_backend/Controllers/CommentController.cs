using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hrms_backend.Models;

namespace hrms_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly HrmsDbContext _dbContext;

        public CommentController(HrmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public ActionResult<Comment> PostComment([FromBody] CommentInputModel commentInput)
        {
            if (commentInput == null || commentInput.FkCandidateId == null)
            {
                return BadRequest("Invalid comment data");
            }

            // Retrieve the employee ID and company ID from claims
            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            if (employeeIdClaim == null || companyIdClaim == null ||
                !int.TryParse(employeeIdClaim.Value, out int employeeId) ||
                !int.TryParse(companyIdClaim.Value, out int companyId))
            {
                return BadRequest("Invalid or missing Employee ID or Company ID claim");
            }

            var comment = new Comment
            {
                Text = commentInput.Text,
                FkCandidateId = commentInput.FkCandidateId,
                FkCompanyId = companyId,
                FkEmployeeId = employeeId,
                Time = DateTime.Now.TimeOfDay  // Set Time to the current time
            };

            _dbContext.Comments.Add(comment);
            _dbContext.SaveChanges();

            return Ok("success");
        }

        public class CommentInputModel
        {
            public string Text { get; set; }
            public int? FkCandidateId { get; set; }
            public int? FkCompanyId { get; set; }
        }
        [HttpGet("GetCommentsByCandidateId/{candidateId}")]
        public ActionResult<IEnumerable<CommentDto>> GetCommentsByCandidateId(int candidateId)
        {
            var comments = _dbContext.Comments
                .Include(c => c.FkEmployee)
                .Where(c => c.FkCandidateId == candidateId)
                .Select(c => new CommentDto
                {
                    CommentId = c.CommentId,
                    Text = c.Text,
                    FkCandidateId = c.FkCandidateId,
                    FkCompanyId = c.FkCompanyId,
                    FkEmployeeId = c.FkEmployeeId,
                    EmployeeName = c.FkEmployee != null ? c.FkEmployee.FullName : "Unknown",
                    EmployeeImage = c.FkEmployee != null ? c.FkEmployee.Image : "Unknown",
                    TimeAgo = CommentController.GetTimeAgo(c.Time)  // Make the method call static
                })
                .ToList();

            return Ok(comments);
        }

        // Static helper method to calculate time ago
        public static string GetTimeAgo(TimeSpan? time)
        {
            if (time.HasValue)
            {
                var timeDifference = DateTime.Now.TimeOfDay - time.Value;
                // Use the TimeSpan properties (Minutes, Seconds) to format the time difference
                if (timeDifference.TotalMinutes < 1)
                {
                    return "Just now";
                }
                else if (timeDifference.TotalMinutes < 60)
                {
                    return $"{(int)timeDifference.TotalMinutes} min ago";
                }
                // Add more conditions as needed (e.g., hours, days, etc.)
            }

            return "Unknown";
        }
        public class CommentDto
        {
            public int CommentId { get; set; }
            public string Text { get; set; }
            public int? FkCandidateId { get; set; }
            public int? FkCompanyId { get; set; }
            public int? FkEmployeeId { get; set; }
            public string EmployeeName { get; set; }
            public string EmployeeImage { get; set; }
            public string TimeAgo { get; set; }
        }
        [HttpDelete("DeleteComment/{commentId}")]
        public ActionResult DeleteComment(int commentId)
        {
            var comment = _dbContext.Comments.Find(commentId);

            if (comment == null)
            {
                return NotFound("Comment not found");
            }

            _dbContext.Comments.Remove(comment);
            _dbContext.SaveChanges();

            return Ok("Comment deleted successfully");
        }
    
    }
}