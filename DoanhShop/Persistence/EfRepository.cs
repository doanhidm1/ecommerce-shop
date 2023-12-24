using Domain.Abstractions;
using Domain.Entities;

namespace Persistence
{
    public class EfRepository : IRepository
    {
        private readonly ApplicationDbContext _context;
        public EfRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public List<Student> FindAll()
        {
            return _context.Students.ToList();
        }

        public void Add(Student student)
        {
            _context.Students.Add(student);
        }
    }
}
