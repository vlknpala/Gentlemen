using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Gentlemen.Data;
using Gentlemen.Models;

namespace Gentlemen.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!model.AcceptTerms)
                {
                    ModelState.AddModelError("AcceptTerms", "Kullanım koşullarını kabul etmelisiniz.");
                    return View(model);
                }

                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                    return View(model);
                }

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Password = HashPassword(model.Password),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kayıt başarıyla tamamlandı. Şimdi giriş yapabilirsiniz.";
                return RedirectToAction(nameof(Login));
            }

            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hashedPassword = HashPassword(model.Password);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == hashedPassword);

                if (user != null)
                {
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Geçersiz e-posta veya şifre");
            }

            return View(model);
        }

        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync();
            if (!result.Succeeded)
                return RedirectToAction("Login");

            var claimsIdentity = result.Principal.Identities.FirstOrDefault();
            if (claimsIdentity == null)
                return RedirectToAction("Login");

            var claims = claimsIdentity.Claims.Select(claim => new
            {
                claim.Type,
                claim.Value
            });

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
            var fullName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "";
            var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? fullName.Split(' ').FirstOrDefault() ?? "";
            var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? fullName.Split(' ').LastOrDefault() ?? "";

            // Check if user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Name = fullName,
                    PhoneNumber = "", // Google doesn't provide phone number
                    Password = "", // No password for Google auth
                    CreatedAt = DateTime.Now,
                    RegisterDate = DateTime.Now,
                    IsActive = true
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Set session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.Name ?? "");

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 