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
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        private const string ImageFolder = "AccountAvatars";

        public UserController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment,
            IUnitOfWork unitOfWork
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _unitOfWork = unitOfWork;
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users ?? new List<User>());
        }

        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles ?? new List<IdentityRole>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("CreateUser");
            }
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

                var addRolesResult = await _userManager.AddToRolesAsync(user, model.Roles);
                if (!addRolesResult.Succeeded)
                {
                    throw new Exception("Add role(s) to user failed!");
                }
                await _unitOfWork.SaveChangesAsync();

                var sendEmailResult = await SendConfirmEmail(user, token);
                if (!sendEmailResult)
                {
                    throw new Exception("Send confirm email failed!");
                }

                await transaction.CommitAsync();
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Created user {user.UserName} successfully"));
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Create user failed!"));
                return RedirectToAction("Create");
            }
        }

        public async Task<IActionResult> Update(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Index");
            }
            var model = new UpdateUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Index");
            }
            if (!user.UserName!.Equals(model.UserName))
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
            if (!user.Email!.Equals(model.Email))
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
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Index");
            }
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "You can't delete yourself!"));
                return RedirectToAction("Index");
            }
            DeleteImage(user.AvatarUrl);
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Deleted user {user.UserName} successfully"));
                return RedirectToAction("Index");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Delete user failed!"));
            return RedirectToAction("Index");
        }

        private async Task<bool> SendConfirmEmail(User user, string token)
        {
            try
            {
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, protocol: HttpContext.Request.Scheme);
                var subject = "Confirm your email";
                var message = $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.";
                await _emailSender.SendEmailAsync(user.Email!, subject, message);
                return true;
            }
            catch (Exception)
            {
                return false;
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
            if (user.Email!.Equals(newEmail))
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
    }
}
