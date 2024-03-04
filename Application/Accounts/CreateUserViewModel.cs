using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.Accounts
{
    public class CreateUserViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [PasswordPropertyText(true)]
        public string Password { get; set; }

        [Compare("Password")]
        [PasswordPropertyText(true)]
        public string ConfirmPassword { get; set; }

        [Required]
        public string RoleId { get; set; }
    }
}
