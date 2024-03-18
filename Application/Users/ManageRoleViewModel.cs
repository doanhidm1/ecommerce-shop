using System.ComponentModel.DataAnnotations;

namespace Application.Users
{
    public class ManageRoleViewModel
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string UserName { get; set; }
        public IList<string>? CurrentRoles { get; set; } = new List<string>();
    }
}
