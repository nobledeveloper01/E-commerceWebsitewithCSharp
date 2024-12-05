using System.Collections.Generic;
using E_commerceWebsite.Data;
using E_commerceWebsite.Data.Entities;

namespace E_commerceWebsite.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public List<OrderAdmin> RecentOrders { get; set; }
        public List<User> RecentUsers { get; set; }
        public List<AdminProduct> RecentProducts { get; set; }
    }
}
