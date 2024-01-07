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
}
