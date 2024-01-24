using Domain.Abstractions;
using Domain.Entities;

namespace Application.Categories
{

    public interface ICategoryService
    {
        List<CategoryViewModel> GetCategories();
    }
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IRepository<Product, Guid> _productRepository;

        public CategoryService(
            IRepository<Category, Guid> categoryRepository,
            IRepository<Product, Guid> productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        public List<CategoryViewModel> GetCategories()
        {
            var products = _productRepository.GetAll();
            var categories = _categoryRepository.GetAll();
            var categoryViewModels = (from p in products
                                      join c in categories
                                      on p.CategoryId equals c.Id
                                      group p by new { c.Id, c.Name, c.Image } into g
                                      select new CategoryViewModel
                                      {
                                          Id = g.Key.Id,
                                          Name = g.Key.Name,
                                          Image = g.Key.Image,
                                          ProductCount = g.Count()
                                      }).ToList();
            return categoryViewModels;
        }
    }
}
