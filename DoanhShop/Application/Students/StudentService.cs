using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Students
{
    public interface IStudentService
    {
        Task<StudentData> GetStudentsAsync(Page model);
        Task AddStudent(CreateStudentRequest request);
        Task UpdateStudent(UpdateStudentRequest request);
        Task<StudentViewModel> GetStudentsByIdAsync(Guid id);
        Task DeleteStudent(Guid id);
    }
    //public class StudentService : IStudentService
    //{
    //    private readonly ApplicationDbContext _context;
    //    public StudentService(ApplicationDbContext context)
    //    {
    //        _context = context;
    //    }

    //    public async Task<List<StudentViewModel>> GetStudentsAsync()
    //    {
    //        var students = _context.Students;
    //        var result = await students.Select(s => new StudentViewModel
    //        {
    //            Id = s.Id,
    //            Name = s.Name,
    //            Age = s.Age,
    //        }).ToListAsync();
    //        return result;
    //    }

        //public List<StudentViewModel> GetStudents()
        //{
        //    var students = _context.Students;
        //    var result = students.Select(s => new StudentViewModel
        //    {
        //        Id = s.Id,
        //        Name = s.Name,
        //        Age = s.Age,
        //    }).ToList();
        //    return result;
        //}
    //}

    public class StudentService1 : IStudentService
    {
        private readonly IRepository1<Student, Guid> _repository;
        private readonly IRepository1<Major, Guid> _majorRepository;
        private readonly IUnitOfWork _unitOfWork;
        public StudentService1(IRepository1<Student, Guid> repository, IUnitOfWork unitOfWork, IRepository1<Major, Guid> majorRepository)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _majorRepository = majorRepository;

        }
        // ask how resquest type is CreateStudentRequest
        public async Task AddStudent(CreateStudentRequest request)
        {
            var student = new Student
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Age = request.Age,
            };
            _repository.Add(student);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<StudentViewModel> GetStudentsByIdAsync(Guid id)
        {
            var student = await _repository.FindById(id);
            if (student == null)
            {
                throw new Exception("student not found");
            }
            return new StudentViewModel
            {
                Id = student.Id,
                Name = student.Name,
                Age = student.Age,
            };
        }

        public async Task<StudentData> GetStudentsAsync(Page model) //2
        {
            var data = new StudentData();
            var students = _repository.FindAll();
            data.TotalStudent = students.Count();
            students = students.OrderBy(s => s.Name).Skip(model.SkipNumber).Take(model.PageSize);
            var result = await students.Select(s => new StudentViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Age = s.Age,
            }).ToListAsync();
            data.Students = result;
            return data;
        }

        public async Task UpdateStudent(UpdateStudentRequest request)
        {
            var student = await _repository.FindById(request.Id);
            student.Name = request.Name;
            student.Age = request.Age;
            _repository.Update(student);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task DeleteStudent(Guid id)
        {
            var student = await _repository.FindById(id);
            _repository.Delete(student);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
