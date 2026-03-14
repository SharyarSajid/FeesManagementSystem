using System.ComponentModel.DataAnnotations;

namespace FeesManagementSystem.Models
{
    public class VerifyOtpViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(4, ErrorMessage = "The OTP must be 4 digits.", MinimumLength = 4)]
        public string Otp { get; set; }
    }
}
