using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using cafeteriaKFE.Data;
using cafeteriaKFE.Models;
using Microsoft.AspNetCore.Identity;
using cafeteriaKFE.Core.Users.Response; // Add this using statement
using cafeteriaKFE.Core.Users.Request;
using cafeteriaKFE.Models.Users; // Add this using statement

namespace cafeteriaKFE.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsers();
        Task<User?> GetUserById(string id); // Changed return type to nullable
        Task CreateUser(CreateUserResponse request); // Changed parameter type
        Task UpdateUser(UpdateUserResponse request); // Changed parameter type
        Task DeleteUser(string id);
        bool UserExists(string id);
    }
    public class UserService : IUserService
    {
        private readonly PosDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserService(PosDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<User?> GetUserById(string id) // Changed return type to nullable
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task CreateUser(CreateUserResponse request) // Changed parameter type
        {
            var newUser = new User
            {
                UserName = request.email,
                Email = request.email,
                Name = request.name,
                Lastname = request.lastname,
                PhoneNumber = request.phoneNumber,
                CreatedAt = System.DateTime.UtcNow,
                UpdatedAt = System.DateTime.UtcNow,
                Deleted = false,
                EmailConfirmed = true // Assuming email is confirmed on creation
            };
            await _userManager.CreateAsync(newUser, request.password);
        }

        public async Task UpdateUser(UpdateUserResponse request) // Changed parameter type
        {
            var existingUser = await _userManager.FindByIdAsync(request.userId.ToString()); // Assuming UpdateUserResponse has an Id
            if (existingUser != null)
            {
                existingUser.Email = request.email;
                existingUser.Name = request.name;
                existingUser.Lastname = request.lastname;
                existingUser.PhoneNumber = request.phoneNumber;
                existingUser.UpdatedAt = System.DateTime.UtcNow;
                await _userManager.UpdateAsync(existingUser);
            }
        }

        public async Task DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }

        public bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id.ToString() == id);
        }
    }
}
