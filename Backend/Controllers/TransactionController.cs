// Backend/Controllers/TransactionController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Backend.Models; // Ensure this points to your Transaction model
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionController(AppDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID (NameIdentifier) not found in token or invalid format.");
            }
            return userId;
        }

        // GET: api/Transaction/history - Unified endpoint for all transaction history
        [HttpGet("history")]
        public async Task<IActionResult> GetTransactionHistory()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Fetch ALL transactions involving the current user.
                var transactions = await _context.Transactions
                    .Include(t => t.Sender)    // Include sender details for display
                    .Include(t => t.Receiver)   // Include receiver details for display
                    .Where(t => t.SenderId == currentUserId || t.ReceiverId == currentUserId) // User is either sender OR receiver
                    .OrderByDescending(t => t.Date) // Order by date, newest first
                    .ToListAsync();

                // Project to a TransactionDisplayDto that includes context-aware details
                var transactionDisplayDtos = transactions.Select(t =>
                {
                    string transactionTypeForDisplay;
                    string transactionDescriptionForDisplay;
                    decimal amountForDisplay = Math.Abs(t.Amount); // Always use absolute amount for the DTO

                    // Determine the display type and description based on current user's role in THIS transaction
                    if (t.SenderId == currentUserId)
                    {
                        transactionTypeForDisplay = "Outgoing";
                        // For outgoing, append the stored description to indicate who it went to.
                        if (t.Description != null && t.Description.StartsWith("To ")) {
                             transactionDescriptionForDisplay = t.Description;
                        } else {
                            transactionDescriptionForDisplay = $"To {t.Receiver.Name} ({t.Receiver.Email}): {t.Description ?? "Transfer"}";
                        }
                    }
                    else // t.ReceiverId == currentUserId (It's an incoming transaction)
                    {
                        transactionTypeForDisplay = "Incoming";
                        // For incoming, append the stored description to indicate who it came from.
                        if (t.Description != null && t.Description.StartsWith("From ")) {
                            transactionDescriptionForDisplay = t.Description;
                        } else {
                            transactionDescriptionForDisplay = $"From {t.Sender.Name}: {t.Description ?? "Transfer"}";
                        }
                    }

                    return new TransactionDisplayDto
                    {
                        TransactionId = t.TransactionId,
                        Date = t.Date,
                        Amount = amountForDisplay, // DTO always sends positive amount
                        Description = transactionDescriptionForDisplay, // This is the contextualized description
                        Type = transactionTypeForDisplay,      // "Outgoing" or "Incoming" from current user's perspective
                        SenderName = t.Sender.Name,
                        ReceiverName = t.Receiver.Name
                    };
                }).ToList();

                return Ok(transactionDisplayDtos);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving transactions for user: {ex}");
                return StatusCode(500, "An error occurred while fetching transaction history.");
            }
        }

        // POST: api/Transaction/transfer - Handle fund transfer (MODIFIED to add only ONE transaction)
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferFunds([FromBody] TransferRequestDto dto)
        {
            // --- ADDED DEBUGGING LOGS (KEEP THESE FOR FUTURE DEBUGGING) ---
            Console.WriteLine($"--- TransferRequestDto Received (Raw) ---");
            Console.WriteLine($"SenderUserID: {dto.SenderUserID}");
            Console.WriteLine($"BeneficiaryID: {dto.BeneficiaryID}");
            Console.WriteLine($"Amount: {dto.Amount}");
            Console.WriteLine($"Description: {(string.IsNullOrEmpty(dto.Description) ? "NULL/Empty" : dto.Description)}");
            Console.WriteLine($"-----------------------------------------");
            // --- END DEBUGGING LOGS ---


            if (!ModelState.IsValid)
            {
                // --- ADDED DETAILED MODELSTATE LOGGING (KEEP THESE FOR DEBUGGING) ---
                Console.WriteLine("--- ModelState Errors ---");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Any())
                    {
                        Console.WriteLine($"- Property: {state.Key}");
                        foreach (var error in state.Value.Errors)
                        {
                            Console.WriteLine($"  Error: {error.ErrorMessage}");
                        }
                    }
                }
                Console.WriteLine("-------------------------");
                // --- END DETAILED MODELSTATE LOGGING ---

                return BadRequest(ModelState);
            }

            var currentSenderId = GetCurrentUserId();
            if (currentSenderId != dto.SenderUserID)
            {
                return Forbid("You are not authorized to perform this transfer for another user.");
            }

            var sender = await _context.Users.FindAsync(dto.SenderUserID);
            if (sender == null || sender.Status != "Approved")
            {
                return BadRequest("Sender not found or not approved.");
            }

            var beneficiaryRelationship = await _context.Beneficiaries
                .Include(b => b.BeneficiaryUser)
                .FirstOrDefaultAsync(b => b.BeneficiaryId == dto.BeneficiaryID && b.OwnerUserId == sender.UserID);

            if (beneficiaryRelationship == null)
            {
                return NotFound("Beneficiary not found or does not belong to the sender.");
            }

            var receiver = beneficiaryRelationship.BeneficiaryUser;

            if (receiver == null || receiver.Status != "Approved")
            {
                return BadRequest("Recipient user is not found or not approved.");
            }

            if (sender.Balance < dto.Amount)
            {
                return BadRequest("Insufficient balance.");
            }

            using var databaseTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                sender.Balance -= dto.Amount;
                receiver.Balance += dto.Amount;

                _context.Users.Update(sender);
                _context.Users.Update(receiver);

                // --- RECORDING TRANSACTION (MODIFIED: ONLY ONE TRANSACTION RECORD IS ADDED) ---
                _context.Transactions.Add(new Transaction
                {
                    SenderId = sender.UserID,
                    ReceiverId = receiver.UserID,
                    Amount = dto.Amount,
                    Date = DateTime.UtcNow,
                    Type = "Transfer", // Store a generic "Transfer" type in DB for simplicity
                    Description = dto.Description // Store the full custom description from frontend
                });
                // --- REMOVED THE SECOND _context.Transactions.Add() CALL HERE ---

                await _context.SaveChangesAsync();
                await databaseTransaction.CommitAsync();

                // It's better to return the ID of the ONE transaction that was just created, not Last()
                // Assuming EF Core populates TransactionId after SaveChangesAsync()
                return Ok(new { message = "Transfer successful.", transactionId = _context.Transactions.OrderByDescending(t => t.TransactionId).FirstOrDefault()?.TransactionId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during fund transfer: {ex}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException}");
                }

                try
                {
                    if (databaseTransaction != null && databaseTransaction.GetDbTransaction().Connection != null)
                    {
                        await databaseTransaction.RollbackAsync();
                        Console.WriteLine("Database transaction rolled back.");
                    }
                }
                catch (Exception rollbackEx)
                {
                    Console.WriteLine($"Error attempting to rollback transaction: {rollbackEx.Message}");
                }

                return StatusCode(500, "An error occurred during transfer. Please try again.");
            }
        }
    }

    // This DTO defines the structure of the transaction data sent to the frontend
    public class TransactionDisplayDto
    {
        public int TransactionId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; } // Will be the absolute amount for display
        public string Description { get; set; } = null!; // The full description to be displayed
        public string Type { get; set; } = null!; // "Outgoing" or "Incoming" (for frontend's styling/logic)
        public string SenderName { get; set; } = null!;
        public string ReceiverName { get; set; } = null!;
    }

    // IMPORTANT: Make sure this DTO is defined somewhere in your Backend project,
    // preferably in a 'Models/DTOs' folder or similar, accessible by this controller.
    // I'm including it here for completeness of context, but ensure it's not a duplicate if it already exists.
    public class TransferRequestDto
    {
        [System.ComponentModel.DataAnnotations.Required] // Use full namespace to avoid ambiguity if 'using System.Linq;' creates a conflict
        public int SenderUserID { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        public int BeneficiaryID { get; set; }
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Amount must be greater than zero.")] // Cast decimal.MaxValue to double
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}