using System;
using System.ComponentModel.DataAnnotations;

namespace FeesManagementSystem.Models
{
    public class NotificationLog
    {
        public int Id { get; set; }

        [Required]
        public string Recipient { get; set; } = string.Empty; // Email or Phone

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = "Email"; // Email or SMS

        public DateTime SentAt { get; set; } = DateTime.Now;

        public bool Success { get; set; }
        
        public string? ErrorMessage { get; set; }
    }
}
