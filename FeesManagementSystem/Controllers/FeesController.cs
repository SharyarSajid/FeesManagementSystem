using System;
using System.Threading.Tasks;
using FeesManagementSystem.Data;
using FeesManagementSystem.Models;
using FeesManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FeesManagementSystem.Controllers
{
    public class FeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFeeService _feeService;

        public FeesController(ApplicationDbContext context, IFeeService feeService)
        {
            _context = context;
            _feeService = feeService;
        }

        // GET: Fees/Assign
        public IActionResult Assign()
        {
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Name");
            ViewData["FeeHeadId"] = new SelectList(_context.FeeHeads, "Id", "Name");
            return View();
        }

        // POST: Fees/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int studentId, int feeHeadId, decimal amount, DateTime dueDate)
        {
            if (amount <= 0) ModelState.AddModelError("", "Amount must be greater than 0");
            
            if (ModelState.IsValid)
            {
                await _feeService.AssignFeeAsync(studentId, feeHeadId, amount, dueDate);
                return RedirectToAction(nameof(Index)); // or Redirect to Students List
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

        // POST: Fees/Pay/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id)
        {
            await _feeService.PayFeeAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Quick helper to seed FeeHeads if empty
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
