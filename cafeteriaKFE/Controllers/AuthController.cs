using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using cafeteriaKFE.Models;
using cafeteriaKFE.Core; // For ResponseModel
using cafeteriaKFE.Services; // Add this for IAuthService
using Microsoft.AspNetCore.Authorization;
using cafeteriaKFE.Core.Users.Response; // For LoginResponse
using System.Collections.Generic;

namespace cafeteriaKFE.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IAuthService _authService;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IAuthService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authService = authService;
        }

        /*
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterResponse model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.RegisterUser(model);

                if (result.success)
                {
                    var user = (User)result.data;
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Product");
                }

                var errors = (IEnumerable<IdentityError>)result.data;
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }
        */

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Dashboard", "Home");
                }
                return RedirectToAction("Venta", "Pos");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginResponse model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var authResult = await _authService.LoginUser(model);

                if (authResult.success)
                {
                    if (authResult.data != null && await _userManager.IsInRoleAsync((User)authResult.data, "Admin"))
                    {
                        return LocalRedirect("/Home/Dashboard");
                    }
                    return LocalRedirect(returnUrl ?? "/Pos/Venta");
                }
                
            }
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
