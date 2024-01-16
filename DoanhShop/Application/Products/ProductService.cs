using Domain.Abstractions;
using Domain.Entities;

namespace Application.Products
{
    public interface IProductService
    {
        GenericData<ProductViewModel> GetProducts(ProductPage model);
    }
    public class ProductService : IProductService
    {
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IRepository<Review, Guid> _reviewRepository;
        private readonly IRepository<ProductImage, Guid> _imageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService
        (
            IRepository<Product, Guid> productRepository,
            IRepository<Category, Guid> categoryRepository,
            IRepository<Review, Guid> reviewRepository,
            IRepository<ProductImage, Guid> imageRepository,
            IUnitOfWork unitOfWork
        )
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _reviewRepository = reviewRepository;
            _imageRepository = imageRepository;
            _unitOfWork = unitOfWork;
        }

        public GenericData<ProductViewModel> GetProducts(ProductPage filter)
        {
            var data = new GenericData<ProductViewModel>();
            var products = _productRepository.FindAll();
            var categories = _categoryRepository.FindAll();

            var result = (from p in products
                          join c in categories
                          on p.CategoryId equals c.Id
                          select new ProductViewModel
                          {
                              ProductId = p.Id,
                              ProductName = p.Name,
                              Price = p.Price,
                              DiscountPrice = p.DiscountPrice,
                              CategoryName = c.Name,
                              CategoryId = c.Id,
                          }).AsEnumerable();

            if (!string.IsNullOrEmpty(filter.CategoryId) && Guid.TryParse(filter.CategoryId, out Guid categoryId))
            {
                result = result.Where(s => s.CategoryId == categoryId);
            }

            if (filter.FromPrice.HasValue && filter.ToPrice.HasValue)
            {
                result = result.Where(s => s.Price >= filter.FromPrice.Value && s.Price <= filter.ToPrice);
            }

            if (filter.ToPrice.HasValue && !filter.FromPrice.HasValue)
            {
                result = result.Where(s => s.Price <= filter.ToPrice.Value);
            }
            if (filter.FromPrice.HasValue && !filter.ToPrice.HasValue)
            {
                result = result.Where(s => s.Price >= filter.FromPrice.Value);
            }

            if (!string.IsNullOrEmpty(filter.KeyWord))
            {
                result = result.Where(s => s.ProductName.Contains(filter.KeyWord, StringComparison.OrdinalIgnoreCase) || s.CategoryName.Contains(filter.KeyWord, StringComparison.OrdinalIgnoreCase));
            }
            if (filter.SortBy.Equals(SortEnum.Price))
            {
                result = result.OrderBy(s => s.Price);
            }
            else
            {
                result = result.OrderBy(s => s.ProductName);
            }
            return data;
        }
    }
}
