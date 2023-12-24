using Microsoft.EntityFrameworkCore;

namespace Domain.Entities
{
    public class ApplicationDbContext : DbContext
    {

        public DbSet<Student> Students { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

    }
}
