using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Products
{
    public interface IProductService
    {
        Task<ProductData> GetProductsAsync(Page model);
        Task AddProduct(CreateProductRequest request);
        Task UpdateProduct(UpdateProductRequest request);
        Task<ProductViewModel> GetProductsByIdAsync(Guid id);
        Task DeleteProduct(Guid id);
    }

    public class ProductService : IProductService
    {
        private readonly IRepository<Product, Guid> _repository;
        private readonly IUnitOfWork _unitOfWork;
        public ProductService(IRepository<Product, Guid> productRepository, IUnitOfWork unitOfWork)
        {
            _repository = productRepository;
            _unitOfWork = unitOfWork;

        }

        public async Task AddProduct(CreateProductRequest request)
        {
            var Product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
            };
            _repository.Add(Product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ProductViewModel> GetProductsByIdAsync(Guid id)
        {
            var Product = await _repository.FindById(id) ?? throw new Exception("Product not found");
            return new ProductViewModel
            {
                Id = Product.Id,
                Name = Product.Name,
            };
        }

        public async Task<ProductData> GetProductsAsync(Page model)
        {
            var data = new ProductData();
            var Products = _repository.GetAll();
            data.TotalProduct = Products.Count();
            Products = Products.OrderBy(s => s.Name).Skip(model.SkipNumber).Take(model.PageSize);
            var result = await Products.Select(s => new ProductViewModel
            {
                Id = s.Id,
                Name = s.Name,
            }).ToListAsync();
            data.Products = result;
            return data;
        }

        public async Task UpdateProduct(UpdateProductRequest request)
        {
            var Product = await _repository.FindById(request.Id) ?? throw new Exception("Product not found");
            Product.Name = request.Name;
            _repository.Update(Product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteProduct(Guid id)
        {
            var Product = await _repository.FindById(id) ?? throw new Exception("Product not found");
            _repository.Delete(Product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
