using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class EfRepository : IRepository
    {
        private readonly ApplicationDbContext _context;
        public EfRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Student>> FindAll()
        {
            return await _context.Students.ToListAsync();
        }

        public void Add(Student student)
        {
            _context.Students.Add(student);
        }

        public void Update(Student student)
        {
            _context.Students.Update(student);
        }

        public async Task<Student> FindById(Guid id)
        {
            return await _context.Students.FirstAsync(s => s.Id == id) ?? throw new Exception("student not found");
        }

        public void Delete(Student student)
        {
            _context.Students.Remove(student);
        }
    }

    public class EfRepository1<TEntity, TKey> : IRepository1<TEntity, TKey> where TEntity : class
    {
        private readonly ApplicationDbContext _context;
        public EfRepository1(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

        public void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public IQueryable<TEntity> FindAll()
        {
            var result = _context.Set<TEntity>().AsQueryable();
            return result;
        }

        public async Task<TEntity> FindById(TKey id)
        {
            var result = await _context.Set<TEntity>().FindAsync(id);
            if(result == null)
            {
                throw new Exception("id not found");
            }
            return result;
        }

        public void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }
    }
}
