using System.ComponentModel.DataAnnotations;

namespace Application.Roles
{
    public class CreateRoleViewModel
    {
        [Required]
        public string RoleName { get; set; }
    }
}