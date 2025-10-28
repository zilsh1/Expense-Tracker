using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; } // ⚠️ Hash in production

        // Profile image filename
        public string? ProfileImage { get; set; } = "profile.jpg";
    }
}
