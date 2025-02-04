using System.Threading.Tasks;
using E_commerceWebsite.Data;

namespace E_commerceWebsite.Services
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(string fullName, string username, string email, string password, string role = "User");
        Task<User?> LoginAsync(string email, string password);
        Task<string?> GenerateResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}
