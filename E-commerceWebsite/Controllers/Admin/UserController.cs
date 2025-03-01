using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_commerceWebsite.Data;
using E_commerceWebsite.Models;
using E_commerceWebsite.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace E_commerceWebsite.Controllers.Admin
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // Method to manage users with pagination and search
        [Authorize]
        public async Task<IActionResult> UserManagement(int page = 1, string status = "", string searchTerm = "")
        {
            const int pageSize = 10; // Number of users per page

            var query = _context.Users.AsQueryable();

            // Filter by status if specified
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(u => u.Status == status);
            }

            // Filter by search term if specified
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.FullName.Contains(searchTerm) || u.Email.Contains(searchTerm));
            }

            // Get the total number of users matching the query
            var totalUsers = await query.CountAsync();

            // Calculate the total number of pages
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            // Fetch the current page of users
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // In your action method
            var userCountsViewModel = new UserCountsViewModel
            {
                ActiveUsers = await _context.Users.CountAsync(u => u.Status == "Active"),
                InactiveUsers = await _context.Users.CountAsync(u => u.Status == "Inactive"),
                Admins = await _context.Users.CountAsync(u => u.Role == "Admin")
            };

            // Prepare the model to be passed to the view
            var model = new UserManagementViewModel
            {
                Users = users,
                UserCounts = MapToUserCounts(userCountsViewModel), // Mapping from UserCountsViewModel to UserCounts
                CurrentPage = page,
                TotalPages = totalPages,
                Status = status,
                SearchTerm = searchTerm
            };

            // Return the view with the model
            return View("~/Views/Admin/UserManagement.cshtml", model);
        }

        [Authorize]
        private UserCounts MapToUserCounts(UserCountsViewModel viewModel)
        {
            return new UserCounts
            {
                ActiveUsers = viewModel.ActiveUsers,
                InactiveUsers = viewModel.InactiveUsers,
                Admins = viewModel.Admins
            };
        }
        [Authorize]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            // Fetch user from the database
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Deactivate the user and save changes
            user.Status = "Inactive";
            await _context.SaveChangesAsync();

            // Redirect back to the UserManagement page
            return RedirectToAction("UserManagement");
        }
        [Authorize]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Activate the user and save changes
            user.Status = "Active";
            await _context.SaveChangesAsync();

            // Redirect back to the UserManagement page
            return RedirectToAction("UserManagement");
        }

        [Authorize]
        public async Task<IActionResult> UserComplains()
        {
            var contactMessages = await _context.ContactMessages.ToListAsync();
            return View("~/Views/Admin/UserComplains.cshtml", contactMessages);
        }
        [Authorize]
        public async Task<IActionResult> UserContactUs()
        {
            var contactMessages = await _context.ContactForms.ToListAsync();
            return View("~/Views/Admin/UserContactUs.cshtml", contactMessages);
        }
        [Authorize]
        public async Task<IActionResult> UserProductComplains()
        {
            var contactMessages = await _context.ComplaintForms.ToListAsync();
            return View("~/Views/Admin/UserProductComplain.cshtml", contactMessages);
        }

        [Authorize]
        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateNewAdmin([FromBody] UserManagementViewModel model)
        {
            if (model?.CreateAdmin == null)
            {
                return BadRequest(new { message = "Invalid data submitted." });
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.CreateAdmin.Username);
            if (existingUser != null)
            {
                return BadRequest(new { message = "An account with this email already exists." });
            }

            var newAdmin = new User
            {
                FullName = model.CreateAdmin.FullName,
                Username = model.CreateAdmin.Username,
                Email = model.CreateAdmin.Username,
                Role = "Admin",
                Status = "Active",
                DateRegistered = DateTime.UtcNow
            };

            newAdmin.PasswordHash = _passwordHasher.HashPassword(newAdmin, model.CreateAdmin.Password);

            _context.Users.Add(newAdmin);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin account created successfully!" });
        }



    }
}
