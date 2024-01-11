using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain.Abstractions
{
    public interface IRepository
    {
        Task<List<Student>> FindAll();
        void Add(Student student);
        void Update(Student student);
        void Delete(Student student);
        Task<Student> FindById(Guid id);
    }

    public interface IRepository1<TEntity, TKey> where TEntity : class
    {
        IQueryable<TEntity> FindAll();

        void Add(TEntity student);
        void Update(TEntity student);
        void Delete(TEntity student);
        Task<TEntity> FindById(TKey id);
    }

    
}
