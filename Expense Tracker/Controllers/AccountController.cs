using Microsoft.AspNetCore.Mvc;
using Expense_Tracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AccountController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ===========================
        // GET: /Account/Login
        // ===========================
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            if (user == null)
            {
                ViewBag.Error = "Invalid Email or Password";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserProfileImage", user.ProfileImage ?? "profile.jpg");

            return RedirectToAction("Index", "Dashboard");
        }

        // ===========================
        // GET: /Account/Register
        // ===========================
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Name,Email,Password")] User user, IFormFile ProfileImageFile)
        {
            if (!ModelState.IsValid)
                return View(user);

            // Check if email already exists
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existing != null)
            {
                ViewBag.Error = "Email already registered.";
                return View(user);
            }

            // Handle Profile Image Upload
            if (ProfileImageFile != null && ProfileImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = $"{Guid.NewGuid()}_{ProfileImageFile.FileName}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImageFile.CopyToAsync(fileStream);
                }

                user.ProfileImage = fileName;
            }

            _context.Add(user);
            await _context.SaveChangesAsync();

            // Set session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserProfileImage", user.ProfileImage ?? "profile.jpg");

            return RedirectToAction("Index", "Dashboard");
        }

        // ===========================
        // GET: /Account/Logout
        // ===========================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ===========================
        // GET: /Account/Profile
        // ===========================
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile([Bind("UserId,Name,Email,Password")] User user, IFormFile ProfileImageFile)
        {
            if (!ModelState.IsValid)
                return View(user);

            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser == null)
                return NotFound();

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Password = user.Password;

            if (ProfileImageFile != null && ProfileImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = $"{existingUser.UserId}_{ProfileImageFile.FileName}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImageFile.CopyToAsync(fileStream);
                }

                existingUser.ProfileImage = fileName;
            }

            _context.Update(existingUser);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UserName", existingUser.Name);
            HttpContext.Session.SetString("UserProfileImage", existingUser.ProfileImage ?? "profile.jpg");

            ViewBag.Message = "Profile updated successfully!";
            return View(existingUser);
        }
    }
}
