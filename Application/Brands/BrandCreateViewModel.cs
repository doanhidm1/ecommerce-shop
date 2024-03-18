using System.ComponentModel.DataAnnotations;

namespace Application.Brands
{
    public class BrandCreateViewModel
    {
        public Guid? Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
