using Microsoft.EntityFrameworkCore;

namespace Domain.Entities
{
    public class ShopDBContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Review> Reviews { get; set; }
		public DbSet<ProductImage> ProductImages { get; set; }
        public ShopDBContext(DbContextOptions<ShopDBContext> options) : base(options)
        {
        }
    }
}
