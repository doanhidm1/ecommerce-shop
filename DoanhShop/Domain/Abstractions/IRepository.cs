using Domain.Entities;

namespace Domain.Abstractions
{
    public interface IRepository
    {
        List<Student> FindAll();

        void Add(Student student);
    }
}
