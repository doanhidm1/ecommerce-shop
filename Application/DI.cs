using Application.Brands;
using Application.Categories;
using Application.Checkout;
using Application.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DI
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IBrandService, BrandService>();
            services.AddScoped<IBillService, BillService>();
        }
    }
}
