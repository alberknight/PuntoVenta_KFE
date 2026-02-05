using cafeteriaKFE.Models;
using cafeteriaKFE.Core; // For ResponseModel
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System; // For DateTime.UtcNow
using System.Collections.Generic;
using cafeteriaKFE.Core.Users.Response; // For LoginResponse and RegisterResponse
// Assuming RegisterResponse and LoginResponse are in cafeteriaKFE.Models, so cafeteriaKFE.Data might not be needed here.
// using cafeteriaKFE.Data; // Check if this is truly needed.

namespace cafeteriaKFE.Services
{
    public interface IAuthService
    {
        // Task<ResponseModel> RegisterUser(RegisterResponse model);
        Task<ResponseModel> LoginUser(LoginResponse model);
    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<ResponseModel> LoginUser(LoginResponse model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                return new ResponseModel {
                    success = true,
                    message = "Login successful.",
                    data = user };
            }
            else
            {
                string errorMessage;
                if (result.IsLockedOut)
                {
                    errorMessage = "User account locked out.";
                }
                else if (result.IsNotAllowed)
                {
                    errorMessage = "User not allowed to sign in (e.g., email not confirmed or disabled).";
                }
                else if (result.RequiresTwoFactor)
                {
                    errorMessage = "Two-factor authentication required.";
                }
                else
                {
                    errorMessage = "Invalid login attempt. Please check your email and password.";
                }

                return new ResponseModel {
                    success = false,
                    message = errorMessage
                };
            }
        }
    }
}
