using System;
using System.ComponentModel.DataAnnotations;

namespace FeesManagementSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public string Course { get; set; } = string.Empty;

        public DateTime AdmissionDate { get; set; } = DateTime.Now;
    }
}
