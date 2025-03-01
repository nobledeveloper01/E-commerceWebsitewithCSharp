using E_commerceWebsite.Data;
using E_commerceWebsite.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using E_commerceWebsite.ViewModels;
using System;
using Microsoft.AspNetCore.Authorization;

namespace E_commerceWebsite.Controllers.Customer
{
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SupportController> _logger;

        public SupportController(ApplicationDbContext context, ILogger<SupportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Display the contact form
        [Authorize]
        [HttpGet]
       public IActionResult Contact()
        {
            return View("~/Views/Customer/Contact.cshtml");
        }

        // POST: Handle form submission
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Save to database
                var message = new ContactMessage
                {
                    Name = model.Name,
                    Email = model.Email,
                    Subject = model.Subject,
                    Message = model.Message,
                    SubmittedAt = DateTime.UtcNow
                };

                _context.ContactMessages.Add(message);
                _context.SaveChanges();

                TempData["Success"] = "Your message has been sent. We'll get back to you shortly.";
                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving contact message.");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request.");
                return View(model);
            }
        }

        // Optional: Confirmation page
        [Authorize]
        public IActionResult Confirmation()
        {
            return View();
        }
    }
}
