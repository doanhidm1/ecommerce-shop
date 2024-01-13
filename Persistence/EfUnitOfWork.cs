using Domain.Abstractions;
using Domain.Entities;

namespace Persistence
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly ShopDBContext _context;
        public EfUnitOfWork(ShopDBContext context)
        {
            _context = context;
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
