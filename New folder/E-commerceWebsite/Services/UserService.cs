using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using E_commerceWebsite.Data;

namespace E_commerceWebsite.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> RegisterAsync(string fullName, string username, string email, string password, string role = "User")
        {
            if (await _context.Users.AnyAsync(u => u.Email == email || u.Username == username))
                throw new ArgumentException("Email or Username already exists.");

            if (password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters long.");

            var user = new User
            {
                FullName = fullName,
                Username = username,
                Email = email,
                Role = role,
                PasswordHash = _passwordHasher.HashPassword(null, password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            // Find user by username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                Console.WriteLine("User not found.");
                return null; // Return early if user is not found
            }

            // Verify password
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            Console.WriteLine($"Password verification result: {result}");

            // Return user if password is valid
            return result == PasswordVerificationResult.Success ? user : null;
        }



        public async Task<string?> GenerateResetTokenAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();
            return user.ResetToken;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpires > DateTime.UtcNow);
            if (user == null) return false;

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
