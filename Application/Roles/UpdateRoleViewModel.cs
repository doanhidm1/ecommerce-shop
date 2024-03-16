using System.ComponentModel.DataAnnotations;

namespace Application.Roles
{
    public class UpdateRoleViewModel
    {
        [Required]
        public string RoleId { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}
