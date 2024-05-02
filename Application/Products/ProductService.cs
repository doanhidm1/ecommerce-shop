using Application.Brands;
using Application.Categories;
using ClosedXML.Excel;
using Domain.Abstractions;
using Domain.Entities;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using Xceed.Document.NET;
using Xceed.Words.NET;
using iTLE = iText.Layout.Element;

namespace Application.Products
{
    public interface IProductService
    {
        Task<GenericData<ProductViewModel>> GetProducts(ProductPage model);
        Task<ProductDetailViewModel> GetProductDetail(Guid productId);
        Task<CartItemViewModel> GetProductDetailForCart(Guid productId);
        Task<WishlistItemViewModel> GetProductDetailForWishlist(Guid productId);

        Task CreateProduct(ProductCreateViewModel model);
        Task UpdateProduct(ProductUpdateViewModel model);
        Task DeleteProduct(Guid productId);
        Task UpdateQuantity(Guid productId, int quantity);

        Task<byte[]> ExportToWord(List<ProductViewModel> data);
        Task<byte[]> ExportToExcel(List<ProductViewModel> data);
        Task<byte[]> ExportToPdf(List<ProductViewModel> data);
    }

    public class ProductService : IProductService
    {
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Review, Guid> _reviewRepository;
        private readonly IRepository<ProductImage, Guid> _imageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;

        public ProductService(
            IRepository<Product, Guid> productRepository,
            IRepository<Review, Guid> reviewRepository,
            IRepository<ProductImage, Guid> imageRepository,
            IUnitOfWork unitOfWork,
            ICategoryService categoryService,
            IBrandService brandService
        )
        {
            _productRepository = productRepository;
            _reviewRepository = reviewRepository;
            _imageRepository = imageRepository;
            _unitOfWork = unitOfWork;
            _categoryService = categoryService;
            _brandService = brandService;
        }

        public async Task<byte[]> ExportToWord(List<ProductViewModel> data)
        {
            byte[] result;
            using (var document = DocX.Create("ProductReport.docx"))
            {
                document.InsertParagraph("Product Report").FontSize(24).Font(new Font("Times New Roman")).Bold().Alignment = Alignment.center;
                document.InsertParagraph("");
                document.SetDefaultFont(new Font("Times New Roman"), 12);

                var table = document.AddTable(data.Count + 1, 9);
                table.Rows[0].Cells[0].Paragraphs.First().Append("No.").Bold();
                table.Rows[0].Cells[1].Paragraphs.First().Append("Product Name").Bold();
                table.Rows[0].Cells[2].Paragraphs.First().Append("Price").Bold();
                table.Rows[0].Cells[3].Paragraphs.First().Append("Discount Price").Bold();
                table.Rows[0].Cells[4].Paragraphs.First().Append("Stock").Bold();
                table.Rows[0].Cells[5].Paragraphs.First().Append("Category").Bold();
                table.Rows[0].Cells[6].Paragraphs.First().Append("Brand").Bold();
                table.Rows[0].Cells[7].Paragraphs.First().Append("Featured ?").Bold();
                table.Rows[0].Cells[8].Paragraphs.First().Append("Rating").Bold();

                for (int i = 0; i < data.Count; i++)
                {
                    var product = data[i];
                    var brand = await _brandService.GetBrandDetail(product.BrandId);
                    var category = await _categoryService.GetCategoryDetail(product.CategoryId);

                    table.Rows[i + 1].Cells[0].Paragraphs.First().Append((i + 1).ToString());
                    table.Rows[i + 1].Cells[1].Paragraphs.First().Append(product.ProductName);
                    table.Rows[i + 1].Cells[2].Paragraphs.First().Append(product.Price.ToString());
                    table.Rows[i + 1].Cells[3].Paragraphs.First().Append(product.DiscountPrice.ToString());
                    table.Rows[i + 1].Cells[4].Paragraphs.First().Append(product.Stock.ToString());
                    table.Rows[i + 1].Cells[5].Paragraphs.First().Append(category.Name);
                    table.Rows[i + 1].Cells[6].Paragraphs.First().Append(brand.Name);
                    table.Rows[i + 1].Cells[7].Paragraphs.First().Append(product.IsFeatured ? "Yes" : "No");
                    table.Rows[i + 1].Cells[8].Paragraphs.First().Append(product.Rating.ToString("0.##"));
                }
                document.InsertTable(table);
                using var stream = new MemoryStream();
                document.SaveAs(stream);
                result = stream.ToArray();
            }
            return result;
        }

