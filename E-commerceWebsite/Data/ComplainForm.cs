using E_commerceWebsite.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace E_commerceWebsite.Data
{


    public class ComplaintForm
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ComplaintDescription { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
