using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.Accounts
{
    public class LoginViewModel
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        [PasswordPropertyText(true)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
