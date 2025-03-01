using System.ComponentModel.DataAnnotations;

namespace E_commerceWebsite.ViewModels
{
    public class CreateAdminViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [MaxLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }
    }
}
