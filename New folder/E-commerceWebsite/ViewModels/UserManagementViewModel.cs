using E_commerceWebsite.Data;

namespace E_commerceWebsite.ViewModels
{
    public class UserManagementViewModel
    {
        public List<User> Users { get; set; }
        public UserCounts UserCounts { get; set; }
        public string Status { get; set; }
        public string SearchTerm { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class UserCounts
    {
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int Admins { get; set; }
    }
}