using FeesManagementSystem.Data;
using FeesManagementSystem.Models;
using FeesManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace FeesManagementSystem.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class FeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFeeService _feeService;
        private readonly INotificationService _notificationService; // Step 1: Add this
        private readonly ILogger<FeesController> _logger; // Added logger

        // Constructor update karein notification service inject karne ke liye
        public FeesController(ApplicationDbContext context, IFeeService feeService, INotificationService notificationService, ILogger<FeesController> logger)
        {
            _context = context;
            _feeService = feeService;
            _notificationService = notificationService;
            _logger = logger;
        }

        // GET: Fees/Assign
        public async Task<IActionResult> Assign()
        {
            if (!await _context.FeeHeads.AnyAsync())
            {
                _context.FeeHeads.AddRange(
                    new FeeHead { Name = "Tuition Fee", Description = "Monthly Tuition" },
                    new FeeHead { Name = "Exam Fee", Description = "Semester Exam Fee" },
                    new FeeHead { Name = "Library Fee", Description = "Yearly Library Fee" }
                );
                await _context.SaveChangesAsync();
            }

            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Name");
            ViewData["FeeHeadId"] = new SelectList(_context.FeeHeads, "Id", "Name");
            
            // Pass simple projection for JS lookup: RegNo -> Id
            var studentLookup = await _context.Students
                .Select(s => new { s.Id, s.RegistrationNo, s.Name })
                .ToListAsync();
            ViewBag.StudentLookup = System.Text.Json.JsonSerializer.Serialize(studentLookup);

            return View();
        }

        // POST: Fees/Assign (No changes needed)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int studentId, int feeHeadId, decimal amount, DateTime dueDate)
        {
            if (amount <= 0) ModelState.AddModelError("", "Amount must be greater than 0");

            if (ModelState.IsValid)
            {
                await _feeService.AssignFeeAsync(studentId, feeHeadId, amount, dueDate);
                return RedirectToAction(nameof(Index));
            }
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Name", studentId);
            ViewData["FeeHeadId"] = new SelectList(_context.FeeHeads, "Id", "Name", feeHeadId);
            return View();
        }

        // GET: Fees/Unpaid (Index)
        public async Task<IActionResult> Index()
        {
            var fees = await _context.StudentFees
                .Include(f => f.Student)
                .Include(f => f.FeeHead)
                .Where(f => !f.IsPaid)
                .OrderBy(f => f.DueDate)
                .ToListAsync();
            return View(fees);
        }

        // GET: Fees/Print/5
        public async Task<IActionResult> Print(int id)
        {
            var fee = await _context.StudentFees
                .Include(f => f.Student)
                .Include(f => f.FeeHead)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fee == null)
            {
                return NotFound();
            }

            return View(fee);
        }

        // POST: Fees/Pay/5 - Step 2: Email logic added here
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id)
        {
            // 1. Fee ka status update karein service ke zariye
            await _feeService.PayFeeAsync(id);

            // 2. Student aur Fee ki details nikalain email ke liye
            var paidFee = await _context.StudentFees
                .Include(f => f.Student)
                .Include(f => f.FeeHead)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (paidFee != null && paidFee.Student != null && !string.IsNullOrEmpty(paidFee.Student.Email))
            {
                string subject = "Fee Payment Confirmation";
                string body = $@"
                    <h3>Payment Successful!</h3>
                    Dear {paidFee.Student.Name},<br/><br/>
                    We have received your payment of <b>Rs {paidFee.Amount}</b> for <b>{paidFee.FeeHead?.Name}</b>.<br/>
                    <b>Payment Date:</b> {DateTime.Now.ToString("dd MMM yyyy")}<br/><br/>
                    Thank you for your payment!";

                try
                {
                    await _notificationService.SendEmailAsync(paidFee.Student.Email, subject, body);
                    _logger.LogInformation($"Confirmation email sent to {paidFee.Student.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Could not send confirmation email: {ex.Message}");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // Quick helper to seed FeeHeads (No changes needed)
        public async Task<IActionResult> SeedFeeHeads()
        {
            if (!await _context.FeeHeads.AnyAsync())
            {
                _context.FeeHeads.AddRange(
                    new FeeHead { Name = "Tuition Fee", Description = "Monthly Tuition" },
                    new FeeHead { Name = "Exam Fee", Description = "Semester Exam Fee" },
                    new FeeHead { Name = "Library Fee", Description = "Yearly Library Fee" }
                );
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Assign));
        }
    }
}