using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.Users
{
    public class CreateUserViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        [PasswordPropertyText(true)]
        public string Password { get; set; }

        [Compare("Password")]
        [PasswordPropertyText(true)]
        public string ConfirmPassword { get; set; }

        [Required]
        public IFormFile Avatar { get; set; }
        public string? AvatarUrl { get; set; }

        public List<string> Roles { get; set; }
    }
}
