using Application.Categories;
using Application.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DI
    {
        public static void AddService(this IServiceCollection services)
        {
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
        }
    }
}
