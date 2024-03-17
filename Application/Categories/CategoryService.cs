using Domain.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories
{

    public interface ICategoryService
    {
        Task<List<CategoryViewModel>> GetCategories();
        Task CreateCategory(CategoryCreateViewModel model);
        Task UpdateCategory(CategoryUpdateViewModel model);
        Task<CategoryViewModel> GetCategoryDetail(Guid id);
        Task DeleteCategory(Guid id);
    }
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(
            IRepository<Category, Guid> categoryRepository,
            IRepository<Product, Guid> productRepository,
            IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task CreateCategory(CategoryCreateViewModel model)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Image = model.ImageUrl!,
                CreatedDate = DateTime.Now,
                Status = EntityStatus.Active,
            };
            await _categoryRepository.Add(category);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteCategory(Guid id)
        {
            var category = await _categoryRepository.FindById(id);
            if (category == null)
            {
                throw new Exception("Category not found!");
            }
            var products = await _productRepository.GetAll().Where(p => p.CategoryId == id).ToListAsync();
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (products.Any())
                {
                    foreach (var product in products)
                    {
                        await _productRepository.Delete(product);
                    }
                }
                await _categoryRepository.Delete(category);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<CategoryViewModel>> GetCategories()
        {
            var products = _productRepository.GetAll();
            var categories = _categoryRepository.GetAll();
            var categoryViewModels = await (from p in products
                                            join c in categories
                                            on p.CategoryId equals c.Id
                                            group p by new { c.Id, c.Name, c.Image } into g
                                            select new CategoryViewModel
                                            {
                                                Id = g.Key.Id,
                                                Name = g.Key.Name,
                                                Image = g.Key.Image,
                                                ProductCount = g.Count()
                                            }).ToListAsync();
            return categoryViewModels;
        }

        public async Task<CategoryViewModel> GetCategoryDetail(Guid id)
        {
            var category = await _categoryRepository.FindById(id);
            if (category == null)
            {
                throw new Exception("Category not found!");
            }
            var productCount = await _productRepository.GetAll().CountAsync(p => p.CategoryId == id);
            var categoryViewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ProductCount = productCount,
                Image = category.Image
            };
            return categoryViewModel;
        }

        public async Task UpdateCategory(CategoryUpdateViewModel model)
        {
            var category = await _categoryRepository.FindById(model.Id);
            if (category == null)
            {
                throw new Exception("Category not found!");
            }
            category.Name = model.Name;
            category.Image = model.ImageUrl;
            await _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
