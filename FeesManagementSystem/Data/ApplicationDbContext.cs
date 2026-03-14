using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FeesManagementSystem.Models;

namespace FeesManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<FeeHead> FeeHeads { get; set; }
        public DbSet<StudentFee> StudentFees { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }
        public DbSet<StudentAuditLog> StudentAuditLogs { get; set; }
    }
}
