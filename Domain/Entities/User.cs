using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class User : IdentityUser
    {
        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string AvatarUrl { get; set; } = string.Empty;
    }
}