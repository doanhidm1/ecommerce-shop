using Domain.Abstractions;
using Domain.Entities;

namespace Persistence
{
    public class EfRepository<TEntity, TKey>(ShopDBContext context) : IRepository<TEntity, TKey> where TEntity : class
    {
        private readonly ShopDBContext _context = context;

        public void Add(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

        public void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public IQueryable<TEntity> GetAll()
        {
            return _context.Set<TEntity>().AsQueryable();
        }

        public async Task<TEntity?> FindById(TKey id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }
    }
}
