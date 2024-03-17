using Domain.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Brands
{

    public interface IBrandService
    {
        Task<List<BrandViewModel>> GetBrands();
        Task CreateBrand(BrandCreateViewModel model);
        Task UpdateBrand(BrandUpdateViewModel model);
        Task<BrandViewModel> GetBrandDetail(Guid id);
        Task DeleteBrand(Guid id);
    }
    public class BrandService : IBrandService
    {
        private readonly IRepository<Brand, Guid> _brandRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BrandService(
            IRepository<Brand, Guid> brandRepository,
            IRepository<Product, Guid> productRepository,
            IUnitOfWork unitOfWork)
        {
            _brandRepository = brandRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<BrandViewModel>> GetBrands()
        {
            var products = _productRepository.GetAll();
            var brands = _brandRepository.GetAll();
            var brandViewModels = await (from p in products
                                         join b in brands
                                         on p.BrandId equals b.Id
                                         group p by new { b.Id, b.Name } into g
                                         select new BrandViewModel
                                         {
                                             Id = g.Key.Id,
                                             Name = g.Key.Name,
                                             ProductCount = g.Count()
                                         }).ToListAsync();
            return brandViewModels;
        }

        public async Task<BrandViewModel> GetBrandDetail(Guid id)
        {
            var brand = await _brandRepository.FindById(id);
            if (brand == null)
            {
                throw new Exception("Brand not found!");
            }
            var productCount = await _productRepository.GetAll().CountAsync(p => p.BrandId == id);
            var brandViewModel = new BrandViewModel
            {
                Id = brand.Id,
                Name = brand.Name,
                ProductCount = productCount
            };
            return brandViewModel;
        }

        public async Task CreateBrand(BrandCreateViewModel model)
        {
            var brand = new Brand
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                CreatedDate = DateTime.Now,
                Status = EntityStatus.Active,
            };
            await _brandRepository.Add(brand);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateBrand(BrandUpdateViewModel model)
        {
            var brand = await _brandRepository.FindById(model.Id);
            if (brand == null)
            {
                throw new Exception("Brand not found!");
            }
            brand.Name = model.Name;
            await _brandRepository.Update(brand);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteBrand(Guid id)
        {
            var brand = await _brandRepository.FindById(id);
            if (brand == null)
            {
                throw new Exception("Brand not found!");
            }
            var products = await _productRepository.GetAll().Where(p => p.BrandId == id).ToListAsync();
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
                await _brandRepository.Delete(brand);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
