// Backend/Controllers/BeneficiaryController.cs
using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // For ClaimTypes.NameIdentifier

namespace Backend.Controllers
{
    [Authorize]  // Protects all endpoints with JWT
    [ApiController]
    [Route("api/[controller]")] // Base route for this controller (e.g., /api/Beneficiary)
    public class BeneficiaryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BeneficiaryController(AppDbContext context)
        {
            _context = context;
        }

        // Helper method to get the current user's ID from the JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); // Uses ClaimTypes.NameIdentifier
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User ID (NameIdentifier) not found in token or invalid format.");
            }
            return userId;
        }

        // GET: api/Beneficiary/{requestingUserId} - Get all beneficiaries for a specific user
        [HttpGet("{requestingUserId}")] // Note: Frontend will call /api/Beneficiary/userId
        public async Task<IActionResult> GetBeneficiaries(int requestingUserId)
        {
            int ownerUserId = GetCurrentUserId();
            // Security check: Ensure the requesting user ID matches the authenticated user's ID
            if (ownerUserId != requestingUserId)
            {
                return Forbid("You are not authorized to view these beneficiaries.");
            }

            // Fetch beneficiaries, including their associated User details (BeneficiaryUser)
            var beneficiaries = await _context.Beneficiaries
                .Include(b => b.BeneficiaryUser) // Eager load the BeneficiaryUser object
                .Where(b => b.OwnerUserId == ownerUserId) // Filter by the owner (logged-in user)
                .Select(b => new // Project to an anonymous object (camelCase) for the frontend
                {
                    beneficiaryID = b.BeneficiaryId, // ID of the beneficiary *relationship*
                    name = b.BeneficiaryUser.Name,   // Name of the actual beneficiary user
                    accountNumber = b.BeneficiaryUser.Email // Email of the actual beneficiary user
                })
                .ToListAsync();

            return Ok(beneficiaries); // Return the list of beneficiaries
        }

        // POST: api/Beneficiary/add - Add a new beneficiary
        [HttpPost("add")]
        public async Task<IActionResult> AddBeneficiary([FromBody] AddBeneficiaryDto dto)
        {
            if (!ModelState.IsValid) // Basic validation from DTO attributes
            {
                return BadRequest(ModelState);
            }

            int ownerUserId = GetCurrentUserId();

            // Find the potential beneficiary user by their email (accountNumber) and ensure they are Approved
            var beneficiaryUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.AccountNumber && u.Status == "Approved");

            if (beneficiaryUser == null)
            {
                return BadRequest("Recipient user not found or not approved.");
            }

            // --- NEW FEATURE: Prevent adding an Admin as a beneficiary ---
            if (beneficiaryUser.Role == "Admin") // Check the 'Role' property of the found user
            {
                return BadRequest("Administrators cannot be added as beneficiaries.");
            }
            // --- END NEW FEATURE ---

            // Prevent a user from adding themselves as a beneficiary
            if (ownerUserId == beneficiaryUser.UserID)
            {
                return BadRequest("You cannot add yourself as a beneficiary.");
            }

            // Check if this specific beneficiary relationship already exists for this owner
            bool beneficiaryExists = await _context.Beneficiaries.AnyAsync(b =>
                b.OwnerUserId == ownerUserId &&
                b.BeneficiaryUserId == beneficiaryUser.UserID);

            if (beneficiaryExists)
            {
                return Conflict("This beneficiary has already been added by you."); // Use Conflict (409) for existing resource
            }

            // Create the new Beneficiary relationship record
            var beneficiary = new Beneficiary
            {
                OwnerUserId = ownerUserId,
                BeneficiaryUserId = beneficiaryUser.UserID,
                DateAdded = DateTime.UtcNow
                // Note: 'BeneficiaryName' field is not in your current Beneficiary model,
                // so it's not set here. The name is retrieved via BeneficiaryUser.Name when needed.
            };

            _context.Beneficiaries.Add(beneficiary); // Add to EF context
            await _context.SaveChangesAsync(); // Save changes to the database

            // Return 201 Created status with the newly created beneficiary info and a success message
            return StatusCode(201, new
            {
                beneficiaryID = beneficiary.BeneficiaryId, // ID of the relationship
                name = beneficiaryUser.Name,               // Name of the actual user who is the beneficiary
                accountNumber = beneficiaryUser.Email,     // Email of the actual user who is the beneficiary
                message = "Beneficiary added successfully!" // Consistent success message
            });
        }

        // DELETE: api/Beneficiary/{id} - Remove a beneficiary for the logged-in user
        // This method already exists in your provided code, confirming its availability.
        [HttpDelete("{id}")] // {id} refers to the BeneficiaryId (the relationship ID)
        public async Task<IActionResult> RemoveBeneficiary(int id)
        {
            int ownerUserId = GetCurrentUserId(); // Get the ID of the currently authenticated user

            // Find the beneficiary relationship ensuring it belongs to the logged-in user
            var beneficiary = await _context.Beneficiaries
                .FirstOrDefaultAsync(b => b.BeneficiaryId == id && b.OwnerUserId == ownerUserId);

            if (beneficiary == null)
            {
                // Return 404 Not Found if the beneficiary relationship doesn't exist
                // or if it exists but doesn't belong to the current user
                return NotFound("Beneficiary not found or you don't own it.");
            }

            _context.Beneficiaries.Remove(beneficiary); // Mark for removal
            await _context.SaveChangesAsync(); // Commit the removal to the database

            // Return 200 OK with a success message
            return Ok("Beneficiary removed successfully.");
        }
    }
}