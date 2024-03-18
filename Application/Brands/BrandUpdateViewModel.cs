using System.ComponentModel.DataAnnotations;

namespace Application.Brands
{
    public class BrandUpdateViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
