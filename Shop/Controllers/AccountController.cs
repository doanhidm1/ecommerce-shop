using Application;
using Application.Accounts;
using Domain.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Shop.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public AccountController
        (
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "AdminProduct");
            }
            return View();
        }

        private async Task<IdentityUser?> FindUser(string input)
        {
            return await _userManager.FindByEmailAsync(input) ?? await _userManager.FindByNameAsync(input);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await FindUser(model.EmailOrUsername);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return View(model);
            }
            if (await _userManager.IsLockedOutAsync(user))
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User is locked out!"));
                return View(model);
            }
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"User {user.UserName} logged in as {string.Join(", ", roles)}"));
                return RedirectToAction("Index", "AdminProduct");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User password not correct!"));
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Roles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles ?? new List<IdentityRole>());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult CreateRole()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role not exist!"));
                return RedirectToAction("Roles");
            }
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Deleted role {role.Name} successfully"));
                return RedirectToAction("Roles");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Delete role failed!"));
            return RedirectToAction("Roles");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            bool isRoleExisted = await _roleManager.RoleExistsAsync(model.RoleName);
            if (isRoleExisted)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role name existed!"));
                return View(model);
            }
            var role = new IdentityRole
            {
                Name = model.RoleName,
                NormalizedName = model.NormalizedName ?? model.RoleName.ToUpper(),
                ConcurrencyStamp = model.ConcurrencyStamp ?? Guid.NewGuid().ToString()
            };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Created role {role.Name} successfully"));
                return RedirectToAction("Roles");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Create role failed!"));
            return View(model);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users ?? new List<IdentityUser>());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles ?? new List<IdentityRole>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("CreateUser");
            }
            // check if username or email existed
            var checkUser = await _userManager.FindByNameAsync(model.UserName);
            if (checkUser != null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "This username has been used!"));
                return RedirectToAction("CreateUser");
            }

            checkUser = await _userManager.FindByEmailAsync(model.Email);
            if (checkUser != null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "This email has been used!"));
                return RedirectToAction("CreateUser");
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = new IdentityUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };
                var userResult = await _userManager.CreateAsync(user, model.Password);
                if (!userResult.Succeeded)
                {
                    throw new Exception("Create user failed!");
                }
                await _unitOfWork.SaveChangesAsync();
                if (model.RoleIds != null)
                {
                    foreach (var roleId in model.RoleIds)
                    {
                        var role = await _roleManager.FindByIdAsync(roleId);
                        if (role == null)
                        {
                            throw new Exception("Role not exist!");
                        }
                        var roleResult = await _userManager.AddToRoleAsync(user, role.Name);
                        if (!roleResult.Succeeded)
                        {
                            throw new Exception("Add role to user failed!");
                        }
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
                await transaction.CommitAsync();
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Created user {user.UserName} successfully"));
                return RedirectToAction("Users");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Create user failed!"));
                return RedirectToAction("CreateUser");
            }
        }

        //private string GetErrorMessage(IdentityResult result)
        //{
        //    if (result.Errors.Any())
        //    {
        //        var errorMessage = string.Join(" ", result.Errors.Select(x => x.Description).ToList());
        //        return errorMessage;
        //    }
        //    return string.Empty;
        //}
    }
}