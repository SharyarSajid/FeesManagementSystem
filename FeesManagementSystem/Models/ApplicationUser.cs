using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FeesManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        public string? OtpCode { get; set; }
        public DateTime? OtpExpiration { get; set; }
    }
}
