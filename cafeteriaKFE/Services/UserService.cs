using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using cafeteriaKFE.Data;
using cafeteriaKFE.Models;
using Microsoft.AspNetCore.Identity;
using cafeteriaKFE.Core.Users.Response; // Add this using statement
using cafeteriaKFE.Core.Users.Request;


namespace cafeteriaKFE.Services
{
    public interface IUserService
    {
        Task<IEnumerable<GetUsersRequest>> GetAllUsers();
        Task<GetUsersDetailsRequest?> GetUserById(long id); // Changed return type to nullable
        Task<IdentityResult> CreateUser(CreateUserResponse request); // Changed return type
        Task<IdentityResult> UpdateUser(UpdateUserResponse request); // Changed return type
        Task<IdentityResult> DeleteUser(string id);
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

        public async Task<IEnumerable<GetUsersRequest>> GetAllUsers()
        {   
            return await _userManager.Users.Where(u => !u.Deleted).Select(u => new GetUsersRequest
            {
                userId = u.Id,
                email = u.Email,
                name = u.Name,
                lastName = u.LastName,
                createdAt = u.CreatedAt
            }).ToListAsync();
        }

        public async Task<GetUsersDetailsRequest?> GetUserById(long id) // Changed return type to nullable
        {
            return await _userManager.Users
        .Where(u => u.Id == id) // Added WHERE clause
        .Select(u => new GetUsersDetailsRequest{
            userId = u.Id, // Changed to long
            name = u.Name,
            lastname = u.LastName, // Assuming User.LastName is correct, will map to DTO's lastname
            email = u.Email,
            createdAt = u.CreatedAt,
            phoneNumber = u.PhoneNumber
        })
        .FirstOrDefaultAsync();
        }

        public async Task<IdentityResult> CreateUser(CreateUserResponse request) // Changed return type
        {
            var newUser = new User
            {
                UserName = request.name,
                Email = request.email,
                Name = request.name,
                LastName = request.lastname,
                PhoneNumber = request.phoneNumber,
                CreatedAt = System.DateTime.UtcNow,
                UpdatedAt = System.DateTime.UtcNow,
                Deleted = false,
                EmailConfirmed = true // Assuming email is confirmed on creation
                
            };
            return await _userManager.CreateAsync(newUser, request.password);
        }

        public async Task <IdentityResult> UpdateUser(UpdateUserResponse request) // Changed parameter type
        {
            var existingUser = await _userManager.FindByIdAsync(request.userId.ToString()); // Assuming UpdateUserResponse has an Id
            if (existingUser != null)
            {
                existingUser.Email = request.email;
                existingUser.Name = request.name;
                existingUser.LastName = request.lastName;
                existingUser.PhoneNumber = request.phoneNumber;
                existingUser.UpdatedAt = System.DateTime.UtcNow;
                await _userManager.UpdateAsync(existingUser);
                return IdentityResult.Success;
            }
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });
        }

        public async Task <IdentityResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Deleted = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                return IdentityResult.Success;
            }
            return IdentityResult.Failed(new IdentityError { Description = "Error al eliminar" });
        }

        public bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id.ToString() == id);
        }
    }
}
