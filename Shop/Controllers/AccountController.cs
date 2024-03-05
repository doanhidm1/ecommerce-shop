﻿using Application;
using Application.Accounts;
using Domain.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Shop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController
        (
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        private const string ImageFolder = "AccountAvatars";

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "AdminProduct");
            }
            return View();
        }

        private async Task<User?> FindUser(string input)
        {
            return await _userManager.FindByEmailAsync(input) ?? await _userManager.FindByNameAsync(input);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [AllowAnonymous]
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

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Access denied!"));
            return RedirectToAction("Logout");
        }

        public async Task<IActionResult> Roles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles ?? new List<IdentityRole>());
        }

        public IActionResult CreateRole()
        {
            return View();
        }

        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role not exist!"));
                return RedirectToAction("Roles");
            }
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name) ?? new List<User>();
            foreach (var user in usersInRole)
            {
                await _userManager.UpdateSecurityStampAsync(user);
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

        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Users");
            }
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "You can't delete yourself!"));
                return RedirectToAction("Users");
            }
            var result =  await _userManager.UpdateSecurityStampAsync(user);
            if (!result.Succeeded)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Update security stamp failed!"));
                return RedirectToAction("Users");
            }
            DeleteImage(user.AvatarUrl);
            result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Deleted user {user.UserName} successfully"));
                return RedirectToAction("Users");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Delete user failed!"));
            return RedirectToAction("Users");
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
                Name = model.RoleName
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
            return View(users ?? new List<User>());
        }

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
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };
                user.AvatarUrl = await SaveImage(model.Avatar);
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

        private async Task<string> SaveImage(IFormFile image)
        {
            string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, ShopConstants.UploadFolder);
            string productImageDir = Path.Combine(uploadFolder, ImageFolder);
            if (!Directory.Exists(productImageDir))
            {
                Directory.CreateDirectory(productImageDir);
            }
            string fileName = $"{Guid.NewGuid()}-{image.FileName}";
            string fileUrl = $"/{ShopConstants.UploadFolder}/{ImageFolder}/{fileName}";
            using var stream = new FileStream(Path.Combine(productImageDir, fileName), FileMode.Create);
            await image.CopyToAsync(stream);
            return fileUrl;
        }

        private void DeleteImage(string? image)
        {
            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, ShopConstants.UploadFolder);
            var productImageDir = Path.Combine(uploadFolder, ImageFolder);
            if (image == null) return;
            var fileName = image.Split('/').Last();
            var filePath = Path.Combine(productImageDir, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}