using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.Accounts
{
    public class LoginViewModel
    {
        [Required]
        public string EmailOrUsername { get; set; }

        [Required]
        [PasswordPropertyText(true)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
