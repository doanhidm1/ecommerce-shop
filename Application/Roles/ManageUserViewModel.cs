using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Application.Roles
{
    public class ManageUserViewModel
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public List<string>? CurrentUser { get; set; } = new List<string>();
    }
}
