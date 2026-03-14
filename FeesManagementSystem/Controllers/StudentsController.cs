using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using FeesManagementSystem.Data;
using FeesManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace FeesManagementSystem.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public StudentsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            return View(await _context.Students.Where(s => !s.IsDeleted).ToListAsync());
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RegistrationNo,Name,FatherName,DateOfBirth,MobileNo,Email,Gender,Department,Course,Photo,City,Address,AdmissionDate")] Student student)
        {
            if (ModelState.IsValid)
            {
                if (student.Photo != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(student.Photo.FileName);
                    string extension = Path.GetExtension(student.Photo.FileName);
                    student.PhotoPath = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    string path = Path.Combine(wwwRootPath + "/images/", fileName);
                    
                    // Create directory if it doesn't exist (though wwwroot/images should typically rely on standard structure, we ensure it exists)
                    Directory.CreateDirectory(Path.Combine(wwwRootPath, "images"));

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await student.Photo.CopyToAsync(fileStream);
                    }
                }

                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Details/5
        [Authorize(Roles = "Supervisor")]
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

        // GET: Students/Edit/5
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RegistrationNo,Name,FatherName,DateOfBirth,MobileNo,Email,Gender,Department,Course,Photo,PhotoPath,City,Address,AdmissionDate")] Student student)
        {
            if (id != student.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (student.Photo != null)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Path.GetFileNameWithoutExtension(student.Photo.FileName);
                        string extension = Path.GetExtension(student.Photo.FileName);
                        string newFileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath + "/images/", newFileName);

                        // Delete old image if it exists and is not null
                        if (!string.IsNullOrEmpty(student.PhotoPath))
                        {
                            var oldPath = Path.Combine(wwwRootPath, "images", student.PhotoPath);
                            if (System.IO.File.Exists(oldPath))
                            {
                                
                                System.IO.File.Delete(oldPath);
                            }
                        }

                         // Create directory if it doesn't exist
                        Directory.CreateDirectory(Path.Combine(wwwRootPath, "images"));

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await student.Photo.CopyToAsync(fileStream);
                        }
                        
                        student.PhotoPath = newFileName;
                    }
                    
                    
                    
                    // Audit Log
                    var originalStudent = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                    var changes = new List<string>();

                    if (originalStudent != null)
                    {
                        if (originalStudent.RegistrationNo != student.RegistrationNo) changes.Add($"RegistrationNo: '{originalStudent.RegistrationNo}' -> '{student.RegistrationNo}'");
                        if (originalStudent.Name != student.Name) changes.Add($"Name: '{originalStudent.Name}' -> '{student.Name}'");
                        if (originalStudent.FatherName != student.FatherName) changes.Add($"FatherName: '{originalStudent.FatherName}' -> '{student.FatherName}'");
                        if (originalStudent.DateOfBirth != student.DateOfBirth) changes.Add($"DateOfBirth: '{originalStudent.DateOfBirth.ToShortDateString()}' -> '{student.DateOfBirth.ToShortDateString()}'");
                        if (originalStudent.MobileNo != student.MobileNo) changes.Add($"MobileNo: '{originalStudent.MobileNo}' -> '{student.MobileNo}'");
                        if (originalStudent.Email != student.Email) changes.Add($"Email: '{originalStudent.Email}' -> '{student.Email}'");
                        if (originalStudent.Gender != student.Gender) changes.Add($"Gender: '{originalStudent.Gender}' -> '{student.Gender}'");
                        if (originalStudent.Department != student.Department) changes.Add($"Department: '{originalStudent.Department}' -> '{student.Department}'");
                        if (originalStudent.Course != student.Course) changes.Add($"Course: '{originalStudent.Course}' -> '{student.Course}'");
                        if (originalStudent.City != student.City) changes.Add($"City: '{originalStudent.City}' -> '{student.City}'");
                        if (originalStudent.Address != student.Address) changes.Add($"Address: '{originalStudent.Address}' -> '{student.Address}'");
                        if (originalStudent.AdmissionDate != student.AdmissionDate) changes.Add($"AdmissionDate: '{originalStudent.AdmissionDate.ToShortDateString()}' -> '{student.AdmissionDate.ToShortDateString()}'");
                    }

                    var auditLog = new StudentAuditLog
                    {
                        Action = "Edit",
                        StudentId = student.Id,
                        StudentName = student.Name,
                        PerformedBy = User.Identity?.Name ?? "Unknown",
                        Timestamp = DateTime.Now,
                        Changes = string.Join(", ", changes)
                    };
                    _context.StudentAuditLogs.Add(auditLog);

                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Delete/5
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                // Soft Delete: Mark as deleted instead of removing
                student.IsDeleted = true;

                // Audit Log
                var auditLog = new StudentAuditLog
                {
                    Action = "Soft Delete",
                    StudentId = student.Id,
                    StudentName = student.Name,
                    PerformedBy = User.Identity?.Name ?? "Unknown",
                    Timestamp = DateTime.Now,
                    Changes = $"Soft Deleted Student: Name={student.Name}, FatherName={student.FatherName}, Email={student.Email}"
                };
                _context.StudentAuditLogs.Add(auditLog);

                _context.Update(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
