using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeesManagementSystem.Models
{
    public class StudentFee
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [Required]
        public int FeeHeadId { get; set; }

        [ForeignKey("FeeHeadId")]
        public FeeHead? FeeHead { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateTime? PaidDate { get; set; }
    }
}
