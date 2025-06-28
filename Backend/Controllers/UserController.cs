using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims; // Make sure this is included
using Microsoft.AspNetCore.Authorization;
namespace Backend.Controllers
{
    [ApiController]
    // Option 1 (Recommended for clarity): Explicitly set route to "user" (singular)
    [Route("api/User")] // <--- Changed from "api/[controller]" for clarity with frontend
    // Option 2 (If you prefer [controller]): Leave as [Route("api/[controller]")]
    // And ensure frontend calls /api/Users if controller class is UserController
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/User/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("User already exists with this email.");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Mobile = dto.Mobile,
                PasswordHash = HashPassword(dto.Password),
                Status = "Pending",
                Role = "User",
                Balance = 0 // Initial balance for new users
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Registration successful! Awaiting admin approval.");
        }

        // POST: api/User/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || user.PasswordHash != HashPassword(dto.Password))
                return Unauthorized("Invalid email or password.");

            if (user.Status != "Approved")
                return Unauthorized("Account not approved yet.");

            var token = GenerateJwtToken(user);

            // Ensure property names here match what frontend's Login.js expects (camelCase)
            return Ok(new
            {
                token = token,          // Corrected to lowercase 't'
                userId = user.UserID,   // Corrected to camelCase 'userId'
                name = user.Name,       // Corrected to lowercase 'n'
                email = user.Email,     // Corrected to lowercase 'e'
                balance = user.Balance, // Corrected to lowercase 'b'
                role = user.Role        // Corrected to lowercase 'r'
            });
        }

        // GET: api/User/{id} - For fetching user's own profile and balance (needs [Authorize])
        // ADD THIS METHOD (if it's not already there from previous instructions)
        [HttpGet("{id}")]
        [Authorize] // This endpoint requires authorization
        public async Task<IActionResult> GetUserById(int id)
        {
            // Security check: Ensure the logged-in user is requesting their own ID
            // The ClaimTypes.NameIdentifier should now be present in the token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId) || currentUserId != id)
            {
                // Optionally allow admins to view other users, but for now, strict self-access
                return Forbid("You are not authorized to view this user's data.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                userId = user.UserID,
                name = user.Name,
                email = user.Email,
                balance = user.Balance,
                // Do not return password hash
            });
        }


        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings.GetValue<string>("SecretKey")!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim> // Use List<Claim> for easier addition
            {
                // CRITICAL FIX: ADD ClaimTypes.NameIdentifier
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()), // <--- ADD THIS LINE!

                new Claim(JwtRegisteredClaimNames.Sub, user.Email), // 'sub' is typically the unique subject
                new Claim("userId", user.UserID.ToString()), // Your custom userId claim
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email), // Added email to claims
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings.GetValue<string>("Issuer"),
                audience: jwtSettings.GetValue<string>("Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}