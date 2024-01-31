using Domain.Abstractions;
using Domain.Entities;

namespace Application.Brands
{

    public interface IBrandService
    {
        List<BrandViewModel> GetBrands();
    }
    public class BrandService : IBrandService
    {
        private readonly IRepository<Brand, Guid> _brandRepository;
        private readonly IRepository<Product, Guid> _productRepository;

        public BrandService(
            IRepository<Brand, Guid> brandRepository,
            IRepository<Product, Guid> productRepository)
        {
            _brandRepository = brandRepository;
            _productRepository = productRepository;
        }

        public List<BrandViewModel> GetBrands()
        {
            var products = _productRepository.GetAll();
            var brands = _brandRepository.GetAll();
            var brandViewModels = (from p in products
                                   join b in brands
                                   on p.BrandId equals b.Id
                                   group p by new { b.Id, b.Name } into g
                                   select new BrandViewModel
                                   {
                                       Id = g.Key.Id,
                                       Name = g.Key.Name,
                                       ProductCount = g.Count()
                                   }).ToList();
            return brandViewModels;
        }
    }
}
