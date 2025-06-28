using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/admin/pending-users
        [HttpGet("pending-users")]
        public async Task<IActionResult> GetPendingUsers()
        {
            var pendingUsers = await _context.Users
                .Where(u => u.Status == "Pending")
                .Select(u => new
                {
                    u.UserID,
                    u.Name,
                    u.Email,
                    u.Mobile,
                    u.Status,
                    u.Balance,
                    u.Role
                })
                .ToListAsync();

            return Ok(pendingUsers);
        }

        // POST: api/admin/approve-user/{id}
        [HttpPost("approve-user/{id}")]
        public async Task<IActionResult> ApproveUser(int id, [FromBody] ApproveUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("User not found.");

            if (user.Status == "Approved")
                return BadRequest("User is already approved.");

            // Approve user and set balance
            user.Status = "Approved";
            user.Balance = request.Balance;

            await _context.SaveChangesAsync();

            return Ok($"User {user.Name} approved with balance {user.Balance}.");
        }
        // POST: api/admin/reject-user/{id}
        [HttpPost("reject-user/{id}")]
        public async Task<IActionResult> RejectUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("User not found.");

            if (user.Status != "Pending")
                return BadRequest("Only pending users can be rejected.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok($"User {user.Name} has been rejected and deleted.");
        }

    }

    public class ApproveUserRequest
    {
        public decimal Balance { get; set; }
    }
}
