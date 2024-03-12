using Application;
using Application.Accounts;
using Domain.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        private readonly IEmailSender _emailSender;

        public AccountController
        (
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment webHostEnvironment,
            IEmailSender emailSender
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
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
            if (!user.EmailConfirmed)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User haven't confirmed email!"));
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
            return View();
        }

        public async Task<IActionResult> UpdateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Users");
            }
            var model = new UpdateUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };
            return View(model);
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
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                foreach (var user in usersInRole)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                }
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
            DeleteImage(user.AvatarUrl);
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
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

        public async Task<IActionResult> UpdateRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role not exist!"));
                return RedirectToAction("Roles");
            }
            var model = new UpdateRoleViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(UpdateUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Users");
            }
            if (!user.UserName.Equals(model.UserName))
            {
                var checkUser = await _userManager.FindByNameAsync(model.UserName);
                if (checkUser != null)
                {
                    TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "New username has been used!"));
                    return RedirectToAction("UpdateUser", new { id = user.Id });
                }
                user.UserName = model.UserName;
            }
            user.PhoneNumber = model.PhoneNumber;
            if (model.Avatar != null)
            {
                DeleteImage(user.AvatarUrl);
                user.AvatarUrl = await SaveImage(model.Avatar);
            }
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User update failed!"));
                return RedirectToAction("UpdateUser", new { id = user.Id });
            }
            await _signInManager.RefreshSignInAsync(user);
            if (!user.Email.Equals(model.Email))
            {
                var checkUser = await _userManager.FindByEmailAsync(model.Email);
                if (checkUser != null)
                {
                    TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Updated user, but new email has been used!"));
                    return RedirectToAction("UpdateUser", new { id = user.Id });
                }
                var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
                var sendEmailResult = await SendChangeConfirmEmail(user, model.Email, token);

                if (!sendEmailResult)
                {
                    TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Change email failed!"));
                    return RedirectToAction("UpdateUser", new { id = user.Id });
                }
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"User '{user.UserName}' updated successfully"));
            return RedirectToAction("Users");
        }

        private async Task<bool> SendChangeConfirmEmail(User user, string newEmail, string token)
        {
            try
            {
                var callbackUrl = Url.Action("ConfirmChangeEmail", "Account", new { userId = user.Id, newEmail = newEmail, token = token }, protocol: HttpContext.Request.Scheme);
                var subject = "Confirm your new email";
                var message = $"Please confirm your new email by <a href='{callbackUrl}'>clicking here</a>.";
                await _emailSender.SendEmailAsync(newEmail, subject, message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmChangeEmail(string userId, string newEmail, string token)
        {
            if (userId.IsNullOrEmpty() || newEmail.IsNullOrEmpty() || token.IsNullOrEmpty())
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Invalid confirm information!"));
                return RedirectToAction("Login");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Login");
            }
            if (user.Email.Equals(newEmail))
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "New email is the same as old email!"));
                return RedirectToAction("Login");
            }
            var checkUser = await _userManager.FindByEmailAsync(newEmail);
            if (checkUser != null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "This email has been used!"));
                return RedirectToAction("Login");
            }
            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "Confirm email change successfully!"));
                return RedirectToAction("Login");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Confirm email change failed!"));
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(UpdateRoleViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role not exist!"));
                return RedirectToAction("Roles");
            }
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name) ?? new List<User>();
            if (!role.Name.Equals(model.RoleName))
            {
                bool isRoleExisted = await _roleManager.RoleExistsAsync(model.RoleName);
                if (isRoleExisted)
                {
                    TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role name existed!"));
                    return RedirectToAction("UpdateRole", new { id = role.Id });
                }
                role.Name = model.RoleName;
            }
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                foreach (var user in usersInRole)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                }
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Role '{role.Name}' updated successfully"));
                return RedirectToAction("Roles");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Role update failed!"));
            return RedirectToAction("UpdateRole", new { id = role.Id });
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
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
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
                var sendEmailResult = await SendConfirmEmail(user, token);

                if (!sendEmailResult)
                {
                    throw new Exception("Send confirm email failed!");
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

        private async Task<bool> SendConfirmEmail(User user, string token)
        {
            try
            {
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, protocol: HttpContext.Request.Scheme);
                var subject = "Confirm your email";
                var message = $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.";
                await _emailSender.SendEmailAsync(user.Email, subject, message);
                return true;
            }
            catch (Exception)
            {
                return false;
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

        private void DeleteImage(string image)
        {
            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, ShopConstants.UploadFolder);
            var productImageDir = Path.Combine(uploadFolder, ImageFolder);
            if (image.IsNullOrEmpty()) return;
            var fileName = image.Split('/').Last();
            var filePath = Path.Combine(productImageDir, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId.IsNullOrEmpty() || token.IsNullOrEmpty())
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Invalid confirm information!"));
                return RedirectToAction("Login");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Login");
            }
            var isConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (isConfirmed)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User already confirmed!"));
                return RedirectToAction("Login");
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "Confirm email successfully!"));
                return RedirectToAction("Login");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Confirm email failed!"));
            return RedirectToAction("Login");
        }
    }
}