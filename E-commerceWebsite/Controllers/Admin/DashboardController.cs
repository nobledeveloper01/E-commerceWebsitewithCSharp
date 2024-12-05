using Microsoft.AspNetCore.Mvc;
using System.Linq;
using E_commerceWebsite.Data.Entities;
using E_commerceWebsite.Data;
using E_commerceWebsite.ViewModels;

namespace E_commerceWebsite.Controllers.Admin
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var totalSales = _context.Orders
                .Where(o => o.Status == "Completed")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            var totalOrders = _context.Orders.Count();
            var totalProducts = _context.Products.Count();
            var totalUsers = _context.Users.Count();

            var recentOrders = _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new OrderAdmin
                {
                    OrderId = o.OrderId ?? "N/A",
                    CustomerName = o.CustomerName ?? "Unknown",
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount
                })
                .ToList();

            var recentUsers = _context.Users
                .OrderByDescending(u => u.DateRegistered)
                .Take(10)
                .ToList();

            var recentProducts = _context.Products
                .OrderByDescending(p => p.Id)
                .Take(10)
                .Select(p => new AdminProduct
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                   
                })
                .ToList();

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

            return View(dashboardData);
        }
    }
}
