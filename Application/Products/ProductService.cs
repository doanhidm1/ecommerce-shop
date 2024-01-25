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
        private readonly IRepository<Review, Guid> _reviewRepository;
        private readonly IRepository<ProductImage, Guid> _imageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(
            IRepository<Product, Guid> productRepository,
            IRepository<Review, Guid> reviewRepository,
            IRepository<ProductImage, Guid> imageRepository,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _reviewRepository = reviewRepository;
            _imageRepository = imageRepository;
            _unitOfWork = unitOfWork;
        }
        public GenericData<ProductViewModel> GetProducts(ProductPage filter)
        {
            var data = new GenericData<ProductViewModel>();
            var products = _productRepository.GetAll();

            var result = products.Select(p => new ProductViewModel
            {
                ProductId = p.Id,
                ProductName = p.Name,
                CategoryId = p.CategoryId,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
            });

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
                result = result.Where(s => s.ProductName.Contains(filter.KeyWord, StringComparison.OrdinalIgnoreCase));
            }
            switch (filter.SortBy)
            {
                case SortEnum.Price:
                    result = result.OrderBy(s => s.Price);
                    break;
                case SortEnum.Name:
                    result = result.OrderBy(s => s.ProductName);
                    break;
                default:
                    break;
            }

            // lấy ra số lượng product để tính số trang
            data.Count = result.Count();

            // lấy ra danh sách product ứng với PageIndex truyền vào (lúc đầu là 1)
            var productViewModels = result.Skip(filter.SkipNumber).Take(filter.PageSize).ToList();

            // lấy ra imageurl và rating
            var productIds = productViewModels.Select(p => p.ProductId).ToList();

            var images = _imageRepository.GetAll().Where(i => productIds.Contains(i.ProductId));
            var reviews = _reviewRepository.GetAll().Where(r => productIds.Contains(r.ProductId));

            foreach (var item in productViewModels)
            {
                var image = images.FirstOrDefault(s => s.ProductId == item.ProductId)?.ImageLink;
                item.ImageUrl = string.IsNullOrEmpty(image) ? string.Empty : image;

                var productReviews = reviews.Where(s => s.ProductId == item.ProductId);
                if (productReviews != null && productReviews.Any())
                {
                    item.Rating = productReviews.Max(s => s.Rating);
                }
            }
            
            data.Data = productViewModels;
            return data;
        }
    }
}
