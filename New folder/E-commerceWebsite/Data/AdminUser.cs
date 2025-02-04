using E_commerceWebsite.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace E_commerceWebsite.Data
{
    public class AdminUser
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public DateTime DateRegistered { get; set; }

        // Navigation property for Customer (singular)
        public ICollection<CustomerEntity> Customers { get; set; }
    }
}



// The DashboardSummary class, representing the dashboard statistics for users
public class DashboardSummary
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int Admins { get; set; }
    }

    // The UserFilter class, used for filtering user queries based on status or search
    public class UserFilter
    {
        public string Status { get; set; }
        public string SearchQuery { get; set; }
    }

    // The PagedResponse class, used for paginating user data
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } 
        public int CurrentPage { get; set; } 
        public int TotalPages { get; set; }
        public int PageSize { get; set; } 
        public int TotalCount { get; set; }
    }

