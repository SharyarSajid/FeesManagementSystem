using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeesManagementSystem.Data;
using FeesManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FeesManagementSystem.Services
{
    public class FeeService : IFeeService
    {
        private readonly ApplicationDbContext _context;

        public FeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudentFee>> GetOverdueFeesAsync()
        {
            var today = DateTime.Today;
            // Fetch unpaid fees where due date is passed
            return await _context.StudentFees
                .Include(f => f.Student)
                .Include(f => f.FeeHead)
                .Where(f => !f.IsPaid && f.DueDate < today)
                .ToListAsync();
        }

        public async Task AssignFeeAsync(int studentId, int feeHeadId, decimal amount, DateTime dueDate)
        {
            var fee = new StudentFee
            {
                StudentId = studentId,
                FeeHeadId = feeHeadId,
                Amount = amount,
                DueDate = dueDate,
                IsPaid = false
            };
            _context.StudentFees.Add(fee);
            await _context.SaveChangesAsync();
        }

        public async Task PayFeeAsync(int studentFeeId)
        {
            var fee = await _context.StudentFees.FindAsync(studentFeeId);
            if (fee != null)
            {
                fee.IsPaid = true;
                fee.PaidDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
