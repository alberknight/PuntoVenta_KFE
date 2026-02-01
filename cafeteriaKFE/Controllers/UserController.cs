using Microsoft.AspNetCore.Mvc;
using cafeteriaKFE.Models;
using System.Threading.Tasks;
using cafeteriaKFE.Services;
using cafeteriaKFE.Core.Users.Request; // Added for request models
using cafeteriaKFE.Core.Users.Response;
using cafeteriaKFE.Models.Users; // Added for response models

namespace cafeteriaKFE.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: User
        public async Task<IActionResult> Index([FromQuery] GetUsersRequest? request = null) // Added request parameter
        {
            var users = await _userService.GetAllUsers(); // Assuming GetAllUsers will return all users for now
            return View(users);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View(new CreateUserResponse()); // Pass an empty CreateUserResponse to the view
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("email,password,name,lastname,phoneNumber")] CreateUserResponse request) // Changed to CreateUserResponse
        {
            if (ModelState.IsValid)
            {
                await _userService.CreateUser(request); // Use CreateUserResponse
                return RedirectToAction(nameof(Index));
            }
            return View(request);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            // Map User to UpdateUserResponse
            var updateUserResponse = new UpdateUserResponse
            {
                userId = (int)user.Id,
                email = user.Email,
                name = user.Name,
                lastname = user.Lastname,
                phoneNumber = user.PhoneNumber
            };
            return View(updateUserResponse);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("userId,email,name,lastname,phoneNumber")] UpdateUserResponse request) // Changed to UpdateUserResponse
        {
            if (id != request.userId.ToString()) // Compare with request.userId
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.UpdateUser(request); // Use UpdateUserResponse
                }
                catch (System.Exception)
                {
                    if (!UserExists(request.userId.ToString())) // Compare with request.userId
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(request);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _userService.DeleteUser(id);
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _userService.UserExists(id);
        }
    }
}
