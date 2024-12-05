using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_commerceWebsite.Data
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [NotMapped]
        public string Password { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

        [Required]
        public string Role { get; set; } = "User";

        public string Status { get; set; } = "Active"; 

        public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
    }
}
