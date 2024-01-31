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
        // private readonly IUnitOfWork _unitOfWork;

        public ProductService(
            IRepository<Product, Guid> productRepository,
            IRepository<Review, Guid> reviewRepository,
            IRepository<ProductImage, Guid> imageRepository
        // IUnitOfWork unitOfWork
        )
        {
            _productRepository = productRepository;
            _reviewRepository = reviewRepository;
            _imageRepository = imageRepository;
            // _unitOfWork = unitOfWork;
        }

        public GenericData<ProductViewModel> GetProducts(ProductPage filter)
        {
            var data = new GenericData<ProductViewModel>();
            var products = _productRepository.GetAll();
            var allReviews = _reviewRepository.GetAll();
            var allImages = _imageRepository.GetAll();

            var result = products.Select(p => new ProductViewModel
            {
                ProductId = p.Id,
                ProductName = p.Name,
                CreatedDate = p.CreatedDate,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                IsFeatured = p.IsFeatured
            });

            if (filter.IsFeatured)
            {
                result = result.Where(s => s.IsFeatured);
                goto ImgAndReview;
            }

            if (!string.IsNullOrEmpty(filter.CategoryId) && Guid.TryParse(filter.CategoryId, out Guid categoryId))
            {
                result = result.Where(s => s.CategoryId == categoryId);
            }
            if (filter.SelectedBrandIds != null && filter.SelectedBrandIds.Any())
            {
                result = result.Where(s => filter.SelectedBrandIds.Contains(s.BrandId));
            }
            if (filter.FromPrice.HasValue && filter.ToPrice.HasValue)
            {
                // if product has discount price, compare with discount price
                result = result.Where(s => (s.Price >= filter.FromPrice.Value && s.Price <= filter.ToPrice.Value && s.DiscountPrice == null) || (s.DiscountPrice >= filter.FromPrice.Value && s.DiscountPrice <= filter.ToPrice.Value));
            }
            if (filter.ToPrice.HasValue && !filter.FromPrice.HasValue)
            {
                // if product has discount price, compare with discount price
                result = result.Where(s => (s.Price <= filter.ToPrice.Value && s.DiscountPrice == null) || s.DiscountPrice <= filter.ToPrice.Value);
            }
            if (filter.FromPrice.HasValue && !filter.ToPrice.HasValue)
            {
                // if product has discount price, compare with discount price
                result = result.Where(s => (s.Price >= filter.FromPrice.Value && s.DiscountPrice == null) || s.DiscountPrice >= filter.FromPrice.Value);
            }
            if (filter.Rating.HasValue)
            {
                result = result.Where(s => allReviews.Where(rv => rv.ProductId == s.ProductId).Average(rv => rv.Rating) >= filter.Rating);
            }
            if (!string.IsNullOrEmpty(filter.KeyWord))
            {
                result = result.Where(s => s.ProductName.Contains(filter.KeyWord, StringComparison.OrdinalIgnoreCase));
            }

            switch (filter.SortBy)
            {
                case SortEnum.Price:
                    result = result.OrderBy(s => s.DiscountPrice.HasValue ? s.DiscountPrice : s.Price);
                    break;
                case SortEnum.Name:
                    result = result.OrderBy(s => s.ProductName);
                    break;
                case SortEnum.Date:
                    result = result.OrderByDescending(s => s.CreatedDate);
                    break;
                case SortEnum.Rating:
                    result = result.OrderByDescending(s => allReviews.Where(rv => rv.ProductId == s.ProductId).Average(rv => rv.Rating));
                    break;
                default:
                    break;
            }
        ImgAndReview:
            data.Count = result.Count();
            var productViewModels = new List<ProductViewModel>();
            if (filter.IsFeatured)
            {
                productViewModels = result.ToList();
            }
            else
            {
                productViewModels = result.Skip(filter.SkipNumber).Take(filter.PageSize).ToList();
            }

            foreach (var productView in productViewModels)
            {
                var productImage = allImages.FirstOrDefault(img => img.ProductId == productView.ProductId);
                if (productImage != null)
                {
                    productView.ImageUrl = productImage.ImageLink;
                    productView.ImageAlt = productImage.Alt;
                }

                var productReviews = allReviews.Where(rv => rv.ProductId == productView.ProductId);
                productView.ReviewCount = productReviews.Count();
                if (productView.ReviewCount > 0)
                {
                    productView.Rating = productReviews.Average(s => s.Rating);
                }
            }

            data.Data = productViewModels;
            return data;
        }

    }
}
