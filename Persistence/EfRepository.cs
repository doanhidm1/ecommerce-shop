using Domain.Abstractions;
using Domain.Entities;

namespace Persistence
{
    public class EfRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
    {
        private readonly ShopDBContext _context;

        public EfRepository(ShopDBContext context)
        {
            _context = context;
        }

        public async Task Add(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public async Task Delete(TEntity entity)
        {
            await Task.Run(() => _context.Set<TEntity>().Remove(entity));
        }

        public IQueryable<TEntity> GetAll()
        {
            return _context.Set<TEntity>().AsQueryable();
        }

        public async Task<TEntity?> FindById(TKey id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public async Task Update(TEntity entity)
        {
            await Task.Run(() => _context.Set<TEntity>().Update(entity));
        }
    }
}
