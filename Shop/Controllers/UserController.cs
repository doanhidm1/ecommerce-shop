using Application;
using Application.Users;
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
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Invalid model state!"));
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
                var callbackUrl = Url.Action("ConfirmEmail", "User", new { userId = user.Id, token }, protocol: HttpContext.Request.Scheme);
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
                return RedirectToAction("Login", "Account");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Login", "Account");
            }
            var isConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (isConfirmed)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User already confirmed!"));
                return RedirectToAction("Login", "Account");
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, "Confirm email successfully!"));
                return RedirectToAction("Login","Account");
            }
            TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Confirm email failed!"));
            return RedirectToAction("Login", "Account");
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

        public async Task<IActionResult> ManageRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Index");
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new ManageRoleViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                CurrentRoles = await _userManager.GetRolesAsync(user),
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRole(ManageRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Invalid model state!"));
                return RedirectToAction("Index");
            }
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "User not exist!"));
                return RedirectToAction("Index");
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var addRolesResult = await _userManager.AddToRolesAsync(user, model.CurrentRoles!.Except(userRoles));
                if (!addRolesResult.Succeeded)
                {
                    throw new Exception("Add role(s) to user failed!");
                }
                await _unitOfWork.SaveChangesAsync();
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(model.CurrentRoles!));
                if (!removeRolesResult.Succeeded)
                {
                    throw new Exception("Remove role(s) from user failed!");
                }
                await _unitOfWork.SaveChangesAsync();
                await _userManager.UpdateSecurityStampAsync(user);
                await transaction.CommitAsync();
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(200, $"Updated role(s) for user {user.UserName} successfully"));
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["response"] = JsonConvert.SerializeObject(new ResponseResult(400, "Update role(s) for user failed!"));
                return RedirectToAction("Index");
            }
        }
    }
}
