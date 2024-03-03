using Domain.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Products
{
    public interface IProductService
    {
        GenericData<ProductViewModel> GetProducts(ProductPage model);
        Task<ProductDetailViewModel> GetProductDetail(Guid productId);
        Task<CartItemViewModel> GetProductDetailForCart(Guid productId);
        Task<WishlistItemViewModel> GetProductDetailForWishlist(Guid productId);
        Task CreateProduct(ProductCreateViewModel model);
        Task DeleteProduct(Guid productId);
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
            IUnitOfWork unitOfWork
        )
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
            var allReviews = _reviewRepository.GetAll();
            var allImages = _imageRepository.GetAll();

            var result = products.Select(p => new ProductViewModel
            {
                ProductId = p.Id,
                ProductName = p.Name,
                CreatedDate = p.CreatedDate,
                Stock = p.Quantity,
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
                result = result.Where(s => s.ProductName.Contains(filter.KeyWord));
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

        public async Task<CartItemViewModel> GetProductDetailForCart(Guid productId)
        {
            var product = await _productRepository.FindById(productId);
            if (product == null)
            {
                return null;
            }
            var cartItem = new CartItemViewModel
            {
                ProductName = product.Name,
                ProductId = product.Id,
                Stock = product.Quantity,
                Image = GetProductImages(productId).Result.First().ImageLink ?? string.Empty,
                Alt = GetProductImages(productId).Result.First().Alt ?? string.Empty,
                Price = product.DiscountPrice.HasValue ? product.DiscountPrice.Value : product.Price,
            };
            return cartItem;
        }

        public async Task<WishlistItemViewModel> GetProductDetailForWishlist(Guid productId)
        {
            var product = await _productRepository.FindById(productId);
            if (product == null)
            {
                return null;
            }
            var wishlistItem = new WishlistItemViewModel
            {
                ProductName = product.Name,
                ProductId = product.Id,
                Stock = product.Quantity,
                Image = GetProductImages(productId).Result.First().ImageLink ?? string.Empty,
                Alt = GetProductImages(productId).Result.First().Alt ?? string.Empty,
                Price = product.DiscountPrice.HasValue ? product.DiscountPrice.Value : product.Price,
            };
            return wishlistItem;
        }

        public async Task<ProductDetailViewModel> GetProductDetail(Guid productId)
        {
            var product = await _productRepository.FindById(productId);
            if (product == null)
            {
                return null;
            }
            var productImages = await GetProductImages(productId);
            var productReviews = await GetProductReviews(productId);

            var productDetail = new ProductDetailViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                CreatedDate = product.CreatedDate,
                Stock = product.Quantity,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                IsFeatured = product.IsFeatured,
                Description = product.Description,
                Detail = product.Detail,
                ReviewCount = productReviews.Count(),
                Rating = productReviews.Count() > 0 ? productReviews.Average(s => s.Rating) : null,
                Images = productImages,
                Reviews = productReviews
            };

            return productDetail;
        }

        public async Task<List<ImageViewModel>> GetProductImages(Guid productId)
        {
            var productImages = await _imageRepository.GetAll()
                .Where(s => s.ProductId == productId)
                .Select(s => new ImageViewModel
                {
                    Id = s.Id,
                    ImageLink = s.ImageLink,
                    Alt = s.Alt
                }).ToListAsync();
            return productImages;
        }

        public async Task<List<ReviewViewModel>> GetProductReviews(Guid productId)
        {
            var productReviews = await _reviewRepository.GetAll()
                .Where(s => s.ProductId == productId)
                .Select(s => new ReviewViewModel
                {
                    Id = s.Id,
                    ReviewerName = s.ReviewerName,
                    Content = s.Content,
                    Rating = s.Rating,
                    CreatedDate = s.CreatedDate
                }).ToListAsync();
            return productReviews;
        }

        public async Task CreateProduct(ProductCreateViewModel model)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var product = new Product
                {
                    Name = model.ProductName,
                    Description = model.Description,
                    Detail = model.Detail,
                    Price = model.Price,
                    DiscountPrice = model.DiscountPrice,
                    Quantity = model.Quantity,
                    CategoryId = model.CategoryId,
                    BrandId = model.BrandId,
                    IsFeatured = model.IsFeatured,
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    Status = Domain.Enums.EntityStatus.Active,
                };
                _productRepository.Add(product);
                await _unitOfWork.SaveChangesAsync();
                foreach (var item in model.ImageUrls)
                {
                    var image = new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ImageLink = item,
                        CreatedDate = product.CreatedDate,
                        Status = Domain.Enums.EntityStatus.Active,
                        ProductId = product.Id,
                        Alt = product.Name

                    };
                    _imageRepository.Add(image);
                    await _unitOfWork.SaveChangesAsync();
                }
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteProduct(Guid productId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var product = await _productRepository.FindById(productId);
                if (product == null)
                {
                    throw new Exception("Product not found");
                }
                // delete product images
                var productImages = await _imageRepository.GetAll().Where(s => s.ProductId == productId).ToListAsync();
                foreach (var item in productImages)
                {
                    _imageRepository.Delete(item);
                    await _unitOfWork.SaveChangesAsync();
                }
                // delete product reviews
                var productReviews = await _reviewRepository.GetAll().Where(s => s.ProductId == productId).ToListAsync();
                foreach (var item in productReviews)
                {
                    _reviewRepository.Delete(item);
                    await _unitOfWork.SaveChangesAsync();
                }
                // delete product
                _productRepository.Delete(product);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
