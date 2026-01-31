using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using cafeteriaKFE.Models;
using cafeteriaKFE.Data;
using Microsoft.AspNetCore.Authorization; // Assuming PosDbContext is here

namespace cafeteriaKFE.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost] // Re-added [AllowAnonymous] for POST action
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email, // Use email as username
                    Email = model.Email,
                    Name = model.Name,
                    Lastname = model.Lastname,
                    PhoneNumber = model.PhoneNumber, // Re-added PhoneNumber assignment
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Deleted = false,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign the default "Customer" role to new users
                    // This is a default action, adjust as needed
                    await _userManager.AddToRoleAsync(user, "Customer");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Product"); // Redirect to home or a dashboard
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null) // Changed to async Task<IActionResult>
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User); // Get the currently logged-in user
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Product");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return LocalRedirect("/Admin/Index");
                    }
                    return LocalRedirect(returnUrl ?? "/Product/Index"); // Redirect to intended URL or home
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Cuenta bloqueada.");
                }
                else
                {
                    // This error message will be displayed in the view's validation summary
                    ModelState.AddModelError(string.Empty, "Intento de inicio de sesión inválido.");
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost] // Re-added [AllowAnonymous] for POST action
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Product"); // Redirect to home or login page
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