        public async Task<byte[]> ExportToExcel(List<ProductViewModel> data)
        {
            byte[] result;
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Product Report");
                worksheet.Style.Font.FontSize = 12;
                worksheet.Style.Font.FontName = "Times New Roman";
                var range = worksheet.Range("A1:I1");
                range.Merge();
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell("A1").Style.Font.FontSize = 24;
                worksheet.Cell("A1").Value = "Product Report";

                worksheet.Cell("A2").Value = "No.";
                worksheet.Cell("B2").Value = "Product Name";
                worksheet.Cell("C2").Value = "Price";
                worksheet.Cell("D2").Value = "Discount Price";
                worksheet.Cell("E2").Value = "Stock";
                worksheet.Cell("F2").Value = "Category";
                worksheet.Cell("G2").Value = "Brand";
                worksheet.Cell("H2").Value = "Featured ?";
                worksheet.Cell("I2").Value = "Rating";

                range = worksheet.Range("A2:I2");
                range.Style.Font.Bold = true;

                for (int i = 0; i < data.Count; i++)
                {
                    var product = data[i];
                    var brand = await _brandService.GetBrandDetail(product.BrandId);
                    var category = await _categoryService.GetCategoryDetail(product.CategoryId);

                    worksheet.Cell($"A{i + 3}").Value = i + 1;
                    worksheet.Cell($"B{i + 3}").Value = product.ProductName;
                    worksheet.Cell($"C{i + 3}").Value = product.Price;
                    worksheet.Cell($"D{i + 3}").Value = product.DiscountPrice;
                    worksheet.Cell($"E{i + 3}").Value = product.Stock;
                    worksheet.Cell($"F{i + 3}").Value = category.Name;
                    worksheet.Cell($"G{i + 3}").Value = brand.Name;
                    worksheet.Cell($"H{i + 3}").Value = product.IsFeatured ? "Yes" : "No";
                    worksheet.Cell($"I{i + 3}").Value = product.Rating.ToString("0.##");
                }
                worksheet.Row(2).InsertRowsAbove(1);

                range = worksheet.Range($"A3:I{data.Count + 3}");
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Columns().AdjustToContents();
                worksheet.Rows().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                result = stream.ToArray();
            }
            return result;
        }

        public async Task<byte[]> ExportToPdf(List<ProductViewModel> data)
        {
            byte[] result;
            using (var stream = new MemoryStream())
            {
                var pdf = new PdfDocument(new PdfWriter(stream));
                var document = new iText.Layout.Document(pdf);
                document.SetFont(PdfFontFactory.CreateFont("timesbd.ttf", PdfEncodings.IDENTITY_H));
                document.SetFontSize(12);

                var header = new iTLE.Paragraph("Product Report");
                header.SetFontSize(24).SetBold().SetTextAlignment(TextAlignment.CENTER);
                document.Add(header);

                document.Add(new iTLE.Paragraph(""));

                var table = new iTLE.Table(9);
                table.SetWidth(UnitValue.CreatePercentValue(100));
                table.AddCell(new iTLE.Paragraph("No.").SetBold());
                table.AddCell(new iTLE.Paragraph("Product Name").SetBold());
                table.AddCell(new iTLE.Paragraph("Price").SetBold());
                table.AddCell(new iTLE.Paragraph("Discount Price").SetBold());
                table.AddCell(new iTLE.Paragraph("Stock").SetBold());
                table.AddCell(new iTLE.Paragraph("Category").SetBold());
                table.AddCell(new iTLE.Paragraph("Brand").SetBold());
                table.AddCell(new iTLE.Paragraph("Featured ?").SetBold());
                table.AddCell(new iTLE.Paragraph("Rating").SetBold());

                document.SetFont(PdfFontFactory.CreateFont("times.ttf", PdfEncodings.IDENTITY_H));
                for (int i = 0; i < data.Count; i++)
                {
                    var product = data[i];
                    var brand = await _brandService.GetBrandDetail(product.BrandId);
                    var category = await _categoryService.GetCategoryDetail(product.CategoryId);

                    table.AddCell((i + 1).ToString());
                    table.AddCell(product.ProductName);
                    table.AddCell(product.Price.ToString());
                    table.AddCell(product.DiscountPrice.ToString());
                    table.AddCell(product.Stock.ToString());
                    table.AddCell(category.Name);
                    table.AddCell(brand.Name);
                    table.AddCell(product.IsFeatured ? "Yes" : "No");
                    table.AddCell(product.Rating.ToString("0.##"));
                }
                document.Add(table);
                document.Close();
                result = stream.ToArray();
            }
            return result;
        }

        public async Task UpdateQuantity(Guid productId, int quantity)
        {
            var product = await _productRepository.FindById(productId) ?? throw new Exception("Product not found");
            product.Quantity -= quantity;
            await _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<GenericData<ProductViewModel>> GetProducts(ProductPage filter)
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
                productViewModels = await result.ToListAsync();
            }
            else
            {
                productViewModels = await result.Skip(filter.SkipNumber).Take(filter.PageSize).ToListAsync();
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
            var product = await _productRepository.FindById(productId) ?? throw new Exception("Product not found");
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
            var product = await _productRepository.FindById(productId) ?? throw new Exception("Product not found");
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
            var product = await _productRepository.FindById(productId) ?? throw new Exception("Product not found");
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
                await _productRepository.Add(product);
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
                    await _imageRepository.Add(image);
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

        public async Task UpdateProduct(ProductUpdateViewModel model)
        {
            var product = await _productRepository.FindById(model.ProductId) ?? throw new Exception("Product not found");
            product.Name = model.ProductName;
            product.Description = model.Description;
            product.Detail = model.Detail;
            product.Price = model.Price;
            product.DiscountPrice = model.DiscountPrice;
            product.Quantity = model.Quantity;
            product.CategoryId = model.CategoryId;
            product.BrandId = model.BrandId;
            product.IsFeatured = model.IsFeatured;
            await _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteProduct(Guid productId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var product = await _productRepository.FindById(productId) ?? throw new Exception("Product not found");
                // delete product images
                var productImages = await _imageRepository.GetAll().Where(s => s.ProductId == productId).ToListAsync();
                foreach (var item in productImages)
                {
                    await _imageRepository.Delete(item);
                    await _unitOfWork.SaveChangesAsync();
                }
                // delete product reviews
                var productReviews = await _reviewRepository.GetAll().Where(s => s.ProductId == productId).ToListAsync();
                foreach (var item in productReviews)
                {
                    await _reviewRepository.Delete(item);
                    await _unitOfWork.SaveChangesAsync();
                }
                // delete product
                await _productRepository.Delete(product);
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
