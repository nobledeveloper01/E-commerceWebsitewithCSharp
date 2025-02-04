using Microsoft.AspNetCore.Mvc;
using System.Linq;
using E_commerceWebsite.Data.Entities;
using E_commerceWebsite.Data;
using E_commerceWebsite.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace E_commerceWebsite.Controllers.Admin
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalSales = await _context.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var totalOrders = await _context.Orders.CountAsync();
            var totalProducts = await _context.AdminProducts.CountAsync();
            var totalUsers = await _context.Users.CountAsync();


            Console.WriteLine("totalorder", totalOrders);
            Console.WriteLine("totalsale", totalSales);
            Console.WriteLine("totalorder", totalProducts);
            Console.WriteLine("totalorder", totalUsers);


            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new OrderAdmin
                {
                    OrderId = o.OrderId ?? "N/A",
                    CustomerName = o.CustomerName ?? "Unknown",
                    OrderDate = o.OrderDate,
                    Status = o.Status ?? "Pending",
                    TotalAmount = o.TotalAmount
                })
                .ToListAsync();

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.DateRegistered)
                .Take(10)
                .Select(u => new User
                {
                    Id = u.Id,
                    FullName = u.FullName ?? "Unknown",
                    Email = u.Email ?? "N/A",
                    DateRegistered = u.DateRegistered,
                    Status = u.Status ?? "Inactive"
                })
                .ToListAsync();

            var recentProducts = await _context.AdminProducts
                .OrderByDescending(p => p.Id)
                .Take(10)
                .Select(p => new AdminProduct
                {
                    Id = p.Id,
                    Name = p.Name ?? "Unknown",
                    Price = p.Price,
                    Category = p.Category ?? "N/A"
                })
                .ToListAsync();

            var dashboardData = new DashboardViewModel
            {
                TotalSales = totalSales,
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalUsers = totalUsers,
                RecentOrders = recentOrders,
                RecentUsers = recentUsers,
                RecentProducts = recentProducts
            };

            return View("~/Views/Admin/Index.cshtml", dashboardData);
        }


    }
}
