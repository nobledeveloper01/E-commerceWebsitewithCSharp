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
                return RedirectToAction("Register");
            }

            try
            {
                await _userService.RegisterAsync(fullName, username, email, password);
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

            return RedirectToAction("Register");
        }


        [HttpGet]
        public IActionResult Login() => View();


        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Username and password are required.";
                return RedirectToAction("Login");
            }

            try
            {
                var user = await _userService.LoginAsync(username, password);
                if (user == null)
                {
                    TempData["Error"] = "Invalid credentials.";
                    return RedirectToAction("Login");
                }

                await SetUserClaims(user);

                
                if (user.Role == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (user.Role == "User")
                {
                    return RedirectToAction("Index", "Product", new { area = "Customer" });
                }

                TempData["Error"] = "Unauthorized role.";
                return RedirectToAction("Login");
            }
            catch
            {
                TempData["Error"] = "An error occurred during login.";
                return RedirectToAction("Login");
            }
        }



        private async Task SetUserClaims(User user)
        {
            if (user != null)
            {
                // Create the list of claims to be used for authentication
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),  
            new Claim(ClaimTypes.Name, user.Username),               
            new Claim(ClaimTypes.Role, user.Role)                    
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

        public async Task<IActionResult> LogOut()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Pass a logout message via TempData
                TempData["SwalMessage"] = "You have been logged out successfully!";
                return RedirectToAction("Login");
            }
            catch
            {
                TempData["SwalMessage"] = "An error occurred during logout.";
                return RedirectToAction("Login");
            }
        }


        public IActionResult EditProfile()
        {
            return View();
        }
    }
}
