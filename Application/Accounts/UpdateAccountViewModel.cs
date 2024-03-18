using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Accounts
{
    public class UpdateAccountViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public IFormFile? Avatar { get; set; }
    }
}
