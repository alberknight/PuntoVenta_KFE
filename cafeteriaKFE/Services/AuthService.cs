using cafeteriaKFE.Models;
using cafeteriaKFE.Core; // For ResponseModel
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System; // For DateTime.UtcNow
using System.Collections.Generic;
using cafeteriaKFE.Models.Users; // For List<IdentityError>
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

        /*
        public async Task<ResponseModel> RegisterUser(
            RegisterResponse model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                Lastname = model.Lastname,
                PhoneNumber = model.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Deleted = false,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                return new ResponseModel { success = true, message = "User registered successfully.", data = user };
            }

            return new ResponseModel { success = false, message = "User registration failed.", data = result.Errors };
        }
        */

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
            return new ResponseModel {
                success = false,
                message = "Login failed."};
        }
    }
}
