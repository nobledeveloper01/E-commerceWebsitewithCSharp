using E_commerceWebsite.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace E_commerceWebsite.Data
{
    public class ContactForm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
