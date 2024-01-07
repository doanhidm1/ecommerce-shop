using Domain.Entities;

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
}
