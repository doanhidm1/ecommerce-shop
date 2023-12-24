using Domain.Abstractions;
using Domain.Entities;

namespace Persistence
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public EfUnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SaveChangeAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
