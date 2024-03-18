using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Categories
{
    public class CategoryCreateViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public IFormFile Image { get; set; }
        public string? ImageUrl { get; set; }
    }
}
