using System;
using System.ComponentModel.DataAnnotations;

namespace FeesManagementSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string RegistrationNo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FatherName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Phone]
        public string MobileNo { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        public string Course { get; set; } = string.Empty;

        public string PhotoPath { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Microsoft.AspNetCore.Http.IFormFile? Photo { get; set; }

        public string City { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public DateTime AdmissionDate { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false;


    }
}
