using System;

namespace FeesManagementSystem.Models
{
    public class StudentAuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } // "Edit" or "Delete"
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string PerformedBy { get; set; } // Email of the user
        public DateTime Timestamp { get; set; }
        public string Changes { get; set; } // Details of what changed
    }
}
