using System.ComponentModel.DataAnnotations;

namespace FeesManagementSystem.Models
{
    public class FeeHead
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., Tuition, Lab, Sports

        public string? Description { get; set; }
    }
}
