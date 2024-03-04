using System.ComponentModel.DataAnnotations;

namespace Application.Accounts
{
    public class CreateRoleViewModel
    {
        [Required]
        public string RoleName { get; set; }
    }
}