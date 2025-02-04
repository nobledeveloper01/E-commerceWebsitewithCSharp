using System.ComponentModel.DataAnnotations;

namespace E_commerceWebsite.ViewModels
{
    public class ContactFormViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Subject must not exceed 200 characters.")]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Message must not exceed 1000 characters.")]
        public string Message { get; set; }
    }
}
