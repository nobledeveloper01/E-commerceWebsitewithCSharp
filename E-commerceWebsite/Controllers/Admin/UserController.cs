using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_commerceWebsite.Data;
using E_commerceWebsite.Models;
using E_commerceWebsite.ViewModels;

namespace E_commerceWebsite.Controllers.Admin
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Method to manage users with pagination and search
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
            return View("~/Views/Admin/UserManagement.cshtml",model);
        }

        private UserCounts MapToUserCounts(UserCountsViewModel viewModel)
        {
            return new UserCounts
            {
                ActiveUsers = viewModel.ActiveUsers,
                InactiveUsers = viewModel.InactiveUsers,
                Admins = viewModel.Admins
            };
        }
    }
}
