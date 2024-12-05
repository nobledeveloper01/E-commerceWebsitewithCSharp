using E_commerceWebsite.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using E_commerceWebsite.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace E_commerceWebsite.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUserService _userService;

        public AccountController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, IUserService userService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string username, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return View();
            }

            try
            {
                await _userService.RegisterAsync(fullName, username, email, password);
                TempData["Success"] = "Registration successful!";
                return RedirectToAction("Login");
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch
            {
                TempData["Error"] = "An unexpected error occurred.";
            }

            return View();
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Username and password are required.";
                return View();
            }

            try
            {
                // Assuming your user service handles login by username now
                var user = await _userService.LoginAsync(username, password);
                if (user == null)
                {
                    TempData["Error"] = "Invalid credentials.";
                    return View();
                }

                // Save user session data as claims
                await SetUserClaims(user);

                // Role-based redirect
                if (user.Role == "Admin")
                {
                    TempData["Success"] = "Login Successful.";
                    return RedirectToAction("Dashboard", "Product", new { area = "Admin" });
                }
                else if (user.Role == "User")
                {
                    TempData["Success"] = "Login Successful, Continue to shopping.";
                    return RedirectToAction("Index", "Product", new { area = "Customer" });
                }

                TempData["Error"] = "Unauthorized role.";
                return View();
            }
            catch
            {
                TempData["Error"] = "An error occurred during login.";
                return View();
            }
        }


        private async Task SetUserClaims(User user)
        {
            if (user != null)
            {
                // Create the list of claims to be used for authentication
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  // Unique user ID
            new Claim(ClaimTypes.Name, user.Username),               // Username
            new Claim(ClaimTypes.Role, user.Role)                    // User role (e.g., Admin, User)
        };

                // Create a ClaimsIdentity based on the claims
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Create the ClaimsPrincipal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Sign in the user and create the authentication cookie
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            }
        }


        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            // Add reset link logic here
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword() => View();

        [HttpPost]
        public IActionResult ResetPassword(string newPassword, string confirmPassword)
        {
            if (newPassword == confirmPassword)
            {
                // Save new password logic
                return RedirectToAction("Login");
            }
            ModelState.AddModelError("", "Passwords do not match.");
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult EditProfile()
        {
            return View();
        }
    }
}
