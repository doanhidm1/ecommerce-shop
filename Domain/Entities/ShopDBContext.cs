using Microsoft.EntityFrameworkCore;

namespace Domain.Entities
{
    public class ShopDBContext(DbContextOptions<ShopDBContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }
    }
}
