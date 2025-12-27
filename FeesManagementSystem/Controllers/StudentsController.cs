using System.Linq;
using System.Threading.Tasks;
using FeesManagementSystem.Data;
using FeesManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeesManagementSystem.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            return View(await _context.Students.ToListAsync());
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,PhoneNumber,Course,AdmissionDate")] Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (student == null) return NotFound();

            // Load fees for this student
            var fees = await _context.StudentFees
                .Include(f => f.FeeHead)
                .Where(f => f.StudentId == id)
                .OrderByDescending(f => f.DueDate)
                .ToListAsync();

            ViewBag.Fees = fees;

            return View(student);
        }
    }
}
