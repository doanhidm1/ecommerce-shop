using System.ComponentModel.DataAnnotations;

namespace Application.Accounts
{
    public class CreateRoleViewModel
    {
        [Required]
        [MinLength(4)]
        public string RoleName { get; set; }

        public string? NormalizedName { get; set; }

        public string? ConcurrencyStamp { get; set; }
    }
}