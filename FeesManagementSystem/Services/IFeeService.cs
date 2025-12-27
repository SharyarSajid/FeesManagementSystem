using System.Collections.Generic;
using System.Threading.Tasks;
using FeesManagementSystem.Models;

namespace FeesManagementSystem.Services
{
    public interface IFeeService
    {
        Task<List<StudentFee>> GetOverdueFeesAsync();
        Task AssignFeeAsync(int studentId, int feeHeadId, decimal amount, DateTime dueDate);
        Task PayFeeAsync(int studentFeeId);
    }
}
